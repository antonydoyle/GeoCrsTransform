namespace GeoCrsTransform;

/// <summary>Identifier for a coordinate reference system (authority + code, e.g. EPSG:4326).</summary>
public readonly struct CrsId : IEquatable<CrsId>
{
    public string Authority { get; }
    public int Code { get; }

    public CrsId(string authority, int code)
    {
        Authority = authority ?? throw new ArgumentNullException(nameof(authority));
        Code = code;
    }

    /// <summary>Parse a string like "EPSG:4326" into a CrsId.</summary>
    public static bool TryParse(string? value, out CrsId id)
    {
        id = default;
        if (string.IsNullOrWhiteSpace(value))
            return false;
        var span = value.Trim().AsSpan();
        var colon = span.IndexOf(':');
        if (colon < 1 || colon >= span.Length - 1)
            return false;
        var authority = span[..colon].ToString();
        var codeSpan = span[(colon + 1)..];
        if (!int.TryParse(codeSpan, System.Globalization.NumberStyles.Integer, null, out var code) || code < 0)
            return false;
        id = new CrsId(authority, code);
        return true;
    }

    public static CrsId Parse(string value)
    {
        if (!TryParse(value, out var id))
            throw new FormatException($"Invalid CRS id: '{value}'. Expected format: AUTHORITY:CODE (e.g. EPSG:4326).");
        return id;
    }

    public override string ToString() => $"{Authority}:{Code}";
    public bool Equals(CrsId other) => string.Equals(Authority, other.Authority, StringComparison.OrdinalIgnoreCase) && Code == other.Code;
    public override bool Equals(object? obj) => obj is CrsId other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(StringComparer.OrdinalIgnoreCase.GetHashCode(Authority ?? ""), Code);
    public static bool operator ==(CrsId left, CrsId right) => left.Equals(right);
    public static bool operator !=(CrsId left, CrsId right) => !left.Equals(right);
}
