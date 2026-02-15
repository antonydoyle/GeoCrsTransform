using GeoCrsTransform;
using Xunit;

namespace GeoCrsTransform.Tests;

public class HelmertTests
{
    [Fact]
    public void Identity_transform_leaves_point_unchanged()
    {
        var t = DatumTransform.Identity;
        var p = new Vector3d(1e6, 2e6, 3e6);
        var toWgs = Helmert.ApplyToWgs84(p, t);
        Assert.Equal(p.X, toWgs.X, 1e-9);
        Assert.Equal(p.Y, toWgs.Y, 1e-9);
        Assert.Equal(p.Z, toWgs.Z, 1e-9);
        var fromWgs = Helmert.ApplyFromWgs84(p, t);
        Assert.Equal(p.X, fromWgs.X, 1e-9);
        Assert.Equal(p.Y, fromWgs.Y, 1e-9);
        Assert.Equal(p.Z, fromWgs.Z, 1e-9);
    }

    [Fact]
    public void Translation_only_ToWgs84_adds_offset()
    {
        var t = new DatumTransform(10, 20, 30, 0, 0, 0, 0);
        var p = new Vector3d(100, 200, 300);
        var q = Helmert.ApplyToWgs84(p, t);
        Assert.Equal(110, q.X, 1e-9);
        Assert.Equal(220, q.Y, 1e-9);
        Assert.Equal(330, q.Z, 1e-9);
    }

    [Fact]
    public void Translation_only_FromWgs84_subtracts_offset()
    {
        var t = new DatumTransform(10, 20, 30, 0, 0, 0, 0);
        var p = new Vector3d(110, 220, 330);
        var q = Helmert.ApplyFromWgs84(p, t);
        Assert.Equal(100, q.X, 1e-9);
        Assert.Equal(200, q.Y, 1e-9);
        Assert.Equal(300, q.Z, 1e-9);
    }

    [Fact]
    public void Round_trip_ToWgs84_FromWgs84_identity()
    {
        var t = new DatumTransform(1, 2, 3, 0.5, -0.3, 0.1, 0.2);
        var p = new Vector3d(4e6, 2e6, 5e6);
        var wgs = Helmert.ApplyToWgs84(p, t);
        var back = Helmert.ApplyFromWgs84(wgs, t);
        Assert.Equal(p.X, back.X, 1e-4);
        Assert.Equal(p.Y, back.Y, 1e-4);
        Assert.Equal(p.Z, back.Z, 1e-4);
    }
}
