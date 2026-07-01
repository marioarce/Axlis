namespace Axlis.GraphQL.Transport;

/// <summary>
/// Configuration options for the Axlis GraphQL HTTP transport.
/// Bind to the <c>"AxlisGraphQL"</c> configuration section via <c>AddAxlisGraphQL()</c>.
/// </summary>
public sealed class AxlisGraphQLOptions
{
    /// <summary>The default configuration section name.</summary>
    public const string SectionName = "AxlisGraphQL";

    /// <summary>Default maximum items per batch query.</summary>
    public const int DefaultBatchSize = 10;

    /// <summary>Default HTTP request timeout in seconds.</summary>
    public const int DefaultTimeoutSeconds = 60;

    /// <summary>Default pooled connection lifetime in minutes.</summary>
    public const int DefaultPooledConnectionLifetimeMinutes = 5;

    /// <summary>Sitecore API key header name.</summary>
    public const string ApiKeyHeaderName = "sc_apikey";

    /// <summary>
    /// Gets or sets the Sitecore Headless GraphQL endpoint URL.
    /// Used as the default when no site-specific endpoint is configured.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional API key sent in the <c>sc_apikey</c> request header.
    /// Prefer injecting this from a secrets manager rather than plain config.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of items included in a single batch GraphQL query.
    /// Default: <see cref="DefaultBatchSize"/>.
    /// </summary>
    public int BatchSize { get; set; } = DefaultBatchSize;

    /// <summary>
    /// Gets or sets the HTTP request timeout in seconds. Default: <see cref="DefaultTimeoutSeconds"/>.
    /// </summary>
    public int TimeoutSeconds { get; set; } = DefaultTimeoutSeconds;

    /// <summary>
    /// Gets or sets how long (minutes) a pooled HTTP connection may be reused before being replaced.
    /// Default: <see cref="DefaultPooledConnectionLifetimeMinutes"/>.
    /// </summary>
    public int PooledConnectionLifetimeMinutes { get; set; } = DefaultPooledConnectionLifetimeMinutes;

    /// <summary>
    /// Gets or sets per-site endpoint overrides for multi-site deployments.
    /// Key: site key string (matches <c>ISiteContext.SiteKey</c>); Value: full endpoint URL.
    /// When <c>null</c> or when a site key is not found, <see cref="Endpoint"/> is used.
    /// </summary>
    public Dictionary<string, string>? SiteEndpoints { get; set; }
}
