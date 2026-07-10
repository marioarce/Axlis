using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PowerCSharp.Feature.Cache.Abstractions;

namespace Axlis.ORM.Caching;

/// <summary>
/// Stampede-safe in-memory cache for Sitecore items.
/// Wraps <see cref="ICacheService"/> and applies the TTL configured via <see cref="AxlisOptions"/>.
/// Items are stored as <see cref="IItem"/> and looked up by their ID or path key.
/// <para>
/// Cache misses for items that do not exist on the server are not cached
/// (a <c>null</c> result from the factory is returned without storage)
/// so that a later re-publish can be picked up on the next request.
/// </para>
/// </summary>
public sealed class SitecoreItemCacheManager
{
    private const string Tag = "axlis.cache";

    private readonly ICacheService _cacheService;
    private readonly TimeSpan? _ttl;
    private readonly ILogger<SitecoreItemCacheManager>? _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="SitecoreItemCacheManager"/>.
    /// </summary>
    /// <param name="cacheService">The underlying in-memory cache service (stampede-safe).</param>
    /// <param name="options">Axlis options providing the item cache TTL.</param>
    /// <param name="logger">Optional logger.</param>
    public SitecoreItemCacheManager(
        ICacheService cacheService,
        IOptions<AxlisOptions> options,
        ILogger<SitecoreItemCacheManager>? logger = null)
    {
        _cacheService = cacheService;
        _ttl = options.Value.CacheTtl;
        _logger = logger;
    }

    /// <summary>
    /// Returns the cached <see cref="IItem"/> for <paramref name="key"/>, or invokes
    /// <paramref name="factory"/> to fetch and store it.
    /// A <c>null</c> result from the factory is returned without being stored in cache.
    /// </summary>
    /// <param name="key">The cache key (item ID or Sitecore path).</param>
    /// <param name="factory">Async delegate that fetches the item from the data source.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<IItem?> GetOrCreateAsync(
        string key,
        Func<Task<IItem?>> factory,
        CancellationToken ct = default)
    {
        // Fast path: check in-memory cache
        var cached = await _cacheService.GetAsync<IItem>(key, ct).ConfigureAwait(false);
        if (cached != null)
        {
            _logger?.LogDebug("{Tag}: Cache hit [{Key}]", Tag, key);
            return cached;
        }

        // Cache miss: invoke factory
        _logger?.LogDebug("{Tag}: Cache miss [{Key}] — fetching from source", Tag, key);
        var item = await factory().ConfigureAwait(false);

        if (item != null)
        {
            // Store under the requested key
            await _cacheService.SetAsync<IItem>(key, item, _ttl, ct).ConfigureAwait(false);

            // Also index under the item's own ID and path if they differ from the key
            if (!string.IsNullOrEmpty(item.Id) &&
                !string.Equals(item.Id, key, StringComparison.OrdinalIgnoreCase))
            {
                await _cacheService.SetAsync<IItem>(item.Id, item, _ttl, ct).ConfigureAwait(false);
            }

            if (!string.IsNullOrEmpty(item.Path) &&
                !string.Equals(item.Path, key, StringComparison.OrdinalIgnoreCase))
            {
                await _cacheService.SetAsync<IItem>(item.Path, item, _ttl, ct).ConfigureAwait(false);
            }
        }
        else
        {
            _logger?.LogDebug("{Tag}: Item [{Key}] not found — skipping cache write", Tag, key);
        }

        return item;
    }

    /// <summary>Removes a cached item by its key (ID or path).</summary>
    /// <param name="key">The cache key to evict.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task InvalidateAsync(string key, CancellationToken ct = default)
    {
        await _cacheService.RemoveAsync(key, ct).ConfigureAwait(false);
        _logger?.LogDebug("{Tag}: Invalidated [{Key}]", Tag, key);
    }
}
