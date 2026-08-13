# Changelog

All notable changes to the Axlis package family are documented here.

Format: [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).
Versioning: [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

---

## [0.4.0] — 2026-08-12

### Added

- Added the `Axlis.Sitecore.Context` package family to the repository:
  - `Axlis.Sitecore.Context.Abstractions` (`net48`) — `AmbientContextStore<T>`, a Sitecore-free, per-logical-call ambient value store built on `MarshalByRefObject` + `ILogicalThreadAffinative` + `CallContext`, the same mechanism that keeps `Sitecore.Context.Database`/`HttpContext.Current` thread-safe when generalized.
  - `Axlis.Sitecore.Context.Sitecore102` (`net48`) — `Axlis.Sitecore.Context.Database`/`.Request`/`.HttpContext`, a thread-safe, per-request replacement for `Sitecore.Context.Database`, `Sitecore.Context.Request`, and `HttpContext.Current`, plus the `SitecoreContextHttpModule` that captures them once per request. Built against Sitecore 10.2.x (`Sitecore.Kernel`/`Sitecore.Web` `[10.2.0,10.3.0)`).
  - `Axlis.Sitecore.Context.sln` and `Axlis.Sitecore.Context.Abstractions.Tests` for isolated family development and testing.
  - `docs/sitecore-context/Architecture.md`, explaining why the `CallContext`/`ILogicalThreadAffinative` propagation mechanism is thread-safe.
  - `samples/Axlis.Sitecore.Context.Samples` — an illustrative Sitecore website (a legacy Web Application Project, Windows Visual Studio only — see its `README.md`) with a live thread-safety demo page: it captures the raw ambient statics and their `Axlis.Sitecore.Context` equivalents on the original request thread, then again from inside several simulated background threads, and renders both side by side. Verified end-to-end against a real Sitecore 10.2.x CM instance running on Visual Studio 2026.

### Changed

- Added `AxlisSitecoreContextVersion` (`0.1.0`) to centralized version management in `Directory.Build.props`.
- Added `.github/workflows/release-sitecore-context.yml`, released independently via `sitecore-context-v*` tags — see `WORKFLOWS.md` SOP 5 for this family's release procedure.
- Added `build-customizations` and `build-sitecore-context` jobs to `ci.yml` so both `net48` package families are actually built and tested on `windows-latest` for every push/PR to `develop`/`main`. Previously `ci.yml` only built/tested `Axlis.ORM.sln`, even though `WORKFLOWS.md` already documented a `build-customizations` CI gate — closed that gap for both families at once.
- Updated root `README.md` and `CLAUDE.md` for the new package family, including the roadmap distinction between the shipped `Axlis.Sitecore.Context` and the still-reserved, unrelated `Axlis.Context` placeholder name. Added NuGet version badges for `Axlis.Customizations` and `Axlis.Sitecore.Context` to the root `README.md` (previously only `Axlis.ORM` had one).
- Added `.vsconfig` next to `Axlis.Sitecore.Context.Samples.sln`, declaring the ASP.NET and web development workload the Samples project requires, and documented that project's Windows-only build requirement across `CLAUDE.md`, the root `README.md`, and its own `README.md`.

### Fixed

- Corrected the Sitecore HTTP module registration guidance before it ever shipped: the module belongs in the consuming site's real `Web.config` (`<system.webServer><modules>`), not a Sitecore `App_Config` include — the two are separate configuration systems, and a module registered only via `App_Config` is silently inert.
- Excluded NuGet-restored assemblies (`Sitecore.Kernel`, `Sitecore.Web`, and their transitive dependencies) from the Samples project's own build/publish output, since a real Sitecore site already ships its own matched set of those assemblies — publishing this project's independently-restored copies on top caused a runtime `FileLoadException` from assembly version drift the one time this was tried against a live site.

---

## [0.3.0] — 2026-08-05

### Added

- Added the `Axlis.Customizations` package family scaffold to the repository:
  - `Axlis.Customizations.Abstractions` (`netstandard2.0` + `net8.0`) with shared constants for customization controls.
  - `Axlis.Customizations.Controls.Sitecore102` (`net48`) with the first Sitecore Shell control implementation: `QueryableTreeList`.
  - `Axlis.Customizations.sln` and `Axlis.Customizations.Abstractions.Tests` for isolated family development and testing.

### Changed

- Added `AxlisCustomizationsVersion` to centralized version management in `Directory.Build.props`.
- Added root `nuget.config` with Sitecore feed support required to restore Sitecore 10.2 package dependencies for customizations.
- Updated repository and workflow documentation for the new package family, including release SOP coverage in `WORKFLOWS.md`.

---

## [0.2.0] — 2026-07-10

### Changed

- **BREAKING:** Renamed the entire package family for ecosystem clarity ahead of additional Axlis components (`Axlis.Context`, `Axlis.Diagnostics`, `Axlis.Caching`):
  - `Axlis` → `Axlis.ORM`
  - `Axlis.Core` → `Axlis.ORM.Core`
  - `Axlis.GraphQL` → `Axlis.ORM.GraphQL`
  - `Axlis.Abstractions` → `Axlis.ORM.Abstractions`
- Namespaces updated throughout to match the new package names (e.g. `namespace Axlis.Core;` → `namespace Axlis.ORM.Core;`).
- DI extension methods renamed: `AddAxlis()` → `AddAxlisORM()`, `AddAxlisGraphQL()` → `AddAxlisORMGraphQL()`. `UseAxlis()` is unchanged.
- Solution restructuring: `Axlis.ORM.sln` is now the live, buildable solution (4 src + 3 test projects); `Axlis.sln` repurposed as empty scaffolding reserved for future ecosystem components.
- `docs/` reorganized: ORM-specific guides moved under `docs/orm/` (`Architecture.md`, `GettingStarted.md`, `Templates.md`, `Axes.md`, `Caching.md`); added `docs/WORKFLOW.md` (GitFlow SOP).
- CI/CD workflows updated to build/test/pack `Axlis.ORM.sln`.
- Per-package `README.md`s rewritten for the new package identities.

> **Note on versioning:** this tag was not accompanied by a `PackageVersion` bump — `Directory.Build.props` still reports `0.1.0` for every package, so the artifacts published under this milestone are `Axlis.ORM.*` version `0.1.0` (new package IDs at their first version), not a distinct `0.2.0` release. The `v0.2.0` git tag marks the rename in history; it does not correspond to a `0.2.0` NuGet version. A real version bump is still owed before the next tag.

---

## [0.1.0] — 2026-07-08

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

[Unreleased]: https://github.com/marioarce/Axlis/compare/v0.4.0...HEAD
[0.4.0]: https://github.com/marioarce/Axlis/compare/v0.3.0...v0.4.0
[0.3.0]: https://github.com/marioarce/Axlis/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/marioarce/Axlis/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/marioarce/Axlis/releases/tag/v0.1.0
