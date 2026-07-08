# Changelog

All notable changes to the Axlis package family are documented here.

Format: [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).
Versioning: [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

---

## [0.1.0-preview] — TBD

### Added

**`Axlis.Abstractions`** — contracts-only, `netstandard2.0` + `net8.0`
- `IItem`, `IItemTemplate`, `IItemTemplateField` — core Sitecore item contracts
- `IBaseItem`, `IExtendedItem`, `IAxes`, `ICacheKeyValue` — ORM model contracts
- `IBaseField` and typed field contracts (`ITextField`, `IImageField`, `IMultilistField`, `IItemReferenceField`, `IHyperlinkField`, `IBooleanField`, `IFileField`)
- `IGraphQLTransport` / `IGraphQLTransportFactory` — pluggable transport seam
- `ISiteContext` / `ISiteResolver` — opt-in multi-site abstraction
- `ISitecoreService` / `ISitecoreFacade` — data-access contracts
- `AxlisResult<T>`, `SitecoreMetadata`, `AxlisDiagnostics`, `AxlisDiagnosticEvent`, `DiagnosticSeverity`
- `IAxlisDiagnosticsSink` — pluggable diagnostics sink
- `[SitecoreTemplate]`, `[SitecoreField]` — codegen-hook attributes
- `NoOpSitecoreFacade`, `NoOpSitecoreService`, `NoOpAxlisDiagnosticsSink`

**`Axlis.Core`** — domain model, `net8.0`
- `Item`, `ItemTemplate`, `ItemTemplateField` — de-branded Sitecore domain model
- Field types: `TextField`, `ImageField`, `MultilistField`, `ItemReferenceField`, `HyperlinkField`, `BooleanField`, `FileField`
- `ExtendedItem` — base class for strongly-typed template POCOs; ambient `IItemLazyLoader`, `Axes`, `GetField<TField>()`
- `BaseItem` — lightweight wrapper for non-Axes scenarios
- `AxesAdapter` — `Parent`, `Children`, `Siblings`, `GetChildren(predicate)`, `GetDescendants(predicate)`
- `ItemConverter` — GraphQL JSON → `Item` graph conversion
- Built-in templates: `DictionaryEntry`, `DictionaryFolder`, `Folder`

**`Axlis.GraphQL`** — default GraphQL transport, `net8.0`
- `HttpGraphQLTransport` — `IGraphQLTransport` via `HttpClient` + `System.Text.Json`; no third-party GraphQL library
- `HttpGraphQLTransportFactory` — named-`HttpClient` factory with per-site endpoint support
- `AxlisGraphQLOptions` — `Endpoint`, `ApiKey`, `BatchSize`, `TimeoutSeconds`, `SiteEndpoints`
- `SitecoreService` — `ISitecoreService`: `GetItemByPath`, `GetItemById`, `GetItemFlat`, `GetItemsByPaths` (chunked parallel batch)
- `GraphQLQueryBuilder` — batched alias query builder (`item0`, `item1`, …)
- `GraphQLClientException` with `IReadOnlyList<GraphQLError>`

**`Axlis`** — facade + wiring, `net8.0`
- `SitecoreFacade : ISitecoreFacade` — clean (`T?`) + rich (`AxlisResult<T>`) APIs for all four fetch methods
- `SitecoreItemCacheManager` — `ICacheService` wrapper; dual-indexed by ID + path; null-safe miss handling
- `SitecoreItemLazyLoader : IItemLazyLoader` — sync lazy-fetch for `ExtendedItem.Axes`; cache-backed
- `LoggerAxlisDiagnosticsSink : IAxlisDiagnosticsSink` — routes events to `ILogger`
- `AxlisOptions` — `CacheTtl` (default 60 min), `EnableDiagnostics` (default true)
- `AddAxlis()`, `AddAxlisGraphQL()`, `UseAxlis()` — DI extension methods

**Docs**
- `/docs`: `Architecture.md`, `GettingStarted.md`, `Templates.md`, `Axes.md`, `Caching.md`
- Per-package READMEs for NuGet

---

[Unreleased]: https://github.com/marioarce/Axlis/compare/v0.1.0-preview...HEAD
[0.1.0-preview]: https://github.com/marioarce/Axlis/releases/tag/v0.1.0-preview
