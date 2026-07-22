# CLAUDE.md — Axlis.ORM.Core

Scoped guidance for this project. Inherits everything in the repo-root [`CLAUDE.md`](../../CLAUDE.md).

## Role of This Project

`Axlis.ORM.Core` is the de-branded domain model: `Item`, `ItemTemplate`, `ItemTemplateField`, the concrete field types (`TextField`, `ImageField`, `MultilistField`, `ItemReferenceField`, `HyperlinkField`, `BooleanField`, `FileField`), `ExtendedItem`/`BaseItem`, `AxesAdapter`, `ItemConverter`, and the built-in templates (`DictionaryEntry`, `DictionaryFolder`, `Folder`).

This is where raw GraphQL JSON becomes a typed object graph, and where consumer template POCOs get their tree-traversal (`Axes`) and field-access (`GetField<TField>`) behavior. **This project contains the two most fragile zones in the entire codebase** — see §"Fragile Zones" below before editing `ItemConverter.cs`, `AxesAdapter.cs`, or `ExtendedItem.cs`.

## Hard Constraints

- **`TargetFramework` is `net8.0` only** (unlike `Abstractions`, which must stay netstandard2.0-compatible). Do not add netstandard constraints here — this project can freely use modern C#/.NET 8 features.
- **Only references `Axlis.ORM.Abstractions`.** Never add a reference to `Axlis.ORM.GraphQL` or `Axlis.ORM` — that would invert the dependency graph documented in the root `CLAUDE.md`.
- **`ItemConverter` is a pure function over `GraphQLItemData`.** It must never perform I/O, caching, or logging — those are the concern of `SitecoreService` (in `Axlis.ORM.GraphQL`) and `SitecoreItemCacheManager` (in `Axlis.ORM`). If you're tempted to add caching logic inside `ItemConverter`, stop — it belongs one layer up.
- **`ExtendedItem` holds a `static` ambient `IItemLazyLoader` field.** This is intentional (see `docs/orm/Architecture.md` — "a DI-injected replacement for the original static factory") but it means: (a) it is shared process-wide, (b) tests that rely on it must call `ExtendedItem.Reset()` in teardown, and (c) parallel test execution that sets different loaders **will race**. Do not "fix" this by silently changing it to instance state without discussing it — it's a deliberate, documented trade-off for lazy-load ergonomics on template POCOs, but any change to it is a cross-cutting behavioral change.

## Fragile Zones — Read the Module Spec First

- [`ItemConverter.md`](ItemConverter.md) — recursive conversion, circular-reference guard, depth semantics for parent vs. children.
- [`AxesLazyLoading.md`](AxesLazyLoading.md) — `AxesAdapter`, the ambient lazy-loader, and the silent exception-swallow in `TryUpdateAxes()`.

## Formatting Constraint

Field type constructors follow one convention across the whole `FieldTypes/` folder: they take a single `ItemTemplateField` and derive their typed value from it. New field types must follow this exact pattern — `GetField<TField>()` in `ExtendedItem` relies on `Activator.CreateInstance(typeof(TField), rawField)` reflectively, so a field type with a different constructor shape will fail at runtime, not compile time.

## See Also

- [`WORKFLOWS.md`](WORKFLOWS.md) — SOPs specific to this project.
- Root [`CLAUDE.md`](../../CLAUDE.md) §4 for the repo-wide framing of these fragile zones.
