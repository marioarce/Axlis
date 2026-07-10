# Getting Started

## 1. Install

```bash
dotnet add package Axlis
dotnet add package Axlis.GraphQL
```

For contracts-only scenarios (e.g. a shared domain project):

```bash
dotnet add package Axlis.Abstractions
```

---

## 2. Configure

Add your Sitecore GraphQL Edge endpoint and API key to `appsettings.json`:

```json
{
  "Axlis": {
    "CacheTtl": "00:30:00",
    "EnableDiagnostics": true
  },
  "AxlisGraphQL": {
    "Endpoint": "https://cm.your-instance.com/sitecore/api/graph/edge",
    "ApiKey":   "your-sc_apikey-here",
    "BatchSize": 10,
    "TimeoutSeconds": 30
  }
}
```

---

## 3. Register services

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services
    // Optional: real cache provider (BitFaster in-memory, stampede-safe).
    // If omitted, a NoOpCacheService is used (no caching, always fetches).
    // .AddBitFasterCache()

    .AddAxlis(o =>
    {
        o.CacheTtl         = TimeSpan.FromMinutes(30);
        o.EnableDiagnostics = true;
    })
    .AddAxlisGraphQL(o =>
    {
        o.Endpoint = builder.Configuration["AxlisGraphQL:Endpoint"]!;
        o.ApiKey   = builder.Configuration["AxlisGraphQL:ApiKey"]!;
    });

var app = builder.Build();

// Wire the ambient lazy-loader into ExtendedItem so tree-traversal works.
app.Services.UseAxlis();

app.Run();
```

---

## 4. Define a template POCO

Create one class per Sitecore template, deriving from `ExtendedItem`:

```csharp
using Axlis.Attributes;
using Axlis.Core;
using Axlis.Core.FieldTypes;

[SitecoreTemplate("{6D1CD897-1936-4A3A-A511-289A94C2A7B1}")]
public class DictionaryEntry : ExtendedItem
{
    [SitecoreField("Key")]
    public TextField Key => GetField<TextField>("Key");

    [SitecoreField("Phrase")]
    public TextField Phrase => GetField<TextField>("Phrase");
}
```

See [Templates Guide](Templates.md) for all field types and conventions.

---

## 5. Fetch an item

Inject `ISitecoreFacade` and call any of the four fetch methods:

```csharp
public class DictionaryService
{
    private readonly ISitecoreFacade _facade;

    public DictionaryService(ISitecoreFacade facade) => _facade = facade;

    // By Sitecore path
    public Task<DictionaryEntry?> GetByPath(string path, CancellationToken ct = default)
        => _facade.GetItemByPathAsync<DictionaryEntry>(path, ct);

    // By item ID (GUID string)
    public Task<DictionaryEntry?> GetById(string id, CancellationToken ct = default)
        => _facade.GetItemByIdAsync<DictionaryEntry>(id, ct);

    // Multiple paths in one batched request
    public Task<IEnumerable<DictionaryEntry?>> GetMany(IEnumerable<string> paths, CancellationToken ct = default)
        => _facade.GetItemsByPathsAsync<DictionaryEntry>(paths, ct);

    // Field-only fetch (no parent/children, cheaper query)
    public Task<DictionaryEntry?> GetFlat(string path, CancellationToken ct = default)
        => _facade.GetItemFlatAsync<DictionaryEntry>(path, ct);
}
```

---

## 6. Rich result with metadata and diagnostics

Use the `WithResult` variants when you need provenance or want to inspect diagnostics:

```csharp
var result = await _facade.GetItemByPathWithResultAsync<DictionaryEntry>(
    "/sitecore/content/dictionary/my-key", ct);

if (result.HasValue)
{
    Console.WriteLine(result.Value!.Phrase.RawValue);
    Console.WriteLine($"Fetched item ID: {result.Metadata?.ItemId}");
    Console.WriteLine($"Version: {result.Metadata?.ItemVersion}");
}
else
{
    // result.Diagnostics contains warning events
    foreach (var evt in result.Diagnostics?.Events ?? [])
        Console.WriteLine($"[{evt.Severity}] {evt.Message}");
}
```

---

## 7. Axes traversal

```csharp
var home = await _facade.GetItemByPathAsync<MyPage>("/sitecore/content/home");

var parent   = home?.Axes.Parent;
var children = home?.Axes.Children;   // IEnumerable<IItem>
var siblings = home?.Axes.Siblings;

// Filtered traversal
var newsPages = home?.Axes.GetChildren(i =>
    i.InnerItem?.Template?.Id == NewsPage.TemplateId);

var allDescendants = home?.Axes.GetDescendants();
```

See [Axes Guide](Axes.md) for details on lazy-loading behavior.
