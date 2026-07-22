# Axlis — Architecture

## Overview

Axlis is a Sitecore Headless GraphQL ORM for .NET 8. It is structured as a family of layered packages, each independently installable, with clear inward-pointing dependencies.

```
┌─────────────────────────────────────────────────────────┐
│                       Axlis.ORM                          │
│  SitecoreFacade  SitecoreItemCacheManager  DI wiring    │
└───────────────────────────┬─────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────┐
│                    Axlis.ORM.GraphQL                      │
│  HttpGraphQLTransport  SitecoreService  QueryBuilder     │
└───────────────────────────┬─────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────┐
│                     Axlis.ORM.Core                        │
│  Item  ExtendedItem  AxesAdapter  ItemConverter          │
│  Field types  Built-in templates                         │
└───────────────────────────┬─────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────┐
│                 Axlis.ORM.Abstractions                   │
│  IItem  ISitecoreFacade  ISitecoreService                │
│  IGraphQLTransport  IAxlisDiagnosticsSink                │
│  AxlisResult<T>  SitecoreMetadata  AxlisDiagnostics      │
│  [SitecoreTemplate]  [SitecoreField]                     │
│  NoOp implementations                                    │
└─────────────────────────────────────────────────────────┘
```

## Package Responsibilities

### `Axlis.ORM.Abstractions`
Contracts only. Targets `netstandard2.0` + `net8.0`. Zero runtime dependencies (beyond `Microsoft.Extensions.Logging.Abstractions`). Safe to reference from any layer including .NET Framework Sitecore projects.

### `Axlis.ORM.Core`
De-branded Sitecore domain model. Converts raw GraphQL JSON into typed `Item` graphs. Provides the `ExtendedItem` base class, all field types, `AxesAdapter`, and built-in Sitecore templates.

### `Axlis.ORM.GraphQL`
Default `IGraphQLTransport` provider. Uses raw `HttpClient` + `System.Text.Json` — no third-party GraphQL library. Implements `ISitecoreService` with single-item and batch alias queries.

### `Axlis.ORM`
Top-level consumer package. Provides the `ISitecoreFacade` implementation, `SitecoreItemCacheManager`, `SitecoreItemLazyLoader`, and all `AddAxlisORM*` DI extension methods.

---

## Key Design Decisions

### Transport abstraction
`IGraphQLTransport` / `IGraphQLTransportFactory` live in `Axlis.ORM.Abstractions`, making the default `HttpClient` transport swappable. Consumers can register any custom `IGraphQLTransport`.

### Two API flavors
Following the PowerCSharp Cache convention:
- **Clean** — `Get*Async<T>()` returns `T?`. Errors are logged internally; the call never throws.
- **Rich** — `Get*WithResultAsync<T>()` returns `AxlisResult<T> { Value, Metadata, Diagnostics }` for diagnostics-aware consumers.

### Lazy loading
`ExtendedItem` exposes `Axes` traversal via an ambient `IItemLazyLoader`. The loader is wired at startup via `UseAxlis()` — a DI-injected replacement for the original static factory, preserving lazy-fetch semantics while remaining testable.

### Caching
`SitecoreItemCacheManager` wraps `ICacheService` from `PowerCSharp.Feature.Cache.Abstractions`. Items are indexed by both ID and path. Null results are never cached so re-published items are picked up on the next request. The default `NoOpCacheService` is a safe-off floor; replace it with `AddBitFasterCache()` for production caching.

### Codegen hooks
`[SitecoreTemplate]` and `[SitecoreField]` attributes are codegen-ready today. A future Roslyn/CLI generator can introspect them without breaking changes.

### Target frameworks
- `Axlis.ORM.Abstractions` — `netstandard2.0;net8.0`
- `Axlis.ORM.Core`, `Axlis.ORM.GraphQL`, `Axlis.ORM` — `net8.0`

---

## Data Flow

```
Consumer code
    │
    ▼
ISitecoreFacade.GetItemByPathAsync<T>(path)
    │
    ├── SitecoreItemCacheManager.GetOrCreateAsync(path, factory)
    │       │
    │       ├── ICacheService.GetAsync(key)        ← cache hit → return
    │       │
    │       └── factory()                          ← cache miss
    │               │
    │               ▼
    │       ISitecoreService.GetItemByPathAsync(path)
    │               │
    │               ▼
    │       IGraphQLTransport.ExecuteAsync<TResponse>(request)
    │               │
    │               ▼
    │       HttpClient → Sitecore GraphQL Edge endpoint
    │               │
    │               ▼
    │       JSON → ItemConverter.ToItem(GraphQLItemData) → Item
    │
    ▼
Activator.CreateInstance<T>().SetInnerItem(item)   ← MapToTyped<T>
    │
    ▼
T (your strongly-typed template POCO)
```
