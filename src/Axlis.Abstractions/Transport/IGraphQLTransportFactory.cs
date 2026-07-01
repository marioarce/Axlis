namespace Axlis.Transport;

/// <summary>
/// Creates <see cref="IGraphQLTransport"/> instances, optionally scoped to a specific site.
/// The default implementation resolves a single configured endpoint; supply an
/// <see cref="Axlis.Site.ISiteContext"/> for multi-site endpoint resolution.
/// </summary>
public interface IGraphQLTransportFactory
{
    /// <summary>
    /// Creates or retrieves an <see cref="IGraphQLTransport"/> for the given site key.
    /// </summary>
    /// <param name="siteKey">
    /// Optional site key for multi-site deployments. When <c>null</c> or empty, the default
    /// single configured endpoint is used.
    /// </param>
    IGraphQLTransport Create(string? siteKey = null);
}
