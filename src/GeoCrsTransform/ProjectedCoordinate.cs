namespace GeoCrsTransform;

/// <summary>Projected coordinate (easting, northing in meters, optional height).</summary>
public readonly struct ProjectedCoordinate
{
    public double EastingMeters { get; }
    public double NorthingMeters { get; }
    public double? HeightMeters { get; }

    public ProjectedCoordinate(double eastingMeters, double northingMeters, double? heightMeters = null)
    {
        EastingMeters = eastingMeters;
        NorthingMeters = northingMeters;
        HeightMeters = heightMeters;
        if (double.IsNaN(eastingMeters) || double.IsInfinity(eastingMeters))
            throw new ArgumentException("Easting must be finite.", nameof(eastingMeters));
        if (double.IsNaN(northingMeters) || double.IsInfinity(northingMeters))
            throw new ArgumentException("Northing must be finite.", nameof(northingMeters));
        if (HeightMeters is { } h && (double.IsNaN(h) || double.IsInfinity(h)))
            throw new ArgumentException("Height must be finite when provided.", nameof(HeightMeters));
    }

    public ProjectedCoordinate WithHeight(double? heightMeters) => new ProjectedCoordinate(EastingMeters, NorthingMeters, heightMeters);
}
