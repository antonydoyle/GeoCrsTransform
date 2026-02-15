using System.Text;
using GeoCrsTransform;
using Xunit;

namespace GeoCrsTransform.Tests;

public class CrsCatalogLoadTests
{
    [Fact]
    public void LoadFromEmbedded_loads_and_contains_WGS84_and_WebMercator()
    {
        var catalog = CrsCatalog.LoadFromEmbedded();
        Assert.True(catalog.TryGet(CrsId.Parse("EPSG:4326"), out var wgs84));
        Assert.NotNull(wgs84);
        Assert.Equal(CrsKind.Geographic, wgs84!.Kind);
        Assert.NotNull(wgs84.Ellipsoid);

        Assert.True(catalog.TryGet(CrsId.Parse("EPSG:3857"), out var webMercator));
        Assert.NotNull(webMercator);
        Assert.Equal(CrsKind.Projected, webMercator!.Kind);
        Assert.NotNull(webMercator.Projection);
        Assert.Equal(ProjectionKind.WebMercator, webMercator.Projection.Kind);
        Assert.Equal(4326, webMercator.Projection.BaseGeographicCrsId.Code);
    }

    [Fact]
    public void Load_resolves_aliases()
    {
        var catalog = CrsCatalog.LoadFromEmbedded();
        Assert.True(catalog.TryGetByAlias("WGS84", out var id));
        Assert.Equal(4326, id.Code);
        Assert.True(catalog.TryGetByAlias("Web Mercator", out id));
        Assert.Equal(3857, id.Code);
    }

    [Fact]
    public void Load_duplicate_id_throws()
    {
        var json = """
            {"ellipsoids":{"WGS84":{"a":6378137,"invF":298.257223563}},
             "geographic":[
               {"id":"EPSG:4326","name":"A","aliases":[],"ellipsoid":"WGS84","accuracy":"High","warnings":[]},
               {"id":"EPSG:4326","name":"B","aliases":[],"ellipsoid":"WGS84","accuracy":"High","warnings":[]}
             ],
             "projected":[]}
            """;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        Assert.Throws<InvalidOperationException>(() => CrsCatalog.Load(stream));
    }

    [Fact]
    public void Load_projected_with_missing_base_throws()
    {
        var json = """
            {"ellipsoids":{"WGS84":{"a":6378137,"invF":298.257223563}},
             "geographic":[],
             "projected":[
               {"id":"EPSG:3857","name":"Web Mercator","aliases":[],"base":"EPSG:4326","projection":"WebMercator","accuracy":"High","warnings":[]}
             ]}
            """;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        Assert.Throws<InvalidOperationException>(() => CrsCatalog.Load(stream));
    }
}
