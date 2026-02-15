namespace GeoCrsTransform;

/// <summary>Geographic coordinate (latitude, longitude, optional height).</summary>
public readonly struct GeoCoordinate
{
    public double LatitudeDeg { get; }
    public double LongitudeDeg { get; }
    public double? HeightMeters { get; }

    public GeoCoordinate(double latitudeDeg, double longitudeDeg, double? heightMeters = null)
    {
        LatitudeDeg = latitudeDeg;
        LongitudeDeg = longitudeDeg;
        HeightMeters = heightMeters;
        Validate();
    }

    private void Validate()
    {
        if (LatitudeDeg < -90 || LatitudeDeg > 90)
            throw new ArgumentOutOfRangeException(nameof(LatitudeDeg), LatitudeDeg, "Latitude must be in [-90, 90].");
        if (double.IsNaN(LongitudeDeg) || double.IsInfinity(LongitudeDeg))
            throw new ArgumentException("Longitude must be finite.", nameof(LongitudeDeg));
        if (HeightMeters is { } h && (double.IsNaN(h) || double.IsInfinity(h)))
            throw new ArgumentException("Height must be finite when provided.", nameof(HeightMeters));
    }

    public GeoCoordinate WithHeight(double? heightMeters) => new GeoCoordinate(LatitudeDeg, LongitudeDeg, heightMeters);
}
