using GeoCrsTransform;
using Xunit;

namespace GeoCrsTransform.Tests;

public class CatalogTests
{
    [Fact]
    public void StubCrsCatalog_TryGetByAlias_resolves_added_alias()
    {
        var catalog = new StubCrsCatalog();
        var wgs84 = new CrsDefinition(CrsId.Parse("EPSG:4326"), "WGS 84", CrsKind.Geographic);
        catalog.Add(wgs84, "WGS84", "EPSG:4326");

        Assert.True(catalog.TryGetByAlias("WGS84", out var id));
        Assert.Equal(4326, id.Code);

        Assert.True(catalog.TryGet(id, out var def));
        Assert.Same(wgs84, def);
    }

    [Fact]
    public void StubCrsCatalog_TryGetByAlias_returns_false_for_unknown()
    {
        var catalog = new StubCrsCatalog();
        Assert.False(catalog.TryGetByAlias("UnknownCRS", out _));
        Assert.False(catalog.TryGetByAlias("", out _));
    }

    [Fact]
    public void StubCrsCatalog_ListMajor_returns_added_ids()
    {
        var catalog = new StubCrsCatalog();
        catalog.Add(new CrsDefinition(CrsId.Parse("EPSG:4326"), "WGS 84", CrsKind.Geographic), "WGS84");
        catalog.Add(new CrsDefinition(CrsId.Parse("EPSG:3857"), "Web Mercator", CrsKind.Projected), "WebMercator");
        var list = catalog.ListMajor();
        Assert.Equal(2, list.Count);
        Assert.Contains(list, c => c.Code == 4326);
        Assert.Contains(list, c => c.Code == 3857);
    }
}
