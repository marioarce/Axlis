namespace Axlis.Site;

/// <summary>
/// Resolves the active site key from the current execution context (e.g. HTTP request, ambient state).
/// Used by the transport factory to select the correct GraphQL endpoint in multi-site deployments.
/// </summary>
public interface ISiteResolver
{
    /// <summary>Resolves and returns the active site key, or <c>null</c> to use the default endpoint.</summary>
    string? Resolve();
}
