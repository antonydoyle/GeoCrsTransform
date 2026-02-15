using System.Reflection;
using System.Text.Json;

namespace GeoCrsTransform;

/// <summary>CRS catalog loaded from embedded major_crs.json.</summary>
public sealed class CrsCatalog : ICrsCatalog
{
    private readonly Dictionary<CrsId, CrsDefinition> _byId = new();
    private readonly Dictionary<string, CrsId> _byAlias = new(StringComparer.OrdinalIgnoreCase);

    public static CrsCatalog LoadFromEmbedded()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var name = assembly.GetName().Name + ".Data.major_crs.json";
        using var stream = assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Embedded resource not found: {name}");
        return Load(stream);
    }

    public static CrsCatalog Load(Stream stream)
    {
        var doc = JsonDocument.Parse(stream);
        var root = doc.RootElement;
        var ellipsoids = ParseEllipsoids(root.GetProperty("ellipsoids"));
        var catalog = new CrsCatalog();
        var geoList = root.GetProperty("geographic");
        foreach (var je in geoList.EnumerateArray())
            catalog.AddGeographic(je, ellipsoids);
        var projList = root.GetProperty("projected");
        foreach (var je in projList.EnumerateArray())
            catalog.AddProjected(je, ellipsoids);
        return catalog;
    }

    private static Dictionary<string, Ellipsoid> ParseEllipsoids(JsonElement el)
    {
        var d = new Dictionary<string, Ellipsoid>(StringComparer.OrdinalIgnoreCase);
        foreach (var prop in el.EnumerateObject())
        {
            var a = prop.Value.GetProperty("a").GetDouble();
            var invF = prop.Value.GetProperty("invF").GetDouble();
            d[prop.Name] = new Ellipsoid(a, invF);
        }
        return d;
    }

    private void AddGeographic(JsonElement je, Dictionary<string, Ellipsoid> ellipsoids)
    {
        var id = CrsId.Parse(je.GetProperty("id").GetString()!);
        var name = je.GetProperty("name").GetString() ?? "";
        var ellKey = je.GetProperty("ellipsoid").GetString()!;
        if (!ellipsoids.TryGetValue(ellKey, out var ell))
            throw new InvalidOperationException($"Ellipsoid '{ellKey}' not found for CRS {id}.");
        var toWgs84 = ParseOptionalHelmert(je, "toWgs84");
        var fromWgs84 = ParseOptionalHelmert(je, "fromWgs84") ?? toWgs84;
        var accuracy = ParseAccuracy(je);
        var warnings = ParseWarnings(je);
        var def = new CrsDefinition(id, name, CrsKind.Geographic, ell, toWgs84, fromWgs84, null, accuracy, warnings);
        if (_byId.ContainsKey(id))
            throw new InvalidOperationException($"Duplicate CRS id: {id}.");
        _byId[id] = def;
        AddAliases(je, id);
    }

    private void AddProjected(JsonElement je, Dictionary<string, Ellipsoid> ellipsoids)
    {
        var id = CrsId.Parse(je.GetProperty("id").GetString()!);
        var name = je.GetProperty("name").GetString() ?? "";
        var baseId = CrsId.Parse(je.GetProperty("base").GetString()!);
        if (!_byId.TryGetValue(baseId, out var baseDef) || baseDef!.Kind != CrsKind.Geographic)
            throw new InvalidOperationException($"Base geographic CRS '{baseId}' for projected {id} not found or not geographic.");
        var projKindStr = je.GetProperty("projection").GetString()!;
        var kind = projKindStr switch
        {
            "WebMercator" => ProjectionKind.WebMercator,
            "TransverseMercator" => ProjectionKind.TransverseMercator,
            "LambertConformalConic" => ProjectionKind.LambertConformalConic,
            _ => throw new InvalidOperationException($"Unknown projection: {projKindStr}")
        };
        var cm = je.TryGetProperty("centralMeridian", out var cmEl) ? cmEl.GetDouble() : 0;
        var lo = je.TryGetProperty("latitudeOfOrigin", out var loEl) ? loEl.GetDouble() : 0;
        var k0 = je.TryGetProperty("scaleFactor", out var k0El) ? k0El.GetDouble() : 1.0;
        var fe = je.TryGetProperty("falseEasting", out var feEl) ? feEl.GetDouble() : 0;
        var fn = je.TryGetProperty("falseNorthing", out var fnEl) ? fnEl.GetDouble() : 0;
        var proj = new ProjectionDefinition(kind, baseId, cm, lo, k0, fe, fn);
        var accuracy = ParseAccuracy(je);
        var warnings = ParseWarnings(je);
        var def = new CrsDefinition(id, name, CrsKind.Projected, null, null, null, proj, accuracy, warnings);
        if (_byId.ContainsKey(id))
            throw new InvalidOperationException($"Duplicate CRS id: {id}.");
        _byId[id] = def;
        AddAliases(je, id);
    }

    private static DatumTransform? ParseOptionalHelmert(JsonElement je, string prop)
    {
        if (!je.TryGetProperty(prop, out var el) || el.ValueKind == JsonValueKind.Null)
            return null;
        var tx = el.GetProperty("tx").GetDouble();
        var ty = el.GetProperty("ty").GetDouble();
        var tz = el.GetProperty("tz").GetDouble();
        var rx = el.GetProperty("rx").GetDouble();
        var ry = el.GetProperty("ry").GetDouble();
        var rz = el.GetProperty("rz").GetDouble();
        var scale = el.GetProperty("scalePpm").GetDouble();
        return new DatumTransform(tx, ty, tz, rx, ry, rz, scale);
    }

    private static AccuracyClass ParseAccuracy(JsonElement je)
    {
        if (!je.TryGetProperty("accuracy", out var el))
            return AccuracyClass.Unknown;
        return el.GetString() switch
        {
            "High" => AccuracyClass.High,
            "Medium" => AccuracyClass.Medium,
            "Low" => AccuracyClass.Low,
            _ => AccuracyClass.Unknown
        };
    }

    private static IReadOnlyList<string> ParseWarnings(JsonElement je)
    {
        if (!je.TryGetProperty("warnings", out var arr))
            return Array.Empty<string>();
        var list = new List<string>();
        foreach (var e in arr.EnumerateArray())
            list.Add(e.GetString() ?? "");
        return list;
    }

    private void AddAliases(JsonElement je, CrsId id)
    {
        void AddAlias(string alias)
        {
            if (string.IsNullOrWhiteSpace(alias)) return;
            var key = alias.Trim();
            if (_byAlias.TryGetValue(key, out var existing) && existing != id)
                throw new InvalidOperationException($"Duplicate alias '{key}' for {id} (already used for {existing}).");
            _byAlias[key] = id;
        }
        if (je.TryGetProperty("aliases", out var arr))
        {
            foreach (var e in arr.EnumerateArray())
                AddAlias(e.GetString() ?? "");
        }
        AddAlias(id.ToString());
    }

    public bool TryGet(CrsId id, out CrsDefinition? definition) => _byId.TryGetValue(id, out definition);

    public bool TryGetByAlias(string alias, out CrsId id)
    {
        if (string.IsNullOrWhiteSpace(alias))
        {
            id = default;
            return false;
        }
        return _byAlias.TryGetValue(alias.Trim(), out id);
    }

    public IReadOnlyList<CrsId> ListMajor() => _byId.Keys.ToList();
}
