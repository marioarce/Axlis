# CLAUDE.md — Axlis.ORM.GraphQL

Scoped guidance for this project. Inherits everything in the repo-root [`CLAUDE.md`](../../CLAUDE.md).

## Role of This Project

`Axlis.ORM.GraphQL` is the default `IGraphQLTransport` provider and `ISitecoreService` implementation: `HttpGraphQLTransport` (raw `HttpClient` + `System.Text.Json`, no third-party GraphQL client), `HttpGraphQLTransportFactory` (named-`HttpClient` factory with per-site endpoint support), `GraphQLQueryBuilder` (batched alias query construction), `SitecoreService`, and `GraphQLClientException`.

This project owns the network boundary of the whole ecosystem — the only place an actual HTTP call to a Sitecore GraphQL Edge endpoint happens.

## Hard Constraints

- **No third-party GraphQL library.** This is a stated design decision (see `docs/orm/Architecture.md`) — do not add `GraphQL.Client`, `StrawberryShake`, or similar. Queries are hand-built strings (`Queries/*.cs`), requests/responses are plain `System.Text.Json`-serializable POCOs (`Models/*.cs`).
- **Do not swallow exceptions here.** `HttpGraphQLTransport.ExecuteAsync` deliberately catches specific exception types (`HttpRequestException`, `JsonException`, `TaskCanceledException`, and a final catch-all) only to **wrap and rethrow** as `GraphQLClientException` — never to suppress. `SitecoreService` does not catch at all; it lets `GraphQLClientException` bubble to `SitecoreFacade`. Preserve this: this layer's job is to translate low-level failures into one consistent exception type, not to decide what happens on failure.
- **`SitecoreService` performs zero caching.** Caching is `Axlis.ORM`'s concern (`SitecoreItemCacheManager`). Do not add any `ICacheService` reference or in-memory memoization here — it would create a second, uncoordinated cache layer.
- **Batch queries use GraphQL aliases (`item0`, `item1`, ...), not query variables per item.** `GraphQLQueryBuilder.BuildBatchQuery` and `SitecoreService.ExecuteBatchAsync` are tightly coupled: the alias-to-path mapping returned by the builder must exactly match how the response dictionary is parsed. If you change alias naming in the builder, the parsing side must change in the same commit.
- **`JsonSerializerOptions` are duplicated between `HttpGraphQLTransport` and `SitecoreService`** (both define an equivalent `_jsonOptions` with `MaxDepth = 256`, `ReferenceHandler.IgnoreCycles`, etc.). If you change one, change the other, or better: flag the duplication for consolidation into a shared internal helper as a follow-up (don't silently leave them to drift).

## HTTP/Resilience Notes

- `HttpClient` instances are created via `IHttpClientFactory` (registered in `Axlis.ORM`'s DI extensions, not here) — this project only *consumes* the named client (`HttpGraphQLTransportFactory.DefaultClientName`), it does not configure retry/Polly policies itself. If resilience policies are needed, they belong in the DI registration in `Axlis.ORM`, not hardcoded in this project.
- Timeouts come from `AxlisGraphQLOptions.TimeoutSeconds`, applied to the named `HttpClient` at registration time — don't hardcode a timeout inside `HttpGraphQLTransport`.

## See Also

- [`WORKFLOWS.md`](WORKFLOWS.md) — SOPs specific to this project.
- Root [`CLAUDE.md`](../../CLAUDE.md) §3 for the exception-bubbling contract this project must honor.
