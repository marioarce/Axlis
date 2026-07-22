# Caching

Axlis caching is powered by `ICacheService` from `PowerCSharp.Feature.Cache.Abstractions`. The default registration (`AddAxlisORM()`) installs a `NoOpCacheService` so the library works out-of-the-box without any cache dependency. For production use, swap in a real provider.

---

## How it works

`SitecoreItemCacheManager` sits between `SitecoreFacade` and `ISitecoreService`:

```
SitecoreFacade.GetItemByPathAsync<T>(path)
    │
    ▼
SitecoreItemCacheManager.GetOrCreateAsync(key, factory)
    │
    ├── ICacheService.GetAsync(key)  →  hit: return cached IItem
    │
    └── miss: factory() → ISitecoreService → GraphQL → Item
                │
                ▼
         ICacheService.Set(key, item, ttl)
         ICacheService.Set(item.Id, item, ttl)    ← dual-index
         ICacheService.Set(item.Path, item, ttl)  ← dual-index
```

**Dual-indexing:** Each item is stored under up to three keys — the original lookup key, the item ID, and the item path. This means a subsequent lookup by either ID or path resolves from cache without a second fetch.

**Null-safety:** If the factory returns `null` (item not found on the server), nothing is written to cache. The next request will re-attempt the fetch — which is the correct behavior after a Sitecore publish.

---

## Configuration

```json
{
  "Axlis": {
    "CacheTtl": "01:00:00",
    "EnableDiagnostics": true
  }
}
```

| Option | Type | Default | Description |
|---|---|---|---|
| `CacheTtl` | `TimeSpan?` | `00:60:00` | Item TTL. `null` = eviction-only (no expiry). |
| `EnableDiagnostics` | `bool` | `true` | Populate `AxlisDiagnostics` on `WithResult` calls. |

Or configure in code:

```csharp
builder.Services.AddAxlis(o =>
{
    o.CacheTtl          = TimeSpan.FromMinutes(30);
    o.EnableDiagnostics = false; // disable for high-throughput paths
});
```

---

## Providers

### NoOp (default)

Registered automatically by `AddAxlisORM()` if no `ICacheService` is already registered. Does not cache anything — every request hits the GraphQL endpoint. Suitable for development or testing.

### BitFaster (recommended for production)

Stampede-safe in-memory LRU cache backed by [BitFaster.Caching](https://github.com/bitfaster/BitFaster.Caching).

```bash
dotnet add package PowerCSharp.Feature.Cache.BitFaster
```

```csharp
builder.Services
    .AddBitFasterCache()   // registers ICacheService — must come BEFORE AddAxlisORM()
    .AddAxlisORM()
    .AddAxlisORMGraphQL(...);
```

### Custom provider

Implement `ICacheService` from `PowerCSharp.Feature.Cache.Abstractions` and register it before `AddAxlisORM()`:

```csharp
builder.Services
    .AddSingleton<ICacheService, MyCustomCacheService>()
    .AddAxlisORM()
    .AddAxlisORMGraphQL(...);
```

Because ASP.NET Core DI resolves the **first** matching registration, any `ICacheService` registered before `AddAxlisORM()` takes precedence over the built-in `NoOpCacheService` fallback.

---

## Cache invalidation

Call `SitecoreItemCacheManager.InvalidateAsync(key)` to evict a single item. Inject the manager directly:

```csharp
public class PublishWebhookController : ControllerBase
{
    private readonly SitecoreItemCacheManager _cacheManager;

    public PublishWebhookController(SitecoreItemCacheManager cacheManager)
        => _cacheManager = cacheManager;

    [HttpPost("publish-webhook")]
    public async Task<IActionResult> OnPublish([FromBody] PublishEvent evt)
    {
        await _cacheManager.InvalidateAsync(evt.ItemId);
        await _cacheManager.InvalidateAsync(evt.ItemPath);
        return Ok();
    }
}
```

---

## Lazy-loader cache

`SitecoreItemLazyLoader` (used by `Axes` traversal) also routes through `SitecoreItemCacheManager`. Lazy-fetched items are cached under the same TTL, so repeated `Axes.Parent` calls on different wrapper instances resolve from memory.
