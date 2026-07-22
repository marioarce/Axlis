# Axlis.ORM

![Axlis Banner](https://raw.githubusercontent.com/marioarce/Axlis/refs/heads/main/assets/banner.png)

The Sitecore Headless GraphQL ORM for .NET 8 — a strongly-typed, Synthesis-style item model built on raw `HttpClient` + `System.Text.Json`. No third-party GraphQL library required.

> Part of the [PowerCSharp](https://github.com/marioarce/PowerCSharp) ecosystem. Integrates with `PowerCSharp.Feature.Cache` for stampede-safe item caching.

---

## Package Family

| Package | Description | TFMs |
|---|---|---|
| [`Axlis.ORM`](https://www.nuget.org/packages/Axlis.ORM) | Facade, cache manager, DI wiring (`AddAxlisORM`) | `net8.0` |
| [`Axlis.ORM.GraphQL`](https://www.nuget.org/packages/Axlis.ORM.GraphQL) | Default `HttpClient`+STJ transport, `SitecoreService`, query builder | `net8.0` |
| [`Axlis.ORM.Core`](https://www.nuget.org/packages/Axlis.ORM.Core) | Domain model, field types, `ExtendedItem`, `AxesAdapter`, `ItemConverter` | `net8.0` |
| [`Axlis.ORM.Abstractions`](https://www.nuget.org/packages/Axlis.ORM.Abstractions) | Contracts, NoOps, `AxlisResult<T>`, codegen-hook attributes | `netstandard2.0` + `net8.0` |

---

## Install

```bash
dotnet add package Axlis.ORM
dotnet add package Axlis.ORM.GraphQL
```

Or à la carte — reference only what you need.

---

## Quick Start

```csharp
// Program.cs
// AddAxlisORM/AddAxlisORMGraphQL take an Action<TOptions> delegate — bind from
// IConfiguration explicitly, or set values inline.
builder.Services
    .AddAxlisORM(o => builder.Configuration.GetSection("Axlis").Bind(o))
    .AddAxlisORMGraphQL(o => builder.Configuration.GetSection("AxlisGraphQL").Bind(o));

var app = builder.Build();
app.Services.UseAxlis(); // wires the ambient lazy-loader for Axes traversal

// appsettings.json
{
  "Axlis": {
    "CacheTtl": "01:00:00",
    "EnableDiagnostics": true
  },
  "AxlisGraphQL": {
    "Endpoint": "https://your-sitecore-instance/sitecore/api/graph/edge",
    "ApiKey": "your-sc_apikey-here"
  }
}
```

```csharp
// Your strongly-typed template
[SitecoreTemplate("{6D1CD897-1936-4A3A-A511-289A94C2A7B1}")]
public class DictionaryEntry : ExtendedItem
{
    [SitecoreField("Key")]
    public TextField Key => GetField<TextField>("Key");

    [SitecoreField("Phrase")]
    public TextField Phrase => GetField<TextField>("Phrase");
}

// Fetch an item
var item = await facade.GetItemByPathAsync<DictionaryEntry>("/sitecore/content/dictionary/my-key");
Console.WriteLine(item?.Phrase.RawValue);

// Axes traversal
var children  = item?.Axes.Children;
var parent    = item?.Axes.Parent;
var siblings  = item?.Axes.Siblings;

var pages = item?.Axes.GetChildren<MyPage>(i => i.InnerItem?.Template?.Id == MyPage.TemplateId);

// Rich result with metadata and diagnostics
var result = await facade.GetItemByPathWithResultAsync<DictionaryEntry>("/sitecore/content/dictionary/my-key");
Console.WriteLine(result.Metadata?.ItemPath);
```

---

## What's Included

- `SitecoreFacade : ISitecoreFacade` — clean + rich (`WithResult`) APIs for all four fetch methods
- `SitecoreItemCacheManager` — stampede-safe `ICacheService` wrapper; dual-indexes by ID + path
- `SitecoreItemLazyLoader : IItemLazyLoader` — lazy-fetch for `ExtendedItem.Axes` traversal
- `LoggerAxlisDiagnosticsSink` — default `IAxlisDiagnosticsSink` routing events to `ILogger`
- `AddAxlisORM()` / `AddAxlisORMGraphQL()` / `UseAxlis()` — DI registration helpers
- `AxlisOptions` — `CacheTtl`, `EnableDiagnostics`

---

## Documentation

Full Axlis.ORM documentation:

- [Architecture](../../docs/orm/Architecture.md)
- [Getting Started](../../docs/orm/GettingStarted.md)
- [Templates Guide](../../docs/orm/Templates.md)
- [Axes Guide](../../docs/orm/Axes.md)
- [Caching](../../docs/orm/Caching.md)

---

## Sample App

See **[Axlis.CleanArchitecture.Sample](https://github.com/marioarce/Axlis.CleanArchitecture.Sample)** — a full working consumer built on [PowerCSharp.CleanArchitecture](https://github.com/marioarce/PowerCSharp.CleanArchitecture).

See the [Axlis repository](https://github.com/marioarce/Axlis) for ecosystem information.
