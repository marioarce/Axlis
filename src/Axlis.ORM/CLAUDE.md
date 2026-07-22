# CLAUDE.md — Axlis.ORM (Facade)

Scoped guidance for this project. Inherits everything in the repo-root [`CLAUDE.md`](../../CLAUDE.md).

## Role of This Project

`Axlis.ORM` is the top-level consumer-facing package: `SitecoreFacade` (the `ISitecoreFacade` implementation), `SitecoreItemCacheManager`, `SitecoreItemLazyLoader`, `LoggerAxlisDiagnosticsSink`, `AxlisOptions`, and the DI extension methods (`AddAxlisORM`, `AddAxlisORMGraphQL`, `UseAxlis`). This is the package most consumers install directly (`dotnet add package Axlis.ORM`), and its DI extensions are the first code a consuming application actually calls.

**This is the only layer permitted to catch broadly and convert failures into `null`/`AxlisResult<T>`.** Every other project lets exceptions bubble here by design — see root `CLAUDE.md` §3.

## Hard Constraints

- **External dependency: `PowerCSharp.Feature.Cache.Abstractions`.** This is the one real third-party (well, sibling-ecosystem) dependency in the whole solution. Treat its `ICacheService` contract as a black box — do not assume implementation details beyond its public interface (`GetAsync<T>`, `SetAsync<T>`, `RemoveAsync`). The default registration (`NoOpCacheService`) is a safe-off floor; production caching comes from `PowerCSharp.Feature.Cache.BitFaster`'s `AddBitFasterCache()`, which is **not** a dependency of this project — it's the consumer's choice, registered *before* `AddAxlisORM()`.
- **DI registration order matters and must stay documented.** `AddAxlisORM()` registers `NoOpCacheService` as a fallback; ASP.NET Core DI resolves the *first* registration of a service, so any real `ICacheService` a consumer registers must come *before* `AddAxlisORM()` in their `Program.cs`. Any change to registration order here is a breaking behavioral change for every consumer relying on override semantics — do not "simplify" this without flagging it loudly in the PR and changelog.
- **`UseAxlis(IServiceProvider)` wires static ambient state.** It calls `ExtendedItem.Initialize(lazyLoader)` — a `Core`-layer static field, set from here. This is the one place in the codebase where the facade reaches back into `Core`'s static surface. Do not duplicate this call elsewhere; consumers must call `UseAxlis()` exactly once, after `IServiceProvider` is built.
- **Clean vs. Rich API parity is enforced here, not upstream.** Every `Get*Async<T>` on `SitecoreFacade` has a matching `Get*WithResultAsync<T>`, sharing the same cache/service call but differing in error handling (log-and-null vs. diagnostics-populated `AxlisResult<T>`). Adding one without the other is an incomplete PR — see root `CLAUDE.md` §3.
- **`SitecoreFacade.MapToTyped<T>` uses `Activator.CreateInstance<T>()`.** This means every `IBaseItem`/`ExtendedItem` consumer template class must have a public parameterless constructor — this is an implicit public contract for anyone authoring template POCOs. Don't change this to require a different construction mechanism without a migration path (it would break every existing consumer's template classes).

## Fragile Zone

- [`Caching/CacheManager.md`](Caching/CacheManager.md) — dual-indexing, null-miss non-caching, TTL/eviction semantics, and the sync-over-async pattern in `SitecoreItemLazyLoader` that reads through this cache.

## Naming Note (Known Doc Drift)

The DI extension methods are `AddAxlisORM()` / `AddAxlisORMGraphQL()` / `UseAxlis()` in current code (post ecosystem-rename). All docs and XML doc comments were swept and corrected as of the 2026-07-22 documentation audit — `docs/orm/Caching.md` and several source XML doc comments previously referenced the pre-rename `AddAxlis()` / `AddAxlisGraphQL()`. When writing new docs or examples, use the current names; when you notice stale references creep back in, fix them as part of the change rather than propagating them.

## See Also

- [`WORKFLOWS.md`](WORKFLOWS.md) — SOPs specific to this project.
- Root [`CLAUDE.md`](../../CLAUDE.md) §3 for the exception-handling contract this project implements.
