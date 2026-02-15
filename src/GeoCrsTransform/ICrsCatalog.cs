namespace GeoCrsTransform;

/// <summary>Catalog of coordinate reference system definitions.</summary>
public interface ICrsCatalog
{
    /// <summary>Try to get a CRS definition by id.</summary>
    bool TryGet(CrsId id, out CrsDefinition? definition);

    /// <summary>Try to resolve an alias (e.g. "WGS84", "UTM30N") to a CrsId.</summary>
    bool TryGetByAlias(string alias, out CrsId id);

    /// <summary>List major CRS identifiers in the catalog.</summary>
    IReadOnlyList<CrsId> ListMajor();
}
