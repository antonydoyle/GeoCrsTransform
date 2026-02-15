namespace GeoCrsTransform;

/// <summary>Factory for creating coordinate transformers.</summary>
public static class CoordinateTransform
{
    /// <summary>Create the dependency-free managed transformer (this library). Use this for standalone transforms without external native dependencies.</summary>
    public static ICoordinateTransformer CreateManaged()
    {
        var catalog = CrsCatalog.LoadFromEmbedded();
        return new CoordinateTransformer(catalog);
    }

    /// <summary>Create a transformer with a custom catalog (e.g. for testing or extended CRS set).</summary>
    public static ICoordinateTransformer CreateManaged(ICrsCatalog catalog)
    {
        return new CoordinateTransformer(catalog);
    }
}
