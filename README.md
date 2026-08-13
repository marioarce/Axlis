# Axlis

![Axlis Banner](https://raw.githubusercontent.com/marioarce/Axlis/refs/heads/main/assets/banner.png)

[![CI](https://github.com/marioarce/Axlis/actions/workflows/ci.yml/badge.svg)](https://github.com/marioarce/Axlis/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Axlis.ORM?label=Axlis.ORM)](https://www.nuget.org/packages/Axlis.ORM)
[![NuGet](https://img.shields.io/nuget/v/Axlis.Customizations.Controls.Sitecore102?label=Axlis.Customizations)](https://www.nuget.org/packages/Axlis.Customizations.Controls.Sitecore102)
[![NuGet](https://img.shields.io/nuget/v/Axlis.Sitecore.Context.Sitecore102?label=Axlis.Sitecore.Context)](https://www.nuget.org/packages/Axlis.Sitecore.Context.Sitecore102)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Website](https://img.shields.io/badge/website-axlis.dev-blue)](https://axlis.dev/)

**Axlis** is a Sitecore ecosystem for .NET 8 — a collection of enterprise-grade libraries and tools for Sitecore development.

**Website:** [axlis.dev](https://axlis.dev/)

---

## Ecosystem Components

### Axlis.ORM

The first component in the Axlis ecosystem — a Sitecore Headless GraphQL ORM for .NET 8. A strongly-typed, Synthesis-style item model built on raw `HttpClient` + `System.Text.Json`. No third-party GraphQL library required.

> Part of the [PowerCSharp](https://github.com/marioarce/PowerCSharp) ecosystem. Integrates with `PowerCSharp.Feature.Cache` for stampede-safe item caching.

**Package Family:**

| Package | Description | TFMs |
|---|---|---|
| [`Axlis.ORM`](https://www.nuget.org/packages/Axlis.ORM) | Facade, cache manager, DI wiring (`AddAxlisORM`) | `net8.0` |
| [`Axlis.ORM.GraphQL`](https://www.nuget.org/packages/Axlis.ORM.GraphQL) | Default `HttpClient`+STJ transport, `SitecoreService`, query builder | `net8.0` |
| [`Axlis.ORM.Core`](https://www.nuget.org/packages/Axlis.ORM.Core) | Domain model, field types, `ExtendedItem`, `AxesAdapter`, `ItemConverter` | `net8.0` |
| [`Axlis.ORM.Abstractions`](https://www.nuget.org/packages/Axlis.ORM.Abstractions) | Contracts, NoOps, `AxlisResult<T>`, codegen-hook attributes | `netstandard2.0` + `net8.0` |

See [Axlis.ORM Documentation](src/Axlis.ORM/README.md) for installation and usage.

### Axlis.Customizations

Sitecore Content Editor / Shell control customizations — field types, TreeList variants, and similar Sitecore Client-side extensions. Unlike `Axlis.ORM`, this family targets `net48` directly against real `Sitecore.Kernel`/`Sitecore.Web` packages (from Sitecore's [public NuGet feed](https://nuget.sitecore.com/resources/v3/index.json)), since these customizations extend Sitecore Shell controls that only exist in that runtime. Packages are gated per Sitecore major.minor line (e.g. `.Sitecore102`) rather than floated across versions — see [`Axlis.Customizations.Controls.Sitecore102`'s README](src/Axlis.Customizations/Axlis.Customizations.Controls.Sitecore102/README.md) for why.

**Package Family:**

| Package | Description | TFMs |
|---|---|---|
| [`Axlis.Customizations.Abstractions`](https://www.nuget.org/packages/Axlis.Customizations.Abstractions) | Shared, Sitecore-free constants for the customization family | `netstandard2.0` + `net8.0` |
| [`Axlis.Customizations.Controls.Sitecore102`](https://www.nuget.org/packages/Axlis.Customizations.Controls.Sitecore102) | `QueryableTreeList` and future Content Editor / Shell controls, built against Sitecore 10.2.x | `net48` |

Released independently from `Axlis.ORM` via tags matching `customizations-v*` (not `v*`) — see [`WORKFLOWS.md`](WORKFLOWS.md) SOP 4.

### Axlis.Sitecore.Context

A thread-safe, per-request replacement for `Sitecore.Context.Database`, `Sitecore.Context.Request`, and `HttpContext.Current` — all of which are unreliable in multi-threaded scenarios, where they can return `null` mid-request once code hops off the physical thread ASP.NET started the request on. Like `Axlis.Customizations`, this family targets `net48` directly against real `Sitecore.Kernel`/`Sitecore.Web`, gated per Sitecore major.minor line.

**Package Family:**

| Package | Description | TFMs |
|---|---|---|
| [`Axlis.Sitecore.Context.Abstractions`](https://www.nuget.org/packages/Axlis.Sitecore.Context.Abstractions) | `AmbientContextStore<T>` — the Sitecore-free, per-logical-call propagation mechanism | `net48` |
| [`Axlis.Sitecore.Context.Sitecore102`](https://www.nuget.org/packages/Axlis.Sitecore.Context.Sitecore102) | `Axlis.Sitecore.Context.Database`/`.Request`/`.HttpContext` + the capturing HTTP module, built against Sitecore 10.2.x | `net48` |

Released independently via tags matching `sitecore-context-v*`. See [`src/Axlis.Sitecore.Context/Axlis.Sitecore.Context.Sitecore102`'s README](src/Axlis.Sitecore.Context/Axlis.Sitecore.Context.Sitecore102/README.md) for setup, and [`docs/sitecore-context/Architecture.md`](docs/sitecore-context/Architecture.md) for why the underlying mechanism is thread-safe.

A runnable-once-deployed usage example lives in [`samples/Axlis.Sitecore.Context.Samples`](samples/Axlis.Sitecore.Context.Samples/README.md) — an illustrative Sitecore website project with a page that simulates background threads and shows the raw `Sitecore.Context`/`HttpContext.Current` statics going `null` side-by-side with `Axlis.Sitecore.Context` staying correct. Kept in its own solution, outside `Axlis.Sitecore.Context.sln` and its release CI. **Windows Visual Studio only** (ASP.NET and web development workload) — it's a legacy Web Application Project so it supports `Publish → Folder` to IIS, and will not open or build in Visual Studio for Mac or other cross-platform tooling.

---

## Roadmap

### Planned Components

- **Axlis.Context** — reserved for a future, more general ambient-context/ecosystem-wiring component; not to be confused with the already-shipped `Axlis.Sitecore.Context` above, which solves the specific `Sitecore.Context` thread-safety problem
- **Axlis.Diagnostics** — Enhanced diagnostics and monitoring for Sitecore applications
- **Axlis.Caching** — Advanced caching strategies for Sitecore data
- **Axlis.Customizations.{xyz}** — Additional Sitecore field type / Shell control customizations beyond `QueryableTreeList`

---

## Sample App

See **[Axlis.CleanArchitecture.Sample](https://github.com/marioarce/Axlis.CleanArchitecture.Sample)** — a full working consumer built on [PowerCSharp.CleanArchitecture](https://github.com/marioarce/PowerCSharp.CleanArchitecture).

---

## Documentation

- [Axlis.ORM Documentation](src/Axlis.ORM/README.md)
- [Axlis.ORM Architecture](docs/orm/Architecture.md)
- [Axlis.ORM Getting Started](docs/orm/GettingStarted.md)
- [Axlis.ORM Templates Guide](docs/orm/Templates.md)
- [Axlis.ORM Axes Guide](docs/orm/Axes.md)
- [Axlis.ORM Caching](docs/orm/Caching.md)
- [Axlis.Customizations.Abstractions Documentation](src/Axlis.Customizations/Axlis.Customizations.Abstractions/README.md)
- [Axlis.Customizations.Controls.Sitecore102 Documentation](src/Axlis.Customizations/Axlis.Customizations.Controls.Sitecore102/README.md)
- [Axlis.Sitecore.Context.Abstractions Documentation](src/Axlis.Sitecore.Context/Axlis.Sitecore.Context.Abstractions/README.md)
- [Axlis.Sitecore.Context.Sitecore102 Documentation](src/Axlis.Sitecore.Context/Axlis.Sitecore.Context.Sitecore102/README.md)
- [Axlis.Sitecore.Context Architecture](docs/sitecore-context/Architecture.md)
- [Axlis.Sitecore.Context.Samples](samples/Axlis.Sitecore.Context.Samples/README.md)
- [GitFlow Workflow](docs/WORKFLOW.md)

---

## Support

- **Website** — [axlis.dev](https://axlis.dev/)
- **GitHub Discussions** — Use for questions, ideas, and community discussions
- **GitHub Issues** — Use for bug reports and feature requests

**When to use Discussions:**
- Questions about configuration, usage, or best practices
- Ideas for new features or improvements
- Sharing your implementation or asking for guidance
- General conversations about Axlis

**When to use Issues:**
- Bug reports with reproducible steps
- Specific feature requests with clear requirements
- Security vulnerabilities (see SECURITY.md)

---

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for branch strategy, commit format, and PR checklist.

## License

MIT — see [LICENSE](LICENSE).
