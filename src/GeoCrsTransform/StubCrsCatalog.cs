namespace GeoCrsTransform;

/// <summary>Minimal in-memory catalog for testing; replaced by real catalog in Phase 3.</summary>
public sealed class StubCrsCatalog : ICrsCatalog
{
    private readonly Dictionary<CrsId, CrsDefinition> _byId = new();
    private readonly Dictionary<string, CrsId> _byAlias = new(StringComparer.OrdinalIgnoreCase);

    public void Add(CrsDefinition definition, params string[] aliases)
    {
        _byId[definition.Id] = definition;
        foreach (var a in aliases)
            _byAlias[a] = definition.Id;
    }

    public bool TryGet(CrsId id, out CrsDefinition? definition) => _byId.TryGetValue(id, out definition);

    public bool TryGetByAlias(string alias, out CrsId id)
    {
        if (string.IsNullOrWhiteSpace(alias))
        {
            id = default;
            return false;
        }
        return _byAlias.TryGetValue(alias.Trim(), out id);
    }

    public IReadOnlyList<CrsId> ListMajor() => _byId.Keys.ToList();
}
