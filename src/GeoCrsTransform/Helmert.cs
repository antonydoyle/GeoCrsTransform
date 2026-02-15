using static System.Math;

namespace GeoCrsTransform;

/// <summary>Apply Helmert 7-parameter transform in ECEF. Rotations stored in arc-seconds, converted to radians.</summary>
internal static class Helmert
{
    private const double ArcSecToRad = PI / (180.0 * 3600.0);

    /// <summary>Apply transform ToWgs84: source_datum -> WGS84 (ECEF). X_wgs = s*R*X_src + T.</summary>
    public static Vector3d ApplyToWgs84(Vector3d ecef, DatumTransform t)
    {
        return Apply(ecef, t.TxMeters, t.TyMeters, t.TzMeters, t.RxArcSeconds, t.RyArcSeconds, t.RzArcSeconds, t.ScalePpm);
    }

    /// <summary>Apply transform FromWgs84: WGS84 -> source_datum (ECEF). X_src = R'*(X_wgs - T)/s.</summary>
    public static Vector3d ApplyFromWgs84(Vector3d ecef, DatumTransform t)
    {
        var s = 1.0 + t.ScalePpm * 1e-6;
        var px = (ecef.X - t.TxMeters) / s;
        var py = (ecef.Y - t.TyMeters) / s;
        var pz = (ecef.Z - t.TzMeters) / s;
        var rx = t.RxArcSeconds * ArcSecToRad;
        var ry = t.RyArcSeconds * ArcSecToRad;
        var rz = t.RzArcSeconds * ArcSecToRad;
        var x = px - rz * py + ry * pz;
        var y = rz * px + py - rx * pz;
        var z = -ry * px + rx * py + pz;
        return new Vector3d(x, y, z);
    }

    /// <summary>Standard 7-parameter: X_wgs84 = (1+s) * R * X_src + T. R in arc-seconds (small angles).</summary>
    public static Vector3d Apply(Vector3d p, double tx, double ty, double tz, double rxArcSec, double ryArcSec, double rzArcSec, double scalePpm)
    {
        var rx = rxArcSec * ArcSecToRad;
        var ry = ryArcSec * ArcSecToRad;
        var rz = rzArcSec * ArcSecToRad;
        var s = 1.0 + scalePpm * 1e-6;
        var r00 = 1.0; var r01 = rz;  var r02 = -ry;
        var r10 = -rz; var r11 = 1.0; var r12 = rx;
        var r20 = ry;  var r21 = -rx; var r22 = 1.0;
        var x = s * (r00 * p.X + r01 * p.Y + r02 * p.Z) + tx;
        var y = s * (r10 * p.X + r11 * p.Y + r12 * p.Z) + ty;
        var z = s * (r20 * p.X + r21 * p.Y + r22 * p.Z) + tz;
        return new Vector3d(x, y, z);
    }
}
