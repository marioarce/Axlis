using Axlis.Results;

namespace Axlis.Services;

/// <summary>
/// High-level, strongly-typed facade for Sitecore Headless content access.
/// Provides two API flavors mirroring the PowerCSharp Cache pattern:
/// <list type="bullet">
///   <item><description><b>Clean</b> — returns <c>T?</c> directly, logs internally.</description></item>
///   <item><description><b>WithResult</b> — returns <see cref="AxlisResult{T}"/> with item, metadata, and diagnostics.</description></item>
/// </list>
/// Implemented by <c>SitecoreFacade</c> in the <c>Axlis</c> package.
/// </summary>
public interface ISitecoreFacade
{
    // ── Clean API ─────────────────────────────────────────────────────────────

    /// <summary>Fetches and maps a fully-loaded item by Sitecore path or ID.</summary>
    Task<T?> GetItemByPathAsync<T>(string path, CancellationToken ct = default)
        where T : class, IBaseItem;

    /// <summary>Fetches and maps a fully-loaded item by its Sitecore item ID.</summary>
    Task<T?> GetItemByIdAsync<T>(string id, CancellationToken ct = default)
        where T : class, IBaseItem;

    /// <summary>Fetches and maps multiple items by their paths or IDs in parallel batches.</summary>
    Task<IEnumerable<T?>> GetItemsByPathsAsync<T>(IEnumerable<string> paths, CancellationToken ct = default)
        where T : class, IBaseItem;

    /// <summary>Fetches and maps a flat item (no tree context) by Sitecore path or ID.</summary>
    Task<T?> GetItemFlatAsync<T>(string path, CancellationToken ct = default)
        where T : class, IBaseItem;

    // ── Rich (WithResult) API ─────────────────────────────────────────────────

    /// <summary>Fetches a fully-loaded item and returns it with metadata and diagnostics.</summary>
    Task<AxlisResult<T>> GetItemByPathWithResultAsync<T>(string path, CancellationToken ct = default)
        where T : class, IBaseItem;

    /// <summary>Fetches an item by ID and returns it with metadata and diagnostics.</summary>
    Task<AxlisResult<T>> GetItemByIdWithResultAsync<T>(string id, CancellationToken ct = default)
        where T : class, IBaseItem;

    /// <summary>Fetches multiple items and returns them with metadata and diagnostics.</summary>
    Task<AxlisResult<IEnumerable<T?>>> GetItemsByPathsWithResultAsync<T>(IEnumerable<string> paths, CancellationToken ct = default)
        where T : class, IBaseItem;

    /// <summary>Fetches a flat item and returns it with metadata and diagnostics.</summary>
    Task<AxlisResult<T>> GetItemFlatWithResultAsync<T>(string path, CancellationToken ct = default)
        where T : class, IBaseItem;
}
