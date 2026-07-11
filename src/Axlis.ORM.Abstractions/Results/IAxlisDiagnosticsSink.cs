namespace Axlis.ORM.Results;

/// <summary>
/// Receives <see cref="AxlisDiagnosticEvent"/> instances captured by Axlis during content operations.
/// Register a custom implementation via DI to route events to Sentry, Application Insights,
/// OpenTelemetry, or any other destination. The default implementation routes to <c>ILogger</c>.
/// </summary>
public interface IAxlisDiagnosticsSink
{
    /// <summary>Called when Axlis produces a diagnostic event.</summary>
    /// <param name="evt">The diagnostic event to process.</param>
    void OnEvent(AxlisDiagnosticEvent evt);
}
