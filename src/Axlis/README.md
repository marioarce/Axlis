# Axlis

The Sitecore Headless GraphQL ORM for .NET 8 — batteries included. Targets `net8.0`.

## Install

```
dotnet add package Axlis
```

Or install the full family:

```
dotnet add package Axlis                  # facade, cache, DI
dotnet add package Axlis.GraphQL          # default HttpClient transport
dotnet add package Axlis.Core             # domain model + field types + Axes
dotnet add package Axlis.Abstractions     # contracts only (netstandard2.0+net8.0)
```

## Quick start

```csharp
// Program.cs
builder.Services.AddAxlis(builder.Configuration);

// In your service/factory
var item = await _facade.GetItemByPathAsync<MyTemplate>("/sitecore/content/home");
Console.WriteLine(item?.Heading.RawValue);

// Axes traversal
var children = item?.Axes.Children;
var parent   = item?.Axes.Parent;
```

See the [Axlis repository](https://github.com/marioarce/Axlis) for full documentation and the sample app.
