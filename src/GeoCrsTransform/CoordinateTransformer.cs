namespace GeoCrsTransform;

/// <summary>Transforms coordinates between CRSs using the catalog and WGS84 hub.</summary>
public sealed class CoordinateTransformer : ICoordinateTransformer
{
    private static readonly CrsId Wgs84Id = CrsId.Parse("EPSG:4326");
    private readonly ICrsCatalog _catalog;

    public CoordinateTransformer(ICrsCatalog catalog)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
    }

    public TransformResult<object> Transform(GeoCoordinate input, CrsId sourceCrs, CrsId destCrs, TransformOptions? options = null)
    {
        var srcId = Resolve(sourceCrs);
        var dstId = Resolve(destCrs);
        if (!_catalog.TryGet(srcId, out var srcDef) || srcDef == null)
            return TransformResult<object>.Error($"Source CRS not found: {sourceCrs}", $"{sourceCrs} -> {destCrs}");
        if (!_catalog.TryGet(dstId, out var dstDef) || dstDef == null)
            return TransformResult<object>.Error($"Destination CRS not found: {destCrs}", $"{sourceCrs} -> {destCrs}");
        if (srcDef.Kind != CrsKind.Geographic)
            return TransformResult<object>.Error("Source must be geographic.", $"{sourceCrs} -> {destCrs}");

        var path = new List<string> { srcId.ToString() };
        var warnings = new List<string>(srcDef.WarningsByDefault ?? Array.Empty<string>());
        var acc = srcDef.AccuracyClass;

        if (dstDef.Kind == CrsKind.Geographic)
        {
            path.Add(dstId.ToString());
            warnings.AddRange(dstDef.WarningsByDefault ?? Array.Empty<string>());
            acc = Min(acc, dstDef.AccuracyClass);
            var ecef = GeodeticEcef.ToEcef(input, srcDef.Ellipsoid!);
            if (srcDef.ToWgs84 != null)
                ecef = Helmert.ApplyToWgs84(ecef, srcDef.ToWgs84);
            if (dstDef.FromWgs84 != null)
                ecef = Helmert.ApplyFromWgs84(ecef, dstDef.FromWgs84);
            var outGeo = GeodeticEcef.FromEcef(ecef, dstDef.Ellipsoid!);
            return new TransformResult<object>(outGeo, warnings, acc, string.Join(" -> ", path));
        }

        if (dstDef.Kind != CrsKind.Projected || dstDef.Projection == null)
            return TransformResult<object>.Error("Destination must be geographic or projected.", $"{sourceCrs} -> {destCrs}");
        if (!_catalog.TryGet(dstDef.Projection.BaseGeographicCrsId, out var baseDef) || baseDef?.Ellipsoid == null)
            return TransformResult<object>.Error($"Base CRS not found for {destCrs}", $"{sourceCrs} -> {destCrs}");

        var geoInBase = TransformGeoToGeo(input, srcId, dstDef.Projection.BaseGeographicCrsId, ref path, ref warnings, ref acc);
        path.Add(dstId.ToString());
        warnings.AddRange(dstDef.WarningsByDefault ?? Array.Empty<string>());
        acc = Min(acc, dstDef.AccuracyClass);
        var projOut = ProjectionEngine.Project(geoInBase, dstDef.Projection, baseDef.Ellipsoid);
        return new TransformResult<object>(projOut, warnings, acc, string.Join(" -> ", path));
    }

    public TransformResult<object> Transform(ProjectedCoordinate input, CrsId sourceCrs, CrsId destCrs, TransformOptions? options = null)
    {
        var srcId = Resolve(sourceCrs);
        var dstId = Resolve(destCrs);
        if (!_catalog.TryGet(srcId, out var srcDef) || srcDef == null)
            return TransformResult<object>.Error($"Source CRS not found: {sourceCrs}", $"{sourceCrs} -> {destCrs}");
        if (!_catalog.TryGet(dstId, out var dstDef) || dstDef == null)
            return TransformResult<object>.Error($"Destination CRS not found: {destCrs}", $"{sourceCrs} -> {destCrs}");

        var path = new List<string> { srcId.ToString() };
        var warnings = new List<string>(srcDef.WarningsByDefault ?? Array.Empty<string>());
        var acc = srcDef.AccuracyClass;

        if (srcDef.Kind != CrsKind.Projected || srcDef.Projection == null)
            return TransformResult<object>.Error($"Source CRS must be projected: {sourceCrs}", $"{sourceCrs} -> {destCrs}");

        if (!_catalog.TryGet(srcDef.Projection.BaseGeographicCrsId, out var baseGeoDef) || baseGeoDef?.Ellipsoid == null)
            return TransformResult<object>.Error($"Base geographic CRS not found for {sourceCrs}", $"{sourceCrs} -> {destCrs}");

        var geoBase = ProjectionEngine.Unproject(input, srcDef.Projection, baseGeoDef.Ellipsoid);
        path.Add(baseGeoDef.Id.ToString());

        if (dstDef.Kind == CrsKind.Geographic)
        {
            var geoResult = TransformGeoToGeo(geoBase, baseGeoDef.Id, dstId, ref path, ref warnings, ref acc);
            path.Add(dstId.ToString());
            return new TransformResult<object>(geoResult, warnings, acc, string.Join(" -> ", path));
        }

        if (dstDef.Kind != CrsKind.Projected || dstDef.Projection == null)
            return TransformResult<object>.Error($"Destination CRS must be geographic or projected: {destCrs}", $"{sourceCrs} -> {destCrs}");

        if (!_catalog.TryGet(dstDef.Projection.BaseGeographicCrsId, out var dstBaseDef) || dstBaseDef?.Ellipsoid == null)
            return TransformResult<object>.Error($"Base geographic CRS not found for {destCrs}", $"{sourceCrs} -> {destCrs}");

        var geoDst = TransformGeoToGeo(geoBase, baseGeoDef.Id, dstDef.Projection.BaseGeographicCrsId, ref path, ref warnings, ref acc);
        path.Add(dstId.ToString());
        warnings.AddRange(dstDef.WarningsByDefault ?? Array.Empty<string>());
        acc = Min(acc, dstDef.AccuracyClass);
        var projOut = ProjectionEngine.Project(geoDst, dstDef.Projection, dstBaseDef.Ellipsoid);
        return new TransformResult<object>(projOut, warnings, acc, string.Join(" -> ", path));
    }

    private GeoCoordinate TransformGeoToGeo(GeoCoordinate geo, CrsId srcId, CrsId dstId, ref List<string> path, ref List<string> warnings, ref AccuracyClass acc)
    {
        if (srcId == dstId)
            return geo;
        if (!_catalog.TryGet(srcId, out var srcDef) || srcDef?.Ellipsoid == null)
            throw new InvalidOperationException($"CRS not found: {srcId}");
        if (!_catalog.TryGet(dstId, out var dstDef) || dstDef?.Ellipsoid == null)
            throw new InvalidOperationException($"CRS not found: {dstId}");

        path.Add(dstId.ToString());
        warnings.AddRange(dstDef.WarningsByDefault ?? Array.Empty<string>());
        acc = Min(acc, dstDef.AccuracyClass);

        var ecef = GeodeticEcef.ToEcef(geo, srcDef.Ellipsoid);
        if (srcDef.ToWgs84 != null)
            ecef = Helmert.ApplyToWgs84(ecef, srcDef.ToWgs84);
        if (dstDef.FromWgs84 != null)
            ecef = Helmert.ApplyFromWgs84(ecef, dstDef.FromWgs84);
        return GeodeticEcef.FromEcef(ecef, dstDef.Ellipsoid);
    }

    private static AccuracyClass Min(AccuracyClass a, AccuracyClass b)
    {
        if (a == AccuracyClass.Unknown || b == AccuracyClass.Unknown) return AccuracyClass.Unknown;
        if (a == AccuracyClass.Low || b == AccuracyClass.Low) return AccuracyClass.Low;
        if (a == AccuracyClass.Medium || b == AccuracyClass.Medium) return AccuracyClass.Medium;
        return AccuracyClass.High;
    }

    private CrsId Resolve(CrsId id)
    {
        if (_catalog.TryGet(id, out _))
            return id;
        if (_catalog.TryGetByAlias(id.ToString(), out var resolved))
            return resolved;
        return id;
    }
}
