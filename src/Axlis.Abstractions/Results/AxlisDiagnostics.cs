using System.Runtime.CompilerServices;

namespace Axlis.Results;

/// <summary>
/// Collects <see cref="AxlisDiagnosticEvent"/> entries produced during an Axlis operation.
/// Returned as part of <see cref="AxlisResult{T}"/> when using the <c>WithResult</c> API.
/// </summary>
public sealed class AxlisDiagnostics
{
    private readonly List<AxlisDiagnosticEvent> _events = new();

    /// <summary>Gets all captured events in chronological order.</summary>
    public IReadOnlyList<AxlisDiagnosticEvent> Events => _events;

    /// <summary>Gets a value indicating whether any event with <see cref="DiagnosticSeverity.Error"/> or above was captured.</summary>
    public bool HasErrors => _events.Exists(e => e.Severity >= DiagnosticSeverity.Error);

    /// <summary>Adds a pre-built <see cref="AxlisDiagnosticEvent"/> to the collection.</summary>
    public void Add(AxlisDiagnosticEvent evt) => _events.Add(evt);

    /// <summary>Creates and adds an event with the supplied message and severity.</summary>
    /// <param name="message">Human-readable description of the event.</param>
    /// <param name="severity">Event severity.</param>
    /// <param name="data">Optional structured data.</param>
    /// <param name="callerMember">Automatically captured caller method name.</param>
    /// <param name="callerFile">Automatically captured caller file path.</param>
    /// <param name="callerLine">Automatically captured caller line number.</param>
    public void Add(
        string message,
        DiagnosticSeverity severity = DiagnosticSeverity.Info,
        object? data = null,
        [CallerMemberName] string? callerMember = null,
        [CallerFilePath] string? callerFile = null,
        [CallerLineNumber] int callerLine = 0)
    {
        _events.Add(new AxlisDiagnosticEvent
        {
            Message = message,
            Severity = severity,
            Data = data,
            CallerMember = callerMember,
            CallerFile = callerFile,
            CallerLine = callerLine,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });
    }
}
