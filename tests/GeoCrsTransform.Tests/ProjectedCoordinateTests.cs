using GeoCrsTransform;
using Xunit;

namespace GeoCrsTransform.Tests;

public class ProjectedCoordinateTests
{
    [Fact]
    public void Valid_coordinate_creates()
    {
        var c = new ProjectedCoordinate(500000, 200000);
        Assert.Equal(500000, c.EastingMeters);
        Assert.Equal(200000, c.NorthingMeters);
        Assert.Null(c.HeightMeters);
    }

    [Fact]
    public void Easting_NaN_throws()
    {
        Assert.Throws<ArgumentException>(() => new ProjectedCoordinate(double.NaN, 0));
    }

    [Fact]
    public void Northing_Infinity_throws()
    {
        Assert.Throws<ArgumentException>(() => new ProjectedCoordinate(0, double.PositiveInfinity));
    }
}
