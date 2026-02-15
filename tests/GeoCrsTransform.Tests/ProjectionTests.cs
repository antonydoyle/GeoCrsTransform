using GeoCrsTransform;
using Xunit;

namespace GeoCrsTransform.Tests;

public class ProjectionTests
{
    private static readonly Ellipsoid Wgs84 = Ellipsoid.Wgs84;

    [Fact]
    public void WebMercator_round_trip()
    {
        var def = new ProjectionDefinition(ProjectionKind.WebMercator, CrsId.Parse("EPSG:4326"));
        var geo = new GeoCoordinate(51.5074, -0.1278, 0);
        var proj = ProjectionEngine.Project(geo, def, Wgs84);
        var back = ProjectionEngine.Unproject(proj, def, Wgs84);
        Assert.Equal(geo.LatitudeDeg, back.LatitudeDeg, 8);
        Assert.Equal(geo.LongitudeDeg, back.LongitudeDeg, 8);
    }

    [Fact]
    public void WebMercator_known_point()
    {
        var def = new ProjectionDefinition(ProjectionKind.WebMercator, CrsId.Parse("EPSG:4326"));
        var geo = new GeoCoordinate(0, 0, 0);
        var proj = ProjectionEngine.Project(geo, def, Wgs84);
        Assert.Equal(0, proj.EastingMeters, 0.1);
        Assert.Equal(0, proj.NorthingMeters, 0.1);
    }

    [Fact]
    public void TransverseMercator_round_trip()
    {
        var def = new ProjectionDefinition(
            ProjectionKind.TransverseMercator,
            CrsId.Parse("EPSG:4326"),
            centralMeridianDeg: -3,
            latitudeOfOriginDeg: 0,
            scaleFactor: 0.9996,
            falseEastingMeters: 500000,
            falseNorthingMeters: 0);
        var geo = new GeoCoordinate(51.5, -2.0, 0);
        var proj = ProjectionEngine.Project(geo, def, Wgs84);
        var back = ProjectionEngine.Unproject(proj, def, Wgs84);
        Assert.Equal(geo.LatitudeDeg, back.LatitudeDeg, 1);
        Assert.Equal(geo.LongitudeDeg, back.LongitudeDeg, 1);
    }
}
