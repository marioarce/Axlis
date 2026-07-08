# Axlis

The Sitecore Headless GraphQL ORM for .NET 8 — batteries included. Targets `net8.0`.

Provides `ISitecoreFacade`, `SitecoreItemCacheManager`, `IItemLazyLoader`, and the `AddAxlis` / `AddAxlisGraphQL` DI extension methods. Integrates with `PowerCSharp.Feature.Cache` for stampede-safe item caching.

## Install

```
dotnet add package Axlis
dotnet add package Axlis.GraphQL
```

## Quick start

```csharp
// Program.cs
builder.Services
    .AddAxlis(o => o.CacheTtl = TimeSpan.FromMinutes(30))
    .AddAxlisGraphQL(o =>
    {
        o.Endpoint = builder.Configuration["AxlisGraphQL:Endpoint"]!;
        o.ApiKey   = builder.Configuration["AxlisGraphQL:ApiKey"]!;
    });

app.Services.UseAxlis();   // wires ExtendedItem lazy-loader
```

## Fetch items

```csharp
// Clean API — returns T? (errors logged, never throws)
var item = await _facade.GetItemByPathAsync<ArticlePage>("/sitecore/content/articles/my-article");
Console.WriteLine(item?.Title.RawValue);

// Rich API — returns AxlisResult<T> with Value, Metadata, Diagnostics
var result = await _facade.GetItemByPathWithResultAsync<ArticlePage>("/sitecore/content/articles/my-article");
Console.WriteLine(result.Metadata?.ItemVersion);

// Axes traversal
var children = item?.Axes.Children;
var parent   = item?.Axes.Parent;

// Batch fetch (one GraphQL request with aliased sub-queries)
var pages = await _facade.GetItemsByPathsAsync<ArticlePage>(new[] { "/a", "/b", "/c" });
```

## What's included

- `SitecoreFacade : ISitecoreFacade` — clean + rich (`WithResult`) APIs for all four fetch methods
- `SitecoreItemCacheManager` — stampede-safe `ICacheService` wrapper; dual-indexes by ID + path
- `SitecoreItemLazyLoader : IItemLazyLoader` — lazy-fetch for `ExtendedItem.Axes` traversal
- `LoggerAxlisDiagnosticsSink` — default `IAxlisDiagnosticsSink` routing events to `ILogger`
- `AddAxlis()` / `AddAxlisGraphQL()` / `UseAxlis()` — DI registration helpers
- `AxlisOptions` — `CacheTtl`, `EnableDiagnostics`

See the [Axlis repository](https://github.com/marioarce/Axlis) for full documentation.
