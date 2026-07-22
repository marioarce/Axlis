namespace Axlis.ORM.Services;

/// <summary>
/// Low-level Sitecore data-access contract. Returns raw <see cref="IItem"/> domain objects.
/// Consumers should typically use <see cref="ISitecoreFacade"/> for strongly-typed access.
/// Implemented by <c>SitecoreService</c> in <c>Axlis.ORM.GraphQL</c>.
/// </summary>
public interface ISitecoreService
{
    /// <summary>Fetches a fully-loaded item by its Sitecore path or ID string.</summary>
    /// <param name="path">Sitecore path (e.g. "/sitecore/content/home") or item ID (GUID string).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IItem?> GetItemByPathAsync(string path, CancellationToken ct = default);

    /// <summary>Fetches a fully-loaded item by its Sitecore item ID.</summary>
    /// <param name="id">The item ID as a GUID string (braces optional).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IItem?> GetItemByIdAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Fetches multiple items by their paths or IDs in parallel batches.
    /// Returns a dictionary mapping each input path/ID to the resolved item (or <c>null</c> if not found).
    /// </summary>
    /// <param name="paths">Collection of Sitecore paths or ID strings.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IDictionary<string, IItem?>> GetItemsByPathsAsync(IEnumerable<string> paths, CancellationToken ct = default);

    /// <summary>
    /// Fetches a flat item (no parent, ancestors, or children) by its Sitecore path or ID.
    /// More efficient than <see cref="GetItemByPathAsync"/> when tree structure is not needed.
    /// </summary>
    /// <param name="path">Sitecore path or item ID.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IItem?> GetItemFlatAsync(string path, CancellationToken ct = default);
}
