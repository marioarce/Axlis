# Axlis.GraphQL

Sitecore Headless GraphQL transport for the [Axlis](https://github.com/marioarce/Axlis) ORM. Targets `net8.0`.

## What's in here

- `SitecoreGraphQLClient` — default `IGraphQLTransport` implementation using raw `HttpClient` + `System.Text.Json` (no third-party GraphQL library)
- `GraphQLQueryBuilder` — builds batched queries with aliases for high-throughput fetches
- `SitecoreService` — implements `ISitecoreService`: `GetItemByPath/Id`, `GetItemsByPaths`, `GetItemFlat`
- Automatic request batching and parallel chunk execution
- `GraphQLClientException` — typed exception for transport errors

## Install

```
dotnet add package Axlis.GraphQL
```

See the [Axlis repository](https://github.com/marioarce/Axlis) for full documentation.
