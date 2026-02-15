using GeoCrsTransform;
using Xunit;

namespace GeoCrsTransform.Tests;

public class CrsIdTests
{
    [Theory]
    [InlineData("EPSG:4326", "EPSG", 4326)]
    [InlineData("epsg:27700", "epsg", 27700)]
    [InlineData("  EPSG:0  ", "EPSG", 0)]
    public void TryParse_success(string value, string expectedAuthority, int expectedCode)
    {
        Assert.True(CrsId.TryParse(value, out var id));
        Assert.Equal(expectedAuthority, id.Authority);
        Assert.Equal(expectedCode, id.Code);
        Assert.Equal(value.Trim(), id.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("EPSG")]
    [InlineData(":4326")]
    [InlineData("EPSG:")]
    [InlineData("EPSG:abc")]
    [InlineData("EPSG:-1")]
    public void TryParse_failure(string? value)
    {
        Assert.False(CrsId.TryParse(value, out var id));
        Assert.Equal(0, id.Code);
    }

    [Fact]
    public void Parse_valid_returns_id()
    {
        var id = CrsId.Parse("EPSG:4326");
        Assert.Equal("EPSG", id.Authority);
        Assert.Equal(4326, id.Code);
    }

    [Fact]
    public void Parse_invalid_throws()
    {
        Assert.Throws<FormatException>(() => CrsId.Parse("invalid"));
    }

    [Fact]
    public void Equality_ignores_authority_case()
    {
        var a = new CrsId("EPSG", 4326);
        var b = new CrsId("epsg", 4326);
        Assert.True(a.Equals(b));
        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }
}
