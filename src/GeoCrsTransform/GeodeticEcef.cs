using static System.Math;

namespace GeoCrsTransform;

/// <summary>Geodetic (lat/lon/h) to ECEF and back using the given ellipsoid.</summary>
internal static class GeodeticEcef
{
    private const int MaxIterations = 10;
    private const double Eps = 1e-12;

    /// <summary>Convert geographic coordinate to ECEF (meters).</summary>
    public static Vector3d ToEcef(GeoCoordinate geo, Ellipsoid ellipsoid)
    {
        var lat = geo.LatitudeDeg * (PI / 180.0);
        var lon = geo.LongitudeDeg * (PI / 180.0);
        var h = geo.HeightMeters ?? 0.0;
        var a = ellipsoid.SemiMajorAxisMeters;
        var e2 = ellipsoid.EccentricitySq;
        var sinLat = Sin(lat);
        var cosLat = Cos(lat);
        var sinLon = Sin(lon);
        var cosLon = Cos(lon);
        var n = a / Sqrt(1 - e2 * sinLat * sinLat);
        var x = (n + h) * cosLat * cosLon;
        var y = (n + h) * cosLat * sinLon;
        var z = (n * (1 - e2) + h) * sinLat;
        return new Vector3d(x, y, z);
    }

    /// <summary>Convert ECEF (meters) to geographic. Handles poles and dateline.</summary>
    public static GeoCoordinate FromEcef(Vector3d ecef, Ellipsoid ellipsoid)
    {
        var a = ellipsoid.SemiMajorAxisMeters;
        var e2 = ellipsoid.EccentricitySq;
        var x = ecef.X;
        var y = ecef.Y;
        var z = ecef.Z;
        var p = Sqrt(x * x + y * y);
        if (p < Eps)
        {
            var latPole = z >= 0 ? PI / 2 : -PI / 2;
            var lonPole = 0.0;
            var h = Abs(z) - (1 - Sqrt(1 - e2)) * a;
            return new GeoCoordinate(latPole * (180.0 / PI), lonPole * (180.0 / PI), h);
        }
        var lon = Atan2(y, x);
        var lat = Atan2(z, p * (1 - e2));
        for (var i = 0; i < MaxIterations; i++)
        {
            var sinLat = Sin(lat);
            var n = a / Sqrt(1 - e2 * sinLat * sinLat);
            var latNew = Atan2(z + e2 * n * sinLat, p);
            if (Abs(latNew - lat) < Eps)
            {
                lat = latNew;
                var h = p / Cos(lat) - n;
                return new GeoCoordinate(lat * (180.0 / PI), lon * (180.0 / PI), h);
            }
            lat = latNew;
        }
        var nFinal = a / Sqrt(1 - e2 * Sin(lat) * Sin(lat));
        var height = p / Cos(lat) - nFinal;
        return new GeoCoordinate(lat * (180.0 / PI), lon * (180.0 / PI), height);
    }
}
