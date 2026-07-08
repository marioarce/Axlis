using Axlis.Caching;
using Axlis.Core;
using Axlis.Services;
using Microsoft.Extensions.Logging;

namespace Axlis.LazyLoader;

/// <summary>
/// Default <see cref="IItemLazyLoader"/> that fetches items on demand via <see cref="ISitecoreService"/>,
/// routing through <see cref="SitecoreItemCacheManager"/> so lazy re-fetches are cache-backed.
/// <para>
/// <see cref="LoadItem"/> is synchronous because it is called from <c>ExtendedItem</c> property
/// getters. It wraps the async service call with <c>GetAwaiter().GetResult()</c> which is safe
/// in ASP.NET Core (no ambient synchronization context) but should be avoided in non-ASP environments.
/// </para>
/// </summary>
public sealed class SitecoreItemLazyLoader : IItemLazyLoader
{
    private const string Tag = "axlis.lazy";

    private readonly ISitecoreService _sitecoreService;
    private readonly SitecoreItemCacheManager _cacheManager;
    private readonly ILogger<SitecoreItemLazyLoader>? _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="SitecoreItemLazyLoader"/>.
    /// </summary>
    public SitecoreItemLazyLoader(
        ISitecoreService sitecoreService,
        SitecoreItemCacheManager cacheManager,
        ILogger<SitecoreItemLazyLoader>? logger = null)
    {
        _sitecoreService = sitecoreService;
        _cacheManager = cacheManager;
        _logger = logger;
    }

    /// <inheritdoc/>
    public Item? LoadItem(string idOrPath)
    {
        _logger?.LogDebug("{Tag}: Lazy-loading item [{IdOrPath}]", Tag, idOrPath);

        try
        {
            var item = _cacheManager
                .GetOrCreateAsync(idOrPath, () => _sitecoreService.GetItemByIdAsync(idOrPath))
                .GetAwaiter()
                .GetResult();

            return item as Item;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "{Tag}: Failed to lazy-load item [{IdOrPath}]", Tag, idOrPath);
            return null;
        }
    }
}
