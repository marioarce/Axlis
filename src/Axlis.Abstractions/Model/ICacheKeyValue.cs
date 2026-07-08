namespace Axlis;

/// <summary>
/// Provides a cache key string for an item, used by <c>SitecoreItemCacheManager</c>
/// to key entries in a stampede-safe cache.
/// </summary>
public interface ICacheKeyValue
{
    /// <summary>Returns a unique string key for this item suitable for use as a cache entry key.</summary>
    string GetCacheKeyValue();
}
