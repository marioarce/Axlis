using System.Net;
using System.Text;
using System.Text.Json;
using Axlis.GraphQL.Models;
using Axlis.GraphQL.Transport;
using Axlis.Transport;

namespace Axlis.GraphQL.Tests;

public sealed class HttpGraphQLTransportTests
{
    // ── Private DTOs used as generic type arguments (class types avoid Nullable<T> ambiguity) ──

    private sealed class ItemWrapper
    {
        public FakeItem? Item { get; set; }
    }

    private sealed class FakeItem
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private sealed class FakeHttpHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public FakeHttpHandler(string json, HttpStatusCode status = HttpStatusCode.OK)
        {
            _response = new HttpResponseMessage(status)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => Task.FromResult(_response);
    }

    private static HttpGraphQLTransport BuildTransport(
        string responseJson,
        HttpStatusCode status = HttpStatusCode.OK)
    {
        var handler = new FakeHttpHandler(responseJson, status);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://test.local/") };
        return new HttpGraphQLTransport(httpClient);
    }

    private static GraphQLTransportRequest SimpleRequest() => new()
    {
        Query = "query { item(path: \"/home\") { id } }",
        Variables = new { path = "/home" }
    };

    // ── Success path ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_ValidResponse_ReturnsDeserializedData()
    {
        var json = JsonSerializer.Serialize(
            new { data = new { item = new { id = "abc123", name = "Home" } } });
        var transport = BuildTransport(json);

        var result = await transport.ExecuteAsync<ItemWrapper>(SimpleRequest());

        Assert.NotNull(result);
        Assert.Equal("abc123", result!.Item?.Id);
        Assert.Equal("Home", result.Item?.Name);
    }

    [Fact]
    public async Task ExecuteAsync_NullData_ReturnsNull()
    {
        var json = JsonSerializer.Serialize(new { data = (object?)null });
        var transport = BuildTransport(json);

        var result = await transport.ExecuteAsync<ItemWrapper>(SimpleRequest());

        Assert.Null(result);
    }

    // ── Error paths ───────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_HttpError_ThrowsGraphQLClientException()
    {
        var transport = BuildTransport("Internal Server Error", HttpStatusCode.InternalServerError);

        var ex = await Assert.ThrowsAsync<GraphQLClientException>(
            () => transport.ExecuteAsync<ItemWrapper>(SimpleRequest()));

        Assert.Contains("500", ex.Message);
    }

    [Fact]
    public async Task ExecuteAsync_GraphQLErrors_ThrowsGraphQLClientException()
    {
        var json = JsonSerializer.Serialize(new
        {
            data = (object?)null,
            errors = new[] { new { message = "Item not found" } }
        });
        var transport = BuildTransport(json);

        var ex = await Assert.ThrowsAsync<GraphQLClientException>(
            () => transport.ExecuteAsync<ItemWrapper>(SimpleRequest()));

        Assert.Contains("Item not found", ex.Message);
        Assert.Single(ex.Errors);
        Assert.Equal("Item not found", ex.Errors[0].Message);
    }

    [Fact]
    public async Task ExecuteAsync_MalformedJson_ThrowsGraphQLClientException()
    {
        var transport = BuildTransport("{ not valid json at all >>>>");

        var ex = await Assert.ThrowsAsync<GraphQLClientException>(
            () => transport.ExecuteAsync<ItemWrapper>(SimpleRequest()));

        Assert.NotNull(ex.InnerException as JsonException);
    }

    [Fact]
    public async Task ExecuteAsync_CancelledToken_ThrowsGraphQLClientException()
    {
        var httpClient = new HttpClient(new AlwaysCancelHandler())
        {
            BaseAddress = new Uri("https://test.local/")
        };
        var transport = new HttpGraphQLTransport(httpClient);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var ex = await Assert.ThrowsAsync<GraphQLClientException>(
            () => transport.ExecuteAsync<ItemWrapper>(SimpleRequest(), cts.Token));

        Assert.NotNull(ex);
    }

    // ── GraphQLClientException contract ──────────────────────────────────────

    [Fact]
    public void GraphQLClientException_WithErrors_ExposesErrorList()
    {
        var errors = new[] { new GraphQLError { Message = "err1" }, new GraphQLError { Message = "err2" } };
        var ex = new GraphQLClientException("two errors", errors);

        Assert.Equal(2, ex.Errors.Count);
        Assert.Equal("err1", ex.Errors[0].Message);
    }

    [Fact]
    public void GraphQLClientException_WithInnerException_WrapsCorrectly()
    {
        var inner = new IOException("network");
        var ex = new GraphQLClientException("wrapped", inner);

        Assert.Same(inner, ex.InnerException);
        Assert.Empty(ex.Errors);
    }

    // ── Helper handler for cancellation test ─────────────────────────────────

    private sealed class AlwaysCancelHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => Task.FromCanceled<HttpResponseMessage>(new CancellationToken(canceled: true));
    }
}
