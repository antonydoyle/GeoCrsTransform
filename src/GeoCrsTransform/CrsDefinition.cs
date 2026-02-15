namespace GeoCrsTransform;

/// <summary>Definition of a coordinate reference system.</summary>
public sealed class CrsDefinition
{
    public CrsId Id { get; }
    public string Name { get; }
    public CrsKind Kind { get; }
    public Ellipsoid? Ellipsoid { get; }
    public DatumTransform? ToWgs84 { get; }
    public DatumTransform? FromWgs84 { get; }
    public ProjectionDefinition? Projection { get; }
    public AccuracyClass AccuracyClass { get; }
    public IReadOnlyList<string> WarningsByDefault { get; }

    public CrsDefinition(
        CrsId id,
        string name,
        CrsKind kind,
        Ellipsoid? ellipsoid = null,
        DatumTransform? toWgs84 = null,
        DatumTransform? fromWgs84 = null,
        ProjectionDefinition? projection = null,
        AccuracyClass accuracyClass = AccuracyClass.Unknown,
        IReadOnlyList<string>? warningsByDefault = null)
    {
        Id = id;
        Name = name ?? "";
        Kind = kind;
        Ellipsoid = ellipsoid;
        ToWgs84 = toWgs84;
        FromWgs84 = fromWgs84;
        Projection = projection;
        AccuracyClass = accuracyClass;
        WarningsByDefault = warningsByDefault ?? Array.Empty<string>();
    }
}

/// <summary>Kind of CRS: geographic (lat/lon) or projected (eastings/northings).</summary>
public enum CrsKind
{
    Geographic,
    Projected
}
