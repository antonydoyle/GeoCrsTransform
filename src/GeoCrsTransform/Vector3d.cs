namespace GeoCrsTransform;

/// <summary>3D vector (e.g. ECEF in meters).</summary>
internal readonly struct Vector3d
{
    public double X { get; }
    public double Y { get; }
    public double Z { get; }

    public Vector3d(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static Vector3d operator +(Vector3d a, Vector3d b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vector3d operator -(Vector3d a, Vector3d b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
}
