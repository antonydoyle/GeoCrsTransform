namespace GeoCrsTransform;

/// <summary>Options for coordinate transformation.</summary>
public class TransformOptions
{
    /// <summary>If true, throw when CRS is unknown; if false, return error in result.</summary>
    public bool Strict { get; set; }

    /// <summary>Axis order preference (library uses its default if not specified).</summary>
    public AxisOrderPreference AxisOrderPreference { get; set; } = AxisOrderPreference.LibraryDefault;

    /// <summary>Trade-off between speed and accuracy when multiple paths exist.</summary>
    public AccuracyPreference AccuracyPreference { get; set; } = AccuracyPreference.Balanced;
}

/// <summary>Preferred axis order for geographic coordinates.</summary>
public enum AxisOrderPreference
{
    /// <summary>Use library default (typically lat, lon).</summary>
    LibraryDefault,
    LongitudeLatitude,
    LatitudeLongitude
}

/// <summary>Preference when multiple transformation options exist.</summary>
public enum AccuracyPreference
{
    Fast,
    Balanced,
    BestAvailable
}
