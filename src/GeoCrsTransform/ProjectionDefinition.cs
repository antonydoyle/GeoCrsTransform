namespace GeoCrsTransform;

/// <summary>Definition of a map projection (parameters and base geographic CRS).</summary>
public sealed class ProjectionDefinition
{
    public ProjectionKind Kind { get; }
    public CrsId BaseGeographicCrsId { get; }
    public double CentralMeridianDeg { get; }
    public double LatitudeOfOriginDeg { get; }
    public double ScaleFactor { get; }
    public double FalseEastingMeters { get; }
    public double FalseNorthingMeters { get; }

    public ProjectionDefinition(
        ProjectionKind kind,
        CrsId baseGeographicCrsId,
        double centralMeridianDeg = 0,
        double latitudeOfOriginDeg = 0,
        double scaleFactor = 1.0,
        double falseEastingMeters = 0,
        double falseNorthingMeters = 0)
    {
        Kind = kind;
        BaseGeographicCrsId = baseGeographicCrsId;
        CentralMeridianDeg = centralMeridianDeg;
        LatitudeOfOriginDeg = latitudeOfOriginDeg;
        ScaleFactor = scaleFactor;
        FalseEastingMeters = falseEastingMeters;
        FalseNorthingMeters = falseNorthingMeters;
    }
}

/// <summary>Supported projection types.</summary>
public enum ProjectionKind
{
    TransverseMercator,
    WebMercator,
    LambertConformalConic
}
