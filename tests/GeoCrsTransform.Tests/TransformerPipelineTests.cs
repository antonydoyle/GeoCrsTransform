using GeoCrsTransform;
using Xunit;

namespace GeoCrsTransform.Tests;

public class TransformerPipelineTests
{
    [Fact]
    public void WGS84_to_WebMercator_and_back()
    {
        var catalog = CrsCatalog.LoadFromEmbedded();
        var transformer = new CoordinateTransformer(catalog);
        var wgs84 = CrsId.Parse("EPSG:4326");
        var webMercator = CrsId.Parse("EPSG:3857");
        var geo = new GeoCoordinate(51.5074, -0.1278, 0);

        var toProj = transformer.Transform(geo, wgs84, webMercator);
        Assert.NotNull(toProj.Output);
        var projCoord = Assert.IsType<ProjectedCoordinate>(toProj.Output);
        Assert.Contains("EPSG:3857", toProj.TransformPath);
        Assert.Equal(AccuracyClass.High, toProj.AccuracyClass);

        var back = transformer.Transform(projCoord, webMercator, wgs84);
        var backGeo = Assert.IsType<GeoCoordinate>(back.Output);
        Assert.Equal(geo.LatitudeDeg, backGeo.LatitudeDeg, 6);
        Assert.Equal(geo.LongitudeDeg, backGeo.LongitudeDeg, 6);
    }

    [Fact]
    public void WGS84_to_WGS84_identity()
    {
        var catalog = CrsCatalog.LoadFromEmbedded();
        var transformer = new CoordinateTransformer(catalog);
        var wgs84 = CrsId.Parse("EPSG:4326");
        var geo = new GeoCoordinate(51.5, -0.1, 10);

        var result = transformer.Transform(geo, wgs84, wgs84);
        var outGeo = Assert.IsType<GeoCoordinate>(result.Output);
        Assert.Equal(geo.LatitudeDeg, outGeo.LatitudeDeg, 10);
        Assert.Equal(geo.LongitudeDeg, outGeo.LongitudeDeg, 10);
        Assert.Equal(10, outGeo.HeightMeters!.Value, 1e-6);
    }

    [Fact]
    public void Alias_resolution_WGS84()
    {
        var catalog = CrsCatalog.LoadFromEmbedded();
        var transformer = new CoordinateTransformer(catalog);
        var geo = new GeoCoordinate(0, 0, 0);
        var result = transformer.Transform(geo, CrsId.Parse("EPSG:4326"), CrsId.Parse("EPSG:4326"));
        Assert.NotNull(result.Output);
    }

    [Fact]
    public void WGS84_to_BNG_and_back_has_warning()
    {
        var catalog = CrsCatalog.LoadFromEmbedded();
        var transformer = new CoordinateTransformer(catalog);
        var wgs84 = CrsId.Parse("EPSG:4326");
        var bng = CrsId.Parse("EPSG:27700");
        var geo = new GeoCoordinate(51.5, -2.0, 0);

        var toBng = transformer.Transform(geo, wgs84, bng);
        var proj = Assert.IsType<ProjectedCoordinate>(toBng.Output);
        Assert.True(proj.EastingMeters > 300000 && proj.EastingMeters < 500000);
        Assert.Contains("Grid shift", string.Join(" ", toBng.Warnings));

        var back = transformer.Transform(proj, bng, wgs84);
        var backGeo = Assert.IsType<GeoCoordinate>(back.Output);
        Assert.Equal(geo.LatitudeDeg, backGeo.LatitudeDeg, 1);
        Assert.Equal(geo.LongitudeDeg, backGeo.LongitudeDeg, 1);
    }

    [Fact]
    public void UTM_zone_30N_round_trip()
    {
        var catalog = CrsCatalog.LoadFromEmbedded();
        var transformer = new CoordinateTransformer(catalog);
        var wgs84 = CrsId.Parse("EPSG:4326");
        var utm30n = CrsId.Parse("EPSG:32630");
        var geo = new GeoCoordinate(51.5, -2.0, 0);

        var toUtm = transformer.Transform(geo, wgs84, utm30n);
        var proj = Assert.IsType<ProjectedCoordinate>(toUtm.Output);
        var back = transformer.Transform(proj, utm30n, wgs84);
        var backGeo = Assert.IsType<GeoCoordinate>(back.Output);
        Assert.Equal(geo.LatitudeDeg, backGeo.LatitudeDeg, 1);
        Assert.Equal(geo.LongitudeDeg, backGeo.LongitudeDeg, 1);
    }

    [Fact]
    public void Projected_to_projected_27700_to_3857()
    {
        var catalog = CrsCatalog.LoadFromEmbedded();
        var transformer = new CoordinateTransformer(catalog);
        var bng = CrsId.Parse("EPSG:27700");
        var webMercator = CrsId.Parse("EPSG:3857");
        var bngCoord = new ProjectedCoordinate(400000, 200000, 0);

        var result = transformer.Transform(bngCoord, bng, webMercator);
        var proj = Assert.IsType<ProjectedCoordinate>(result.Output);
        Assert.Contains("EPSG:4326", result.TransformPath);
        Assert.Contains("EPSG:3857", result.TransformPath);
    }
}
