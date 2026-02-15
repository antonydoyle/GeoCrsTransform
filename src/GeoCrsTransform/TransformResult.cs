namespace GeoCrsTransform;

/// <summary>Result of a coordinate transformation with metadata and warnings.</summary>
public sealed class TransformResult<T>
{
    public T Output { get; }
    public IReadOnlyList<string> Warnings { get; }
    public AccuracyClass AccuracyClass { get; }
    public string TransformPath { get; }

    public TransformResult(T output, IReadOnlyList<string> warnings, AccuracyClass accuracyClass, string transformPath)
    {
        Output = output;
        Warnings = warnings ?? Array.Empty<string>();
        AccuracyClass = accuracyClass;
        TransformPath = transformPath ?? "";
    }

    public static TransformResult<T> Error(string message, string path = "")
        => new TransformResult<T>(default!, new[] { message }, AccuracyClass.Unknown, path);
}

/// <summary>Indicates expected accuracy of the transform.</summary>
public enum AccuracyClass
{
    High,
    Medium,
    Low,
    Unknown
}
