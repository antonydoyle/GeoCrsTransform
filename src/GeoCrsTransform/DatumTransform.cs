namespace GeoCrsTransform;

/// <summary>Helmert 7-parameter datum transform. Translations in meters, rotations in arc-seconds, scale in ppm. Apply in ECEF.</summary>
public sealed class DatumTransform
{
    public double TxMeters { get; }
    public double TyMeters { get; }
    public double TzMeters { get; }
    public double RxArcSeconds { get; }
    public double RyArcSeconds { get; }
    public double RzArcSeconds { get; }
    public double ScalePpm { get; }

    public DatumTransform(double tx, double ty, double tz, double rx, double ry, double rz, double scalePpm)
    {
        TxMeters = tx;
        TyMeters = ty;
        TzMeters = tz;
        RxArcSeconds = rx;
        RyArcSeconds = ry;
        RzArcSeconds = rz;
        ScalePpm = scalePpm;
    }

    /// <summary>Identity transform (no change).</summary>
    public static DatumTransform Identity { get; } = new DatumTransform(0, 0, 0, 0, 0, 0, 0);
}
