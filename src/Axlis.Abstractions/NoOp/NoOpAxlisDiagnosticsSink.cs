using Axlis.Results;

namespace Axlis.NoOp;

/// <summary>
/// A diagnostics sink that silently discards all events.
/// Used as the default when diagnostics are not configured.
/// </summary>
public sealed class NoOpAxlisDiagnosticsSink : IAxlisDiagnosticsSink
{
    /// <summary>The shared singleton instance.</summary>
    public static readonly NoOpAxlisDiagnosticsSink Instance = new();

    private NoOpAxlisDiagnosticsSink() { }

    /// <inheritdoc/>
    public void OnEvent(AxlisDiagnosticEvent evt) { }
}
