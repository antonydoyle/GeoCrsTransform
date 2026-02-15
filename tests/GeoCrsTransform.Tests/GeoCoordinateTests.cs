using GeoCrsTransform;
using Xunit;

namespace GeoCrsTransform.Tests;

public class GeoCoordinateTests
{
    [Fact]
    public void Valid_coordinate_creates()
    {
        var c = new GeoCoordinate(51.5, -0.1);
        Assert.Equal(51.5, c.LatitudeDeg);
        Assert.Equal(-0.1, c.LongitudeDeg);
        Assert.Null(c.HeightMeters);
    }

    [Fact]
    public void With_height()
    {
        var c = new GeoCoordinate(0, 0, 100);
        Assert.Equal(100, c.HeightMeters);
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    public void Latitude_out_of_range_throws(double lat)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoCoordinate(lat, 0));
    }

    [Fact]
    public void Longitude_NaN_throws()
    {
        Assert.Throws<ArgumentException>(() => new GeoCoordinate(0, double.NaN));
    }

    [Fact]
    public void WithHeight_returns_new_with_height()
    {
        var c = new GeoCoordinate(1, 2).WithHeight(10);
        Assert.Equal(1, c.LatitudeDeg);
        Assert.Equal(2, c.LongitudeDeg);
        Assert.Equal(10, c.HeightMeters);
    }
}
