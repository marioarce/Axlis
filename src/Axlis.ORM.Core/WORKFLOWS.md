# WORKFLOWS.md — Axlis.ORM.Core

Project-scoped SOPs. See the repo-root [`WORKFLOWS.md`](../../WORKFLOWS.md) for the 3 repo-wide SOPs — this file adds narrower procedures specific to the domain model.

## SOP C1 — Adding a New Field Type

(Narrower version of repo-root SOP 1, steps 2–3, scoped to this project.)

1. Confirm the contract exists in `Axlis.ORM.Abstractions/Fields/` (add it there first if not — see that project's `WORKFLOWS.md` SOP A1).
2. Add the class under `FieldTypes/`, deriving from `BaseField`, with a constructor taking a single `ItemTemplateField` — this is a hard runtime requirement, not a style preference (see this project's `CLAUDE.md` §"Formatting Constraint").
3. If the field needs new raw data from GraphQL (a scalar/shape not already on `GraphQLFieldData`), extend that class in `GraphQL/GraphQLFieldData.cs` and map it in `ItemConverter.ConvertField` — read [`ItemConverter.md`](ItemConverter.md) first.
4. Add a test in `tests/Axlis.ORM.Core.Tests/FieldTypes/`, covering: populated value, missing/empty field via `ItemTemplateField.Empty(fieldName)`, and any lazy-loaded sub-graph (target items) if applicable.

## SOP C2 — Adding a New Built-In Template

1. Add the class under `Templates/<Category>/` (`Common/` or `System/` today; add a new category folder if the template doesn't fit either).
2. Derive from `ExtendedItem`, decorate with `[SitecoreTemplate("{template-guid}")]`.
3. Expose each field as a property using `protected TField GetField<TField>(string fieldName)`.
4. Add a test under `tests/Axlis.ORM.Core.Tests/Model/` or a new `Templates/` test folder, following the pattern in `ExtendedItemTests.cs`.
5. De-branding check: template GUIDs and names must be generic/Sitecore-standard, never referencing a specific prior client implementation.

## SOP C3 — Touching `ItemConverter`, `AxesAdapter`, or `ExtendedItem`

These three files are this project's highest-risk surface (see `CLAUDE.md` §"Fragile Zones"). Before any change:

1. Read the relevant module spec in full: [`ItemConverter.md`](ItemConverter.md) or [`AxesLazyLoading.md`](AxesLazyLoading.md).
2. Write the regression test **before** the fix/feature, using `ItemConverterTests.cs` or `ExtendedItemTests.cs` as the template for test shape.
3. If the change affects recursion depth, circular-reference handling, or the ambient static loader's lifecycle, call this out explicitly in the PR description — these are exactly the properties the module specs document as invariants.
4. Run the full `Axlis.ORM.Core.Tests` suite locally (`dotnet test tests/Axlis.ORM.Core.Tests`), not just the new test — these files are exercised indirectly by many other tests (e.g. `ItemTests.cs`, `ItemTemplateTests.cs`).
