namespace Axlis.Results;

/// <summary>
/// Represents a single diagnostic event captured during an Axlis operation.
/// Consumed by <see cref="IAxlisDiagnosticsSink"/> implementations (e.g. the default ILogger sink).
/// </summary>
public sealed class AxlisDiagnosticEvent
{
    /// <summary>Gets the human-readable message describing the event.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Gets the severity level of the event.</summary>
    public DiagnosticSeverity Severity { get; init; }

    /// <summary>Gets the name of the calling member where the event was raised.</summary>
    public string? CallerMember { get; init; }

    /// <summary>Gets the source file path where the event was raised.</summary>
    public string? CallerFile { get; init; }

    /// <summary>Gets the line number in the source file where the event was raised.</summary>
    public int CallerLine { get; init; }

    /// <summary>Gets the UTC timestamp (Unix milliseconds) when the event was created.</summary>
    public long Timestamp { get; init; }

    /// <summary>Gets optional structured data associated with the event.</summary>
    public object? Data { get; init; }
}
