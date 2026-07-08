namespace Axlis.Results;

/// <summary>
/// Severity levels for <see cref="AxlisDiagnosticEvent"/> entries.
/// </summary>
public enum DiagnosticSeverity
{
    /// <summary>Verbose trace information — not shown in production unless explicitly enabled.</summary>
    Debug = 0,

    /// <summary>Informational event (e.g. cache hit/miss, item fetched).</summary>
    Info = 1,

    /// <summary>Non-fatal unexpected condition (e.g. missing optional field, slow query).</summary>
    Warning = 2,

    /// <summary>Fatal or actionable error (e.g. GraphQL transport failure, null mandatory item).</summary>
    Error = 3
}
