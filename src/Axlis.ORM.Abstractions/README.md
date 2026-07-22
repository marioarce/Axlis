# Axlis.ORM.Abstractions

![Axlis Banner](https://raw.githubusercontent.com/marioarce/Axlis/refs/heads/main/assets/banner.png)

Contracts-only package for [Axlis.ORM](https://github.com/marioarce/Axlis), the Sitecore Headless GraphQL ORM.

Targets `netstandard2.0` + `net8.0`. Zero third-party dependencies — safe to reference from any layer.

## What's in here

- `IItem`, `IItemTemplate`, `IItemTemplateField` — core Sitecore item contracts
- `IBaseItem`, `IExtendedItem`, `IAxes` — ORM model contracts
- `IBaseField` + typed field contracts (`ITextField`, `IImageField`, `IMultilistField`, …)
- `IGraphQLTransport` / `IGraphQLTransportFactory` — pluggable transport seam
- `ISiteContext` / `ISiteResolver` — opt-in multi-site abstraction
- `ISitecoreService` / `ISitecoreFacade` — data-access contracts
- `AxlisResult<T>`, `SitecoreMetadata`, `AxlisDiagnostics` — result envelope
- `IAxlisDiagnosticsSink` — pluggable diagnostics sink
- `[SitecoreTemplate]`, `[SitecoreField]` — codegen-hook attributes
- `NoOp` safe-off implementations

## Install

```
dotnet add package Axlis.ORM.Abstractions
```

See the [Axlis repository](https://github.com/marioarce/Axlis) for full documentation.
