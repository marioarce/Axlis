# WORKFLOWS.md — Axlis.ORM (Facade)

Project-scoped SOPs. See the repo-root [`WORKFLOWS.md`](../../WORKFLOWS.md) for the 3 repo-wide SOPs — this file adds narrower procedures specific to the facade/DI layer.

## SOP F1 — Adding a New Facade Method (Clean + Rich Pair)

1. Confirm the underlying `ISitecoreService` operation already exists (see `Axlis.ORM.GraphQL`'s SOP G1 if not).
2. Add both signatures to `ISitecoreFacade` in `Axlis.ORM.Abstractions` in the same change — never ship Clean without Rich or vice versa (see this project's `CLAUDE.md`).
3. Implement the Clean method: call `_cacheManager.GetOrCreateAsync(...)` (or the service directly if the operation isn't cacheable), `try/catch` broadly, log via `_logger?.LogError`, return `null` on failure. Follow the exact shape of `GetItemByPathAsync<T>` as the template.
4. Implement the Rich method: same call, but build an `AxlisDiagnostics` instance, populate it via `AddDiagnostic`/`RecordError`, and return `AxlisResult<T>` via `BuildResult<T>`. Follow `GetItemByPathWithResultAsync<T>` as the template.
5. Add tests in `tests/Axlis.ORM.Tests/SitecoreFacadeTests.cs` for both methods: happy path, not-found, and service-throws (verify Clean returns `null` + logs, Rich returns populated diagnostics with `DiagnosticSeverity.Error`/`Warning` as appropriate).
6. If the operation should be cached, route it through `SitecoreItemCacheManager.GetOrCreateAsync` — read [`Caching/CacheManager.md`](Caching/CacheManager.md) first if this is a new caching pattern (e.g. a new key shape beyond ID/path).

## SOP F2 — Changing DI Registration (`AddAxlisORM`, `AddAxlisORMGraphQL`, `UseAxlis`)

This is a high-caution SOP — every consumer's `Program.cs` depends on the current registration order and lifetime choices.

1. Preserve registration order semantics: `AddAxlisORM()` must register its `NoOpCacheService` fallback in a way that a consumer's earlier `ICacheService` registration still wins (ASP.NET Core "first registration wins" behavior). Do not reorder without testing both "consumer registered their own cache" and "consumer registered nothing" scenarios.
2. Preserve service lifetimes exactly (`AddSingleton` for `SitecoreItemCacheManager`, `IItemLazyLoader`, `IAxlisDiagnosticsSink`, `ISitecoreFacade`) unless there's a deliberate, documented reason to change one — a lifetime change is a breaking behavioral change, not a refactor.
3. If adding a new required service, provide it a sensible default registration so `AddAxlisORM()` remains a one-call "it just works" experience — don't force new mandatory configuration on existing consumers.
4. `UseAxlis()` must remain idempotent-safe to call once and must continue to be the single wiring point for `ExtendedItem`'s static loader — do not introduce a second code path that also calls `ExtendedItem.Initialize`.
5. Add/update a test in `tests/Axlis.ORM.Tests/` (or a new DI-focused test file) that builds a minimal `ServiceCollection`, calls `AddAxlisORM()` + `AddAxlisORMGraphQL()`, builds the provider, and resolves `ISitecoreFacade` successfully — this is the cheapest regression guard against a broken registration graph.
6. Update `docs/orm/GettingStarted.md` and `docs/orm/Caching.md` if the public registration API surface changes at all — and fix any stale `AddAxlis()`/`AddAxlisGraphQL()` references you encounter while you're in there (see this project's `CLAUDE.md` "Naming Note").

## SOP F3 — Touching `SitecoreItemCacheManager` or `SitecoreItemLazyLoader`

1. Read [`Caching/CacheManager.md`](Caching/CacheManager.md) in full first — this is a documented fragile zone.
2. Any change to key normalization (ID vs. path, case sensitivity) must be verified against `tests/Axlis.ORM.Tests/SitecoreItemCacheManagerTests.cs` for both index paths (by-ID and by-path retrieval after a by-path write, and vice versa).
3. `SitecoreItemLazyLoader.LoadItem` is synchronous by design (`GetAwaiter().GetResult()` over the async cache/service call) because `ExtendedItem`'s property getters are synchronous. Do not "fix" this by making it async without a much larger redesign of `IItemLazyLoader`/`ExtendedItem` — flag any deadlock concern in ASP.NET Core vs. non-ASP hosts explicitly in the PR rather than silently changing the sync/async boundary.
