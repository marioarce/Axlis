# Module Spec — `SitecoreItemCacheManager`

**File:** `src/Axlis.ORM/Caching/SitecoreItemCacheManager.cs`
**Consumed by:** `SitecoreFacade` (every cacheable fetch method), `SitecoreItemLazyLoader` (`src/Axlis.ORM/LazyLoader/SitecoreItemLazyLoader.cs`)
**Wraps:** `ICacheService` from `PowerCSharp.Feature.Cache.Abstractions` (external dependency, treated as a black box — see this project's `CLAUDE.md`)
**Status:** Fragile zone — referenced from root [`CLAUDE.md`](../../CLAUDE.md) §4 and this project's `CLAUDE.md`.

## Purpose

Stampede-safe(*) in-memory cache for `IItem` results, sitting between `SitecoreFacade`/`SitecoreItemLazyLoader` and `ISitecoreService`. Reduces redundant GraphQL round-trips for repeated lookups of the same item by ID or path.

*(*) Stampede-safety is a property of the underlying `ICacheService` implementation (e.g. `PowerCSharp.Feature.Cache.BitFaster`), not of this class itself — `SitecoreItemCacheManager` does not implement its own locking/single-flight logic. Do not assume stampede protection if a consumer has swapped in a custom `ICacheService` without that guarantee.*

## Invariants (do not break these silently)

1. **Dual/triple indexing.** On a cache miss + successful fetch, the item is stored under: (a) the original lookup `key` as passed in, (b) the item's own `Id` (if different from `key`), (c) the item's own `Path` (if different from `key`). This is what lets a lookup by ID resolve from cache after a prior lookup by path, and vice versa. If you change the key-write logic, all three writes must stay in sync, or this cross-resolution silently stops working.
2. **Null results are never cached.** If `factory()` returns `null` (item not found upstream), nothing is written to any cache key — the docstring is explicit about why: "so a later re-publish can be picked up on the next request." Do not add null-caching as a "performance optimization" — it directly breaks Sitecore publish-then-refetch semantics that consumers rely on.
3. **Key comparison is case-insensitive** (`StringComparison.OrdinalIgnoreCase`) when deciding whether the item's own ID/path differs from the requested key (to decide whether to write a second/third cache entry) — but the underlying `ICacheService.GetAsync`/`SetAsync` key comparison semantics are whatever the injected implementation does. If a custom `ICacheService` is case-sensitive, dual-indexing can silently produce three *different* cache entries for what this class considers "the same key" (e.g. `/sitecore/content/Home` vs. `/Sitecore/Content/Home`). This is a real interop risk between this class's assumptions and a consumer-supplied cache implementation — flag it if you see related bug reports.
4. **TTL is a single value for all items** (`AxlisOptions.CacheTtl`, default 60 minutes, `null` = no expiry) — there is no per-item or per-type TTL override today. If you need one, this class's signature (`GetOrCreateAsync(key, factory, ct)`) has no room for it; that's a deliberate simplicity trade-off, not an oversight — treat adding per-item TTL as an API-widening change requiring a new overload, not a silent change to existing behavior.
5. **`InvalidateAsync` only evicts the single key passed in.** It does **not** know about or evict the other 1–2 keys (ID/path) that may have been written alongside it during a prior `GetOrCreateAsync` call. A consumer invalidating by path after a lookup that also indexed by ID must call `InvalidateAsync` for both keys explicitly (see the `PublishWebhookController` example in `docs/orm/Caching.md`, which already does this correctly — do not "simplify" that example to a single call).

## When Modifying

1. Any change to the dual-indexing write logic requires updated tests in `tests/Axlis.ORM.Tests/SitecoreItemCacheManagerTests.cs` covering: write-then-read-by-ID, write-then-read-by-path, and the null-miss non-caching behavior.
2. Do not add caching of `null`/not-found results without an explicit, separate opt-in mechanism and a call-out in `CHANGELOG.md` — this is a behavioral change every consumer depends on implicitly.
3. If you touch `SitecoreItemLazyLoader` (which routes lazy `Axes` fetches through this same cache manager), re-read [`../../Axlis.ORM.Core/AxesLazyLoading.md`](../../Axlis.ORM.Core/AxesLazyLoading.md) — the sync-over-async wrapper there depends on this class's async methods completing promptly (a cache implementation that blocks or is slow directly affects that hazard).
4. Keep `docs/orm/Caching.md` in sync if you change the public shape of `GetOrCreateAsync`/`InvalidateAsync` — and fix the stale `AddAxlis()`/`AddAxlisGraphQL()` naming in that doc if you're already editing it (see this project's `CLAUDE.md` "Naming Note").

## Verification Checklist for Any Change Here

- [ ] `SitecoreItemCacheManagerTests.cs` covers dual-indexing for both read directions.
- [ ] Null-miss non-caching behavior explicitly tested and still passing.
- [ ] If TTL/key semantics changed: `docs/orm/Caching.md` config table updated to match.
- [ ] If `InvalidateAsync` semantics changed: confirmed against the multi-key invalidation example in `docs/orm/Caching.md`.
