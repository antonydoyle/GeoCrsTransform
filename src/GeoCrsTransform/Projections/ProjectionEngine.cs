using static System.Math;

namespace GeoCrsTransform;

/// <summary>Forward and inverse map projections.</summary>
internal static class ProjectionEngine
{
    private const int MaxIterations = 15;
    private const double Eps = 1e-12;

    public static ProjectedCoordinate Project(GeoCoordinate geo, ProjectionDefinition def, Ellipsoid ellipsoid)
    {
        return def.Kind switch
        {
            ProjectionKind.WebMercator => ProjectWebMercator(geo),
            ProjectionKind.TransverseMercator => ProjectTransverseMercator(geo, def, ellipsoid),
            _ => throw new NotSupportedException($"Projection {def.Kind} not implemented.")
        };
    }

    public static GeoCoordinate Unproject(ProjectedCoordinate proj, ProjectionDefinition def, Ellipsoid ellipsoid)
    {
        return def.Kind switch
        {
            ProjectionKind.WebMercator => UnprojectWebMercator(proj),
            ProjectionKind.TransverseMercator => UnprojectTransverseMercator(proj, def, ellipsoid),
            _ => throw new NotSupportedException($"Projection {def.Kind} not implemented.")
        };
    }

    private static ProjectedCoordinate ProjectWebMercator(GeoCoordinate geo)
    {
        var lat = geo.LatitudeDeg * (PI / 180.0);
        var lon = geo.LongitudeDeg * (PI / 180.0);
        lat = Min(Max(lat, -PI / 2 + 1e-10), PI / 2 - 1e-10);
        var x = 6378137.0 * lon;
        var y = 6378137.0 * Log(Tan(PI / 4 + lat / 2));
        return new ProjectedCoordinate(x, y, geo.HeightMeters);
    }

    private static GeoCoordinate UnprojectWebMercator(ProjectedCoordinate proj)
    {
        var x = proj.EastingMeters / 6378137.0;
        var y = proj.NorthingMeters / 6378137.0;
        var lon = x * (180.0 / PI);
        var lat = (2 * Atan(Exp(y)) - PI / 2) * (180.0 / PI);
        return new GeoCoordinate(lat, lon, proj.HeightMeters);
    }

    private static ProjectedCoordinate ProjectTransverseMercator(GeoCoordinate geo, ProjectionDefinition def, Ellipsoid ellipsoid)
    {
        var lat = geo.LatitudeDeg * (PI / 180.0);
        var lon = geo.LongitudeDeg * (PI / 180.0);
        var lon0 = def.CentralMeridianDeg * (PI / 180.0);
        var lat0 = def.LatitudeOfOriginDeg * (PI / 180.0);
        var k0 = def.ScaleFactor;
        var fe = def.FalseEastingMeters;
        var fn = def.FalseNorthingMeters;
        var a = ellipsoid.SemiMajorAxisMeters;
        var e2 = ellipsoid.EccentricitySq;
        var e = Sqrt(e2);

        var dLon = lon - lon0;
        var n = a / Sqrt(1 - e2);
        var nu = a / Sqrt(1 - e2 * Sin(lat) * Sin(lat));
        var psi = lat;
        var psi1 = Atan((1 - e) / (1 + e) * Tan(lat));
        var psi2 = 2 * psi1;
        var psi4 = 4 * psi1;
        var sinPsi = Sin(psi);
        var cosPsi = Cos(psi);
        var tanPsi = Tan(psi);
        var sinPsi2 = Sin(psi2);
        var cosPsi2 = Cos(psi2);
        var sinPsi4 = Sin(psi4);
        var cosPsi4 = Cos(psi4);

        var c1 = e2 / 2 + 5 * e2 * e2 / 24;
        var c2 = 7 * e2 * e2 / 48;
        var m = a * ((1 - e2 / 4 - 3 * e2 * e2 / 64) * psi - (3 * e2 / 8 + 3 * e2 * e2 / 32) * sinPsi2 + (15 * e2 * e2 / 256) * sinPsi4);
        var m0 = a * ((1 - e2 / 4 - 3 * e2 * e2 / 64) * lat0 - (3 * e2 / 8 + 3 * e2 * e2 / 32) * Sin(2 * lat0) + (15 * e2 * e2 / 256) * Sin(4 * lat0));

        var x = k0 * nu * dLon * cosPsi * (1 + dLon * dLon / 6 * (cosPsi * cosPsi * (1 - tanPsi * tanPsi + (nu / n - 1)) + dLon * dLon / 20 * cosPsi * cosPsi * cosPsi * cosPsi * (5 - 18 * tanPsi * tanPsi + tanPsi * tanPsi * tanPsi * tanPsi)));
        var y = k0 * (m - m0 + nu * tanPsi * (dLon * dLon / 2 + dLon * dLon * dLon * dLon / 24 * cosPsi * cosPsi * (5 - tanPsi * tanPsi + 9 * (nu / n - 1))));
        return new ProjectedCoordinate(fe + x, fn + y, geo.HeightMeters);
    }

    private static GeoCoordinate UnprojectTransverseMercator(ProjectedCoordinate proj, ProjectionDefinition def, Ellipsoid ellipsoid)
    {
        var x = proj.EastingMeters - def.FalseEastingMeters;
        var y = proj.NorthingMeters - def.FalseNorthingMeters;
        var lon0 = def.CentralMeridianDeg * (PI / 180.0);
        var lat0 = def.LatitudeOfOriginDeg * (PI / 180.0);
        var k0 = def.ScaleFactor;
        var a = ellipsoid.SemiMajorAxisMeters;
        var e2 = ellipsoid.EccentricitySq;

        var m0 = MeridianArc(a, e2, lat0);
        var m = m0 + y / k0;
        var lat = LatitudeFromMeridian(a, e2, m);
        var sinLat = Sin(lat);
        var cosLat = Cos(lat);
        var tanLat = Tan(lat);
        var nu = a / Sqrt(1 - e2 * sinLat * sinLat);
        var lon = lon0 + (x / (k0 * nu * cosLat)) * (1 - x * x / (6 * k0 * k0 * nu * nu) * (1 + 2 * tanLat * tanLat));
        return new GeoCoordinate(lat * (180.0 / PI), lon * (180.0 / PI), proj.HeightMeters);
    }

    private static double MeridianArc(double a, double e2, double lat)
    {
        var e4 = e2 * e2;
        return a * ((1 - e2 / 4 - 3 * e4 / 64) * lat - (3 * e2 / 8 + 3 * e4 / 32) * Sin(2 * lat) + (15 * e4 / 256) * Sin(4 * lat));
    }

    private static double LatitudeFromMeridian(double a, double e2, double m)
    {
        var e4 = e2 * e2;
        var e6 = e2 * e4;
        var n = (1 - Sqrt(1 - e2)) / (1 + Sqrt(1 - e2));
        var n2 = n * n;
        var n3 = n2 * n;
        var c = a * (1 - e2 / 4 - 3 * e4 / 64 - 5 * e6 / 256);
        var mu = m / c;
        return mu + (3 * n / 2 - 27 * n3 / 32) * Sin(2 * mu) + (21 * n2 / 16) * Sin(4 * mu) + (151 * n3 / 96) * Sin(6 * mu);
    }
}
