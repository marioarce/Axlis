# WORKFLOWS.md — Axlis.ORM.Abstractions

Project-scoped SOPs. See the repo-root [`WORKFLOWS.md`](../../WORKFLOWS.md) for the 3 repo-wide SOPs (new field/template, data-pipeline fix, release) — these are narrower procedures specific to this project.

## SOP A1 — Adding a New Contract (Interface)

1. Decide the target folder by concern: `Domain/` (item shape), `Fields/` (field contracts), `Model/` (base item / axes / cache-key), `Services/` (facade/service contracts), `Transport/` (GraphQL transport contracts), `Site/` (multi-site), `Results/` (diagnostics/result types).
2. Write the interface targeting the lowest common denominator: no `net8.0`-only types, no third-party types in the signature.
3. If the contract needs a default/no-op behavior for consumers who haven't wired a real implementation, add a matching class under `NoOp/` in the same PR — don't leave callers to `null`-check a missing service.
4. Add XML doc comments to the interface and every member — this is the most-visible package for IntelliSense.
5. No test project exists for `Axlis.ORM.Abstractions` directly (it's contracts-only); verify correctness by building against both TFMs: `dotnet build src/Axlis.ORM.Abstractions/Axlis.ORM.Abstractions.csproj --configuration Release`, and confirm downstream projects (`Axlis.ORM.Core`, etc.) still compile against the new/changed contract.
6. Commit: `feat(#N): add I<Contract> to Abstractions`.

## SOP A2 — Changing an Existing Contract (Breaking-Change Review)

Because every project depends on this one, a signature change here ripples through the entire dependency graph.

1. Search the whole solution for implementers/consumers before changing anything: `grep -rn "IYourInterface" src/ tests/`.
2. Prefer **additive** changes: a new member with a default implementation (where the target framework allows default interface members) or a new overload, rather than changing an existing member's signature.
3. If a breaking change is unavoidable, it must be called out explicitly in the PR description as `BREAKING CHANGE:` (Conventional Commits footer) and reflected in `CHANGELOG.md` under the next `[Unreleased]` entry.
4. Update every implementer in the same PR — do not leave the solution in a half-migrated state across commits.
5. Re-run the full solution build + test (`dotnet build Axlis.ORM.sln` / `dotnet test Axlis.ORM.sln`), not just this project, since the blast radius is solution-wide by definition.

## SOP A3 — Adding a NoOp Implementation

1. Confirm the real interface already exists and is stable.
2. Implement every member as a true no-op: return default/empty values, never throw, never allocate meaningfully, no I/O.
3. Name it `NoOp<InterfaceNameWithoutI>` (e.g. `NoOpSitecoreService` for `ISitecoreService`) to match the existing convention.
4. Document on the class that it is the safe-off default and where it's registered by default (usually in `Axlis.ORM`'s DI extensions) so a reader can find where to swap in a real implementation.
