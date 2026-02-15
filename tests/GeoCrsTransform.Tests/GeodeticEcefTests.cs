using GeoCrsTransform;
using Xunit;

namespace GeoCrsTransform.Tests;

public class GeodeticEcefTests
{
    private static readonly Ellipsoid Wgs84 = Ellipsoid.Wgs84;

    [Fact]
    public void Round_trip_equator()
    {
        var geo = new GeoCoordinate(0, 0, 0);
        var ecef = GeodeticEcef.ToEcef(geo, Wgs84);
        var back = GeodeticEcef.FromEcef(ecef, Wgs84);
        Assert.Equal(geo.LatitudeDeg, back.LatitudeDeg, 10);
        Assert.Equal(geo.LongitudeDeg, back.LongitudeDeg, 10);
        Assert.NotNull(back.HeightMeters);
        Assert.Equal(0, back.HeightMeters!.Value, 1e-6);
    }

    [Fact]
    public void Round_trip_north_pole()
    {
        var geo = new GeoCoordinate(90, 0, 100);
        var ecef = GeodeticEcef.ToEcef(geo, Wgs84);
        Assert.True(ecef.X < 1e-6 && ecef.Y < 1e-6 && ecef.Z > 6e6);
        var back = GeodeticEcef.FromEcef(ecef, Wgs84);
        Assert.Equal(90, back.LatitudeDeg, 8);
        Assert.Equal(0, back.LongitudeDeg, 8);
        Assert.Equal(100, back.HeightMeters!.Value, 1e-3);
    }

    [Fact]
    public void Round_trip_south_pole()
    {
        var geo = new GeoCoordinate(-90, 0, 0);
        var ecef = GeodeticEcef.ToEcef(geo, Wgs84);
        var back = GeodeticEcef.FromEcef(ecef, Wgs84);
        Assert.Equal(-90, back.LatitudeDeg, 8);
    }

    [Fact]
    public void Round_trip_dateline()
    {
        var geo = new GeoCoordinate(51.5, 180, 0);
        var ecef = GeodeticEcef.ToEcef(geo, Wgs84);
        var back = GeodeticEcef.FromEcef(ecef, Wgs84);
        Assert.Equal(51.5, back.LatitudeDeg, 10);
        Assert.Equal(180, back.LongitudeDeg, 10);
    }

    [Fact]
    public void Round_trip_seeded_random_points()
    {
        var rng = new Random(12345);
        for (var i = 0; i < 100; i++)
        {
            var lat = (rng.NextDouble() * 180) - 90;
            var lon = (rng.NextDouble() * 360) - 180;
            var h = (rng.NextDouble() - 0.5) * 20000;
            var geo = new GeoCoordinate(lat, lon, h);
            var ecef = GeodeticEcef.ToEcef(geo, Wgs84);
            var back = GeodeticEcef.FromEcef(ecef, Wgs84);
            Assert.Equal(geo.LatitudeDeg, back.LatitudeDeg, 8);
            Assert.Equal(geo.LongitudeDeg, back.LongitudeDeg, 8);
            Assert.NotNull(back.HeightMeters);
            Assert.Equal(geo.HeightMeters!.Value, back.HeightMeters!.Value, 1e-3);
        }
    }
}
