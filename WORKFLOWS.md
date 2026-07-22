# WORKFLOWS.md — Axlis (Repository-Wide SOPs)

Three standard operating procedures for the recurring task categories in this repo. These are repo-wide; each project also has its own `WORKFLOWS.md` with narrower, project-scoped procedures. Follow `CLAUDE.md` at the same directory level for the standards referenced below (coding style, de-branding, GitFlow).

---

## SOP 1 — Adding a New Field Type or Built-In Template

**When to use:** A new Sitecore field type needs a typed wrapper (like `TextField`, `ImageField`), or a new built-in template (like `DictionaryEntry`, `Folder`) needs to ship in `Axlis.ORM.Core`.

1. **Branch.** `git checkout develop && git pull && git checkout -b feature/<issue>-<short-description>`.
2. **Contract first (`Axlis.ORM.Abstractions`).** If this is a new field *type* (not just a new built-in template), add the interface under `Fields/` (e.g. `INewField : IBaseField`). Keep it `netstandard2.0`-safe — no `net8.0`-only members.
3. **Implementation (`Axlis.ORM.Core`).**
   - Field types: add the class under `FieldTypes/`, deriving from `BaseField`, following the existing constructor pattern that takes an `ItemTemplateField`.
   - Built-in templates: add the class under `Templates/<Category>/`, deriving from `ExtendedItem`, decorated with `[SitecoreTemplate("{guid}")]`, with properties using `[SitecoreField]` + `GetField<TField>(name)`.
4. **Wire conversion if needed.** If the new field type requires new GraphQL data (new scalar, new sub-object), extend `GraphQLFieldData` and the corresponding `ConvertField` mapping in `ItemConverter.cs` — see [`src/Axlis.ORM.Core/ItemConverter.md`](src/Axlis.ORM.Core/ItemConverter.md) before touching this file, it's a fragile zone.
5. **Tests.** Add a test class under `tests/Axlis.ORM.Core.Tests/FieldTypes/` (or `Templates/`) mirroring the existing pattern (`BooleanFieldTests.cs`, `MultilistFieldTests.cs`, etc.). Cover: normal value, null/missing field (must return the `Empty` sentinel, never throw), and — for reference-style fields — the lazy-target-loading flag (`isTargetItemLoaded` / `isTargetItemsLoaded`).
6. **XML docs.** Every new public type/member needs a doc comment — the build fails on missing docs (`GenerateDocumentationFile`).
7. **De-branding sweep.** Grep the diff for the banned terms in root `CLAUDE.md` §2 before committing.
8. **Build + test gate.** `dotnet build Axlis.ORM.sln --configuration Release` (0 warnings/errors) then `dotnet test Axlis.ORM.sln --configuration Release`.
9. **Commit + PR.** Conventional Commit with issue scope, e.g. `feat(#N): add DateField type with timezone-safe parsing`. PR → `develop`, body includes `Closes #N`.

---

## SOP 2 — Modifying or Fixing a Bug in the Data Pipeline

**When to use:** A bug report or change request touches any part of the request path: `SitecoreFacade` → `SitecoreItemCacheManager` → `ISitecoreService` → `IGraphQLTransport` → `ItemConverter`.

This is the highest-blast-radius category of change in the repo — a regression here silently corrupts data for every downstream consumer, since the ORM is a pure pass-through data layer with no business logic to catch bad output.

1. **Branch.** `feature/<issue>-<short-description>` from `develop` (or `fix/<issue>-...` locally if you prefer, but the PR still targets `develop` per GitFlow).
2. **Reproduce with a failing test first.** Identify which layer actually owns the bug before touching code:
   - Wrong/missing data in the typed POCO → likely `ItemConverter` or a field type's constructor.
   - Cache returning stale or wrong item → `SitecoreItemCacheManager` (check key normalization: ID vs. path, case-sensitivity — it uses `StringComparer.OrdinalIgnoreCase` in indexing).
   - Wrong item for a valid path/ID, or unhandled GraphQL error → `SitecoreService` / `HttpGraphQLTransport`.
   - Add the failing test in the matching `*.Tests` project (`Axlis.ORM.Core.Tests`, `Axlis.ORM.GraphQL.Tests`, or `Axlis.ORM.Tests`) *before* the fix, so the diff proves the bug existed and is now closed.
3. **Fix at the correct layer — do not patch symptoms downstream.** Example: if `ItemConverter` mis-maps a field, fix `ItemConverter`, don't add a workaround in `SitecoreFacade.MapToTyped<T>`.
4. **Respect the exception-bubbling contract.** Do not add a `try/catch` in `Core` or `GraphQL` layers to "handle" the bug quietly — `SitecoreFacade` is the only layer that swallows and converts to `null`/`AxlisResult`. See root `CLAUDE.md` §3.
5. **Check both API flavors.** If the bug is reachable via `ISitecoreFacade`, verify both the Clean (`Get*Async<T>`) and Rich (`Get*WithResultAsync<T>`) methods are fixed and tested — they share the same underlying call but are separate code paths in `SitecoreFacade`.
6. **Regression-test the fragile zones explicitly if touched.** If the change touches `ItemConverter`, `SitecoreItemCacheManager`, or `AxesAdapter`/lazy-loading, follow the "Verification" checklist in that module's `.md` spec, not just a generic unit test.
7. **De-branding + build/test gate** — same as SOP 1, steps 7–8.
8. **Commit + PR.** `fix(#N): <imperative summary>`, e.g. `fix(#41): prevent duplicate cache write when path and ID collide`.

---

## SOP 3 — Cutting a Release

**When to use:** `develop` has accumulated enough merged work to ship a new `Axlis.ORM` package version to NuGet.org / GitHub Packages.

1. **Confirm `develop` is green.** CI (`ci.yml`) must be passing on the latest `develop` commit.
2. **Create the release branch from `develop`.** `git checkout develop && git pull && git checkout -b release/vX.Y.Z`.
3. **Bump versions in `Directory.Build.props`.** Update `AxlisORMVersion` (and `AxlisVersion` if the broader ecosystem version also moves). This is the single source of truth for all four packages' `PackageVersion` — do not hand-edit individual `.csproj` files.
4. **Update `CHANGELOG.md`.** Move `[Unreleased]` entries into a new `[X.Y.Z] — <date>` section, following Keep a Changelog format already used in this file.
5. **Full local verification.**
   ```bash
   dotnet restore Axlis.ORM.sln
   dotnet build Axlis.ORM.sln --configuration Release
   dotnet test Axlis.ORM.sln --configuration Release
   ```
   All three must be clean before proceeding — this mirrors exactly what `release.yml`'s `build-and-test` job will run.
6. **De-branding sweep across the full diff since the last tag**, not just this branch's commits — releases are the last line of defense before the code is public/downloadable as a package artifact.
7. **Documentation pass.** Confirm `docs/orm/*.md` and package `README.md`s reflect the current public API (this repo has shipped with stale naming before — `docs/orm/Caching.md` referenced pre-rename `AddAxlis()`/`AddAxlisGraphQL()` well after the `*.ORM.*` rename; now fixed, but re-check on every release, don't ship known-stale docs).
8. **PR `release/vX.Y.Z` → `main`.** Body includes release notes summarizing the changelog section. Requires review + passing status checks (branch protection).
9. **Merge triggers automation.** On merge to `main`: tag `vX.Y.Z` → `release.yml` runs `build-and-test` → `pack` → `publish` (NuGet.org + GitHub Packages via `dotnet nuget push --skip-duplicate`) → release branch auto-deleted per WORKFLOW.md.
10. **Verify published packages.** Confirm all four packages (`Axlis.ORM`, `Axlis.ORM.GraphQL`, `Axlis.ORM.Core`, `Axlis.ORM.Abstractions`) appear at the new version on both NuGet.org and GitHub Packages before announcing the release.

---

## Cross-Cutting Gate (applies to every SOP above)

Before any PR is marked ready, per `CONTRIBUTING.md`:

- [ ] `dotnet build --configuration Release` — 0 warnings, 0 errors.
- [ ] `dotnet test --configuration Release` — all green.
- [ ] All public APIs have XML doc comments.
- [ ] De-branding sweep clean (see root `CLAUDE.md` §2) — this is checked on **every** PR, not just releases.
- [ ] Commit/PR title follows Conventional Commits with issue scope.
- [ ] PR body includes `Closes #<N>`.
