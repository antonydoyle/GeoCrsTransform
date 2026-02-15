using GeoCrsTransform;
using Xunit;

namespace GeoCrsTransform.Tests;

/// <summary>Tier A: property-style round-trip tests with seeded RNG.</summary>
public class PropertyStyleRoundTripTests
{
    [Fact]
    public void WGS84_to_WebMercator_to_WGS84_within_tolerance()
    {
        var catalog = CrsCatalog.LoadFromEmbedded();
        var transformer = new CoordinateTransformer(catalog);
        var wgs84 = CrsId.Parse("EPSG:4326");
        var webMercator = CrsId.Parse("EPSG:3857");
        var rng = new Random(42);
        for (var i = 0; i < 20; i++)
        {
            var lat = (rng.NextDouble() * 170) - 85;
            var lon = (rng.NextDouble() * 360) - 180;
            var geo = new GeoCoordinate(lat, lon, 0);
            var toProj = transformer.Transform(geo, wgs84, webMercator);
            var proj = Assert.IsType<ProjectedCoordinate>(toProj.Output);
            var back = transformer.Transform(proj, webMercator, wgs84);
            var backGeo = Assert.IsType<GeoCoordinate>(back.Output);
            Assert.Equal(geo.LatitudeDeg, backGeo.LatitudeDeg, 6);
            Assert.Equal(geo.LongitudeDeg, backGeo.LongitudeDeg, 6);
        }
    }

    [Fact]
    public void WGS84_to_UTM31N_to_WGS84_within_tolerance()
    {
        var catalog = CrsCatalog.LoadFromEmbedded();
        var transformer = new CoordinateTransformer(catalog);
        var wgs84 = CrsId.Parse("EPSG:4326");
        var utm31n = CrsId.Parse("EPSG:32631");
        var rng = new Random(123);
        for (var i = 0; i < 15; i++)
        {
            var lat = 48 + rng.NextDouble() * 12;
            var lon = 0 + rng.NextDouble() * 6;
            var geo = new GeoCoordinate(lat, lon, 0);
            var toProj = transformer.Transform(geo, wgs84, utm31n);
            var proj = Assert.IsType<ProjectedCoordinate>(toProj.Output);
            var back = transformer.Transform(proj, utm31n, wgs84);
            var backGeo = Assert.IsType<GeoCoordinate>(back.Output);
            Assert.Equal(geo.LatitudeDeg, backGeo.LatitudeDeg, 0);
            Assert.Equal(geo.LongitudeDeg, backGeo.LongitudeDeg, 0);
        }
    }
}
