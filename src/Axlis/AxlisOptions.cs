namespace Axlis;

/// <summary>
/// Top-level configuration options for the <c>Axlis</c> package.
/// Bind to the <c>"Axlis"</c> configuration section via <c>AddAxlis()</c>.
/// </summary>
public sealed class AxlisOptions
{
    /// <summary>The default configuration section name.</summary>
    public const string SectionName = "Axlis";

    /// <summary>Default cache TTL in minutes.</summary>
    public const int DefaultCacheTtlMinutes = 60;

    /// <summary>
    /// Gets or sets the time-to-live for cached Sitecore items.
    /// <c>null</c> means entries never expire (eviction only).
    /// Default: 60 minutes.
    /// </summary>
    public TimeSpan? CacheTtl { get; set; } = TimeSpan.FromMinutes(DefaultCacheTtlMinutes);

    /// <summary>
    /// Gets or sets whether to populate <see cref="Results.AxlisDiagnostics"/> on the <c>WithResult</c> API.
    /// When <c>false</c>, diagnostics objects are still created but remain empty for zero overhead.
    /// Default: <c>true</c>.
    /// </summary>
    public bool EnableDiagnostics { get; set; } = true;
}
