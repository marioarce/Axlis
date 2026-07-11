# Axlis.GraphQL

![Axlis Banner](https://raw.githubusercontent.com/marioarce/Axlis/refs/heads/main/assets/banner.png)

Default Sitecore Headless GraphQL transport for the [Axlis](https://github.com/marioarce/Axlis) ORM. Targets `net8.0`.

Uses raw `HttpClient` + `System.Text.Json` — no third-party GraphQL library required.

## Install

```
dotnet add package Axlis.GraphQL
```

## What's included

- `HttpGraphQLTransport` — `IGraphQLTransport` over `HttpClient` + `System.Text.Json`; handles HTTP errors, GraphQL-level errors, JSON failures, and cancellation
- `HttpGraphQLTransportFactory` — `IGraphQLTransportFactory` using named `HttpClient`s; supports per-site endpoints via `AxlisGraphQLOptions.SiteEndpoints`
- `SitecoreService` — `ISitecoreService` implementation: `GetItemByPath`, `GetItemById`, `GetItemFlat`, `GetItemsByPaths`
- `GraphQLQueryBuilder` — builds batched alias queries (`item0`, `item1`, …) for high-throughput parallel fetches
- `GraphQLClientException` — typed exception exposing `IReadOnlyList<GraphQLError>` from the GraphQL response

## Configuration

```json
{
  "AxlisGraphQL": {
    "Endpoint":        "https://cm.your-instance.com/sitecore/api/graph/edge",
    "ApiKey":          "your-sc_apikey-here",
    "BatchSize":       10,
    "TimeoutSeconds":  30
  }
}
```

## Register via `AddAxlisGraphQL`

```csharp
builder.Services
    .AddAxlis()
    .AddAxlisGraphQL(o =>
    {
        o.Endpoint = builder.Configuration["AxlisGraphQL:Endpoint"]!;
        o.ApiKey   = builder.Configuration["AxlisGraphQL:ApiKey"]!;
    });
```

See the [Axlis repository](https://github.com/marioarce/Axlis) for full documentation.
