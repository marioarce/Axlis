# WORKFLOWS.md — Axlis.ORM.GraphQL

Project-scoped SOPs. See the repo-root [`WORKFLOWS.md`](../../WORKFLOWS.md) for the 3 repo-wide SOPs — this file adds narrower procedures specific to the transport/service layer.

## SOP G1 — Adding a New Query / Service Operation

Use when `ISitecoreFacade`/`ISitecoreService` needs a new data-access capability (e.g. a new "get items by template" operation).

1. Add the contract method to `ISitecoreService` in `Axlis.ORM.Abstractions` first (see that project's SOP A1/A2 — this is a breaking-change-review candidate since `ISitecoreService` is a public interface).
2. Add the raw GraphQL query text under `Queries/` as a `static class` with a `const string Query`, following `GetItemByPathQuery`/`GetItemFlatQuery`.
3. Add request/response POCOs under `Models/` if the shape isn't already covered by `GraphQLItemData`/existing response types.
4. Implement the method on `SitecoreService`, following the existing pattern exactly: create transport via `_transportFactory.Create()`, build a `GraphQLTransportRequest`, `await transport.ExecuteAsync<TResponse>(...)`, convert via `ItemConverter.ToItem(...)`, log via `LogItemResult`. Do not add a `try/catch` here (see this project's `CLAUDE.md`).
5. Add tests in `tests/Axlis.ORM.GraphQL.Tests/SitecoreServiceTests.cs`, covering: success, "not found" (null item), and a transport-level error (verify it propagates as `GraphQLClientException`, not swallowed).
6. Implement the corresponding facade methods (Clean + Rich) in `Axlis.ORM` — see that project's `WORKFLOWS.md`.

## SOP G2 — Modifying the Batch Query Builder

Use when changing how `GraphQLQueryBuilder.BuildBatchQuery` constructs aliased multi-item queries, or how batch responses are parsed.

1. Treat `GraphQLQueryBuilder` (alias generation) and `SitecoreService.ExecuteBatchAsync` (alias parsing) as **one unit of change** — never modify one without the other in the same commit.
2. Verify batch-size chunking still respects `AxlisGraphQLOptions.BatchSize` (`DefaultBatchSize` fallback when unset or ≤ 0).
3. Add/update tests in `tests/Axlis.ORM.GraphQL.Tests/GraphQLQueryBuilderTests.cs` for the builder, and `SitecoreServiceTests.cs` for end-to-end batch behavior — include a test with a path count that doesn't divide evenly by the batch size, and one where some aliases return `null`/missing in the response dictionary.
4. Confirm ordering is preserved: `GetItemsByPathsAsync` must return a result keyed by the original input paths regardless of batch/alias ordering.

## SOP G3 — Changing Transport Error Handling

Use when adjusting how `HttpGraphQLTransport` classifies or wraps failures.

1. Preserve the invariant: every path out of `ExecuteAsync` either returns `TResponse?` successfully or throws `GraphQLClientException` — never a raw `HttpRequestException`/`JsonException`/etc. escapes this class.
2. If adding a new caught exception type, wrap it in `GraphQLClientException` with a clear message and the inner exception preserved, matching the existing catch blocks' style.
3. Add a test in `tests/Axlis.ORM.GraphQL.Tests/HttpGraphQLTransportTests.cs` for the new failure path, asserting the exception type surfaced to the caller is `GraphQLClientException`.
4. Do not change `GraphQLClientException`'s public shape (`IReadOnlyList<GraphQLError>`) without a breaking-change review — `SitecoreFacade` and any consumer catching this type depend on it.
