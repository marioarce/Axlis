namespace Axlis.Site;

/// <summary>
/// Provides the current site key for multi-site Sitecore deployments.
/// Implement this interface and register it with DI to enable per-site GraphQL endpoint resolution.
/// Single-site deployments do not need to implement this — the default endpoint is used automatically.
/// </summary>
public interface ISiteContext
{
    /// <summary>Gets the key that identifies the current site (e.g. "honda", "acura", "my-brand").</summary>
    string SiteKey { get; }
}
