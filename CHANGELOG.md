# Changelog

All notable changes to the Axlis package family are documented here.

Format: [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).
Versioning: [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

## [0.1.0-preview] — TBD

### Added
- `Axlis.Abstractions` — contracts, NoOps, `AxlisResult<T>`, `SitecoreMetadata`, `AxlisDiagnostics`, `IAxlisDiagnosticsSink`, `IGraphQLTransport`, `ISiteContext`, codegen-hook attributes (`[SitecoreTemplate]`, `[SitecoreField]`)
- `Axlis.Core` — `Item`, field types, `ExtendedItem`, `AxesAdapter`, `ItemConverter`, built-in System/Common templates
- `Axlis.GraphQL` — default `IGraphQLTransport` via raw `HttpClient` + `System.Text.Json`, `SitecoreService`, `GraphQLQueryBuilder` with alias-batching
- `Axlis` — `SitecoreFacade` (clean + WithResult APIs), `IItemLazyLoader`, `SitecoreItemCacheManager`, `AddAxlis()` DI extension

[Unreleased]: https://github.com/marioarce/Axlis/compare/v0.1.0-preview...HEAD
[0.1.0-preview]: https://github.com/marioarce/Axlis/releases/tag/v0.1.0-preview
