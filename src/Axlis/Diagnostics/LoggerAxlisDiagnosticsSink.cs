using Axlis.Results;
using Microsoft.Extensions.Logging;

namespace Axlis.Diagnostics;

/// <summary>
/// Default <see cref="IAxlisDiagnosticsSink"/> that routes events to <see cref="ILogger"/>.
/// Severity mapping: <see cref="DiagnosticSeverity.Error"/> and above → <c>LogError</c>;
/// <see cref="DiagnosticSeverity.Warning"/> → <c>LogWarning</c>; everything else → <c>LogDebug</c>.
/// </summary>
public sealed class LoggerAxlisDiagnosticsSink : IAxlisDiagnosticsSink
{
    private readonly ILogger<LoggerAxlisDiagnosticsSink>? _logger;

    /// <summary>Initializes a new instance of <see cref="LoggerAxlisDiagnosticsSink"/>.</summary>
    public LoggerAxlisDiagnosticsSink(ILogger<LoggerAxlisDiagnosticsSink>? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public void OnEvent(AxlisDiagnosticEvent evt)
    {
        switch (evt.Severity)
        {
            case DiagnosticSeverity.Error:
                _logger?.LogError(
                    "Axlis [{Member}]: {Message} (data: {@Data})",
                    evt.CallerMember, evt.Message, evt.Data);
                break;

            case DiagnosticSeverity.Warning:
                _logger?.LogWarning(
                    "Axlis [{Member}]: {Message}",
                    evt.CallerMember, evt.Message);
                break;

            default:
                _logger?.LogDebug(
                    "Axlis [{Member}]: {Message}",
                    evt.CallerMember, evt.Message);
                break;
        }
    }
}
