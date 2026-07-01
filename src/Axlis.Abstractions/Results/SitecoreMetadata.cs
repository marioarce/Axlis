namespace Axlis.Results;

/// <summary>
/// Captures provenance metadata for a Sitecore item returned by the <c>WithResult</c> API.
/// </summary>
public sealed class SitecoreMetadata
{
    /// <summary>Gets the unique identifier of the fetched item.</summary>
    public string? ItemId { get; init; }

    /// <summary>Gets the full Sitecore path of the fetched item.</summary>
    public string? ItemPath { get; init; }

    /// <summary>Gets the version number of the fetched item.</summary>
    public int ItemVersion { get; init; }

    /// <summary>Gets the UTC timestamp (Unix milliseconds) when the item was fetched.</summary>
    public long Timestamp { get; init; }

    /// <summary>Gets the cache tags associated with this item (used for targeted invalidation).</summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}
