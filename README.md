# Axlis

![Axlis Banner](https://raw.githubusercontent.com/marioarce/Axlis/refs/heads/main/assets/banner.png)

[![CI](https://github.com/marioarce/Axlis/actions/workflows/ci.yml/badge.svg)](https://github.com/marioarce/Axlis/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Axlis.ORM?label=Axlis.ORM)](https://www.nuget.org/packages/Axlis.ORM)
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

---

## Roadmap

### Planned Components

- **Axlis.Context** — Thread-safe Sitecore context implementation solving the non-thread-safe nature of `Sitecore.Context` in multi-threaded scenarios
- **Axlis.Diagnostics** — Enhanced diagnostics and monitoring for Sitecore applications
- **Axlis.Caching** — Advanced caching strategies for Sitecore data

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