# CLAUDE.md — Axlis.ORM.Abstractions

Scoped guidance for this project. Inherits everything in the repo-root [`CLAUDE.md`](../../CLAUDE.md) — this file only adds project-specific constraints; it never loosens the root rules (de-branding, exception-bubbling, GitFlow).

## Role of This Project

`Axlis.ORM.Abstractions` is the **contracts-only compatibility floor** of the ecosystem: `IItem`, `IItemTemplate`, `IItemTemplateField`, field interfaces (`ITextField`, `IImageField`, `IMultilistField`, `IItemReferenceField`, `IHyperlinkField`, `IBooleanField`, `IFileField`), `IBaseItem`/`IExtendedItem`/`IAxes`, `ISitecoreFacade`/`ISitecoreService`, `IGraphQLTransport`/`IGraphQLTransportFactory`, `ISiteContext`/`ISiteResolver`, `AxlisResult<T>`, diagnostics types, codegen attributes, and NoOp implementations.

Every other project in the solution depends on this one. This one depends on nothing internal.

## Hard Constraints

- **`TargetFrameworks` is `netstandard2.0;net8.0` — both, always.** This is the one project in the solution that must build for .NET Framework Sitecore consumers via `netstandard2.0`. Before adding any API, mentally check it against the [netstandard2.0 API set](https://learn.microsoft.com/dotnet/standard/net-standard) — no `Span<T>`-heavy APIs, no C# language features that don't downlevel cleanly, no `System.Text.Json` types in public signatures (that's a `net8.0`-only-friendly library; if a contract needs a JSON payload type, use `object`/`string` or push the concrete typing to `Axlis.ORM.Core`).
- **Zero third-party package references.** The only allowed dependency is `Microsoft.Extensions.Logging.Abstractions`. Do not add anything else here — if a new contract seems to need a third-party type, that's a signal the type belongs in `Axlis.ORM.Core` or `Axlis.ORM.GraphQL` instead, referenced only from an interface's *implementation*, not its *signature*.
- **This project must never reference `Axlis.ORM.Core`, `Axlis.ORM.GraphQL`, or `Axlis.ORM`.** It is the innermost layer. If you find yourself wanting to reference a concrete type from a higher layer, the abstraction is incomplete — model the missing contract here instead.
- **NoOp implementations live here, not in the facade.** `NoOpSitecoreFacade`, `NoOpSitecoreService`, `NoOpAxlisDiagnosticsSink` exist so any layer can depend on a working default without pulling in the real implementation. Keep new NoOps here, fully inert (no I/O, no state, safe to construct with zero config).
- **Codegen attributes (`[SitecoreTemplate]`, `[SitecoreField]`) are a public contract with forward compatibility in mind.** A future Roslyn/CLI generator will read them reflectively. Do not change their shape (constructor signature, property names) without treating it as a breaking change — these attributes are applied to every consumer's generated/hand-written template class.

## What Belongs Here vs. Elsewhere

| If you're adding... | It goes here if... | Otherwise it goes in... |
|---|---|---|
| A new field *contract* (e.g. `IDateField`) | The shape is a plain interface with primitive/`IItem`-graph members | — |
| A new field *implementation* (e.g. `DateField : BaseField`) | Never | `Axlis.ORM.Core/FieldTypes/` |
| A new transport contract | It's an interface (`IGraphQLTransport`-style) | — |
| A new transport implementation | Never | `Axlis.ORM.GraphQL/Transport/` |
| A new diagnostics event shape | It's a POCO/interface with no behavior | — |

## Formatting Constraint

All public members require XML doc comments — this project ships as the smallest, most-referenced package in the family (including into .NET Framework code where IntelliSense-visible docs matter most). Treat missing/thin doc comments here as more serious than elsewhere.

## See Also

- [`WORKFLOWS.md`](WORKFLOWS.md) — SOPs specific to this project.
- Root [`CLAUDE.md`](../../CLAUDE.md) §3–6 for architecture, fragile zones, coding standards, and Git conventions.
