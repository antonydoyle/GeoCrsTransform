namespace GeoCrsTransform;

/// <summary>Reference ellipsoid (semi-major axis and inverse flattening).</summary>
public sealed class Ellipsoid
{
    /// <summary>Semi-major axis in meters.</summary>
    public double SemiMajorAxisMeters { get; }

    /// <summary>Inverse flattening (1/f).</summary>
    public double InverseFlattening { get; }

    /// <summary>Semi-minor axis (derived).</summary>
    public double SemiMinorAxisMeters { get; }

    /// <summary>First eccentricity squared (derived).</summary>
    public double EccentricitySq { get; }

    public Ellipsoid(double semiMajorAxisMeters, double inverseFlattening)
    {
        SemiMajorAxisMeters = semiMajorAxisMeters;
        InverseFlattening = inverseFlattening;
        var f = 1.0 / inverseFlattening;
        SemiMinorAxisMeters = semiMajorAxisMeters * (1.0 - f);
        EccentricitySq = 2 * f - f * f;
    }

    public static Ellipsoid Wgs84 { get; } = new Ellipsoid(6_378_137.0, 298.257223563);
}
