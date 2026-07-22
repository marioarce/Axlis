# Module Spec — Axes Lazy Loading (`AxesAdapter`, `ExtendedItem`, `IItemLazyLoader`)

**Files:** `src/Axlis.ORM.Core/AxesAdapter.cs`, `src/Axlis.ORM.Core/Model/ExtendedItem.cs`, `src/Axlis.ORM.Core/Model/IItemLazyLoader.cs`
**Implementation of the loader contract:** `src/Axlis.ORM/LazyLoader/SitecoreItemLazyLoader.cs` (in a different project — see that project's `CLAUDE.md`)
**Status:** Fragile zone — referenced from root [`CLAUDE.md`](../../CLAUDE.md) §4 and this project's `CLAUDE.md`.

## Purpose

Gives `ExtendedItem`-derived template POCOs Synthesis-style tree traversal (`.Axes.Parent`, `.Axes.Children`, `.Axes.Siblings`, `.Axes.GetChildren<T>()`, `.Axes.GetDescendants<T>()`) that transparently re-fetches missing parent/children data on demand, rather than requiring the whole tree to be loaded up front.

## How It Actually Works

1. `ExtendedItem` holds a **`private static IItemLazyLoader? _lazyLoader`** field — process-wide ambient state, not instance state. It's set exactly once via `ExtendedItem.Initialize(loader)`, called from `Axlis.ORM`'s `UseAxlis(IServiceProvider)` extension at app startup.
2. `ExtendedItem.RawInnerItem` (property getter) lazily calls `_lazyLoader.LoadItem(Id)` if the inner `Item` wasn't set at construction time, then caches the result on the instance via `SetInnerItem`.
3. `AxesAdapter` (constructed fresh per `.Axes` access — see `ExtendedItem.Axes => new AxesAdapter(RawInnerItem!, _lazyLoader)`) checks `_item.IsFullyLoaded` / `_item.AreChildrenLoaded` and calls `_loader.LoadItem(_item.Id)` via `TryUpdateAxes()` when data is missing or partially loaded.
4. `IItemLazyLoader.LoadItem(string idOrPath)` is **synchronous**. The production implementation (`SitecoreItemLazyLoader`, in `Axlis.ORM`) is a thin wrapper that runs an async cache/service call via `.GetAwaiter().GetResult()`.

## Invariants and Hazards (know these before touching this code)

1. **Static ambient state is shared process-wide.** Any test, background job, or app that calls `ExtendedItem.Initialize(loaderA)` and later `ExtendedItem.Initialize(loaderB)` on the same process **replaces the loader for every `ExtendedItem` instance currently alive**, not just new ones. Tests must call `ExtendedItem.Reset()` in teardown. Parallel test execution (e.g. xUnit collections running concurrently) that each try to set a different loader **will race** — this is a real, documented limitation, not a hypothetical.
2. **`TryUpdateAxes()` has a bare `catch { }` that swallows every exception.** This is deliberate ("lazy-fetch is best-effort; caller receives null/empty" per the inline comment) but it means: a transient network failure, a malformed response, or a genuine bug in the loader all produce the exact same observable behavior — the axes property silently returns `null`/empty, with **no log entry at this layer**. If you're debugging "why is `.Axes.Children` empty," this catch block is the first place to instrument, not assume it's a data problem.
3. **Sync-over-async in `SitecoreItemLazyLoader.LoadItem`.** `.GetAwaiter().GetResult()` over an async call is safe under ASP.NET Core (no ambient `SynchronizationContext`) but is a classic deadlock risk in any host that does have one (WPF, a classic ASP.NET Framework request context, some test runners). The XML doc on that class already calls this out — do not remove that caveat, and do not assume "it works in our tests" means it's safe in every consumer's host.
4. **`IsFullyLoaded` / `AreChildrenLoaded` / partial-children detection drive whether a re-fetch happens.** `AxesAdapter.TryUpdateAxes()` treats a children collection as "partial" if `TotalCount.HasValue` and the loaded count is less than the total — and only then does it consider a fetched replacement. Changing what counts as "fully loaded" changes how aggressively this re-fetches (and therefore how many extra calls hit the cache/service layer).
5. **`ClosestParent<T>`, `GetChildren<T>`, `GetDescendants<T>`** use `TryCast<T>` which, for non-`ExtendedItem` types, does `Activator.CreateInstance(typeof(T))` + `SetInnerItem` — meaning **every template POCO used with these generic methods must have a public parameterless constructor**, same constraint as `SitecoreFacade.MapToTyped<T>`. This is an implicit contract worth stating explicitly to anyone adding a new generic traversal method.

## When Modifying

1. Do not change `_lazyLoader` from `static` to instance state, or vice versa, without discussing it as a cross-cutting design change — it affects `Axlis.ORM`'s `UseAxlis()` wiring, every test that touches `ExtendedItem`, and the whole "ambient" ergonomics documented in `docs/orm/Architecture.md`.
2. Do not remove or narrow the `catch { }` in `TryUpdateAxes()` without deciding what should happen on failure instead (e.g. surface via `IAxlisDiagnosticsSink`?) — changing this silently changes behavior for every consumer relying on "axes traversal never throws."
3. Any change to `IsFullyLoaded`/partial-load detection needs a test in `tests/Axlis.ORM.Core.Tests/Model/ExtendedItemTests.cs` covering: fully loaded (no re-fetch), not loaded (re-fetch happens), and partially loaded (re-fetch happens and merges).
4. If you add a new lazy-loaded axis or generic traversal method, verify the parameterless-constructor assumption in `TryCast<T>` still holds, or extend it deliberately.

## Verification Checklist for Any Change Here

- [ ] `ExtendedItemTests.cs` covers the changed behavior (loaded / not loaded / partially loaded).
- [ ] If touching the static loader lifecycle: confirmed `ExtendedItem.Reset()` is called in every test that sets a custom loader.
- [ ] If touching `TryUpdateAxes`'s exception handling: the new behavior is documented here and in this project's `CLAUDE.md`, not just in a code comment.
- [ ] No new sync-over-async pattern introduced outside the one documented instance (`SitecoreItemLazyLoader`).
