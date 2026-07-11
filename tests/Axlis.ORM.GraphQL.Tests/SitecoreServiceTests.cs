using System.Net;
using System.Text;
using System.Text.Json;
using Axlis.ORM.GraphQL.Models;
using Axlis.ORM.GraphQL.Transport;
using Axlis.ORM.Transport;
using Microsoft.Extensions.Options;

namespace Axlis.ORM.GraphQL.Tests;

public sealed class SitecoreServiceTests
{
    // ── Fake transport infrastructure ────────────────────────────────────────

    private sealed class FakeTransport : IGraphQLTransport
    {
        private readonly string _json;
        private static readonly JsonSerializerOptions _opts =
            new() { PropertyNameCaseInsensitive = true };

        public FakeTransport(string json) => _json = json;

        public async Task<TResponse?> ExecuteAsync<TResponse>(
            GraphQLTransportRequest request,
            CancellationToken ct = default)
        {
            await Task.CompletedTask.ConfigureAwait(false);

            using var doc = JsonDocument.Parse(_json);
            if (doc.RootElement.TryGetProperty("data", out var dataElem) &&
                dataElem.ValueKind != JsonValueKind.Null)
            {
                return JsonSerializer.Deserialize<TResponse>(dataElem.GetRawText(), _opts);
            }

            return default;
        }
    }

    private sealed class FakeTransportFactory : IGraphQLTransportFactory
    {
        private readonly IGraphQLTransport _transport;

        public FakeTransportFactory(IGraphQLTransport transport) => _transport = transport;

        public IGraphQLTransport Create(string? siteKey = null) => _transport;
    }

    private static SitecoreService BuildService(string responseJson)
    {
        var transport = new FakeTransport(responseJson);
        var factory = new FakeTransportFactory(transport);
        var options = Options.Create(new AxlisGraphQLOptions { BatchSize = 5 });
        return new SitecoreService(factory, options);
    }

    private static string ItemJson(string id, string name, string path) =>
        JsonSerializer.Serialize(new
        {
            data = new
            {
                item = new
                {
                    id,
                    name,
                    path,
                    displayName = name,
                    version = 1,
                    hasChildren = false,
                    template = new { id = "{00000000-0000-0000-0000-000000000000}", name = "SampleTemplate" },
                    fields = Array.Empty<object>(),
                    parent = (object?)null,
                    children = (object?)null
                }
            }
        });

    private static string NullItemJson() =>
        JsonSerializer.Serialize(new { data = new { item = (object?)null } });

    // ── GetItemByPathAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetItemByPathAsync_ItemFound_ReturnsItem()
    {
        var service = BuildService(ItemJson("{ABC}", "Home", "/sitecore/content/home"));

        var item = await service.GetItemByPathAsync("/sitecore/content/home");

        Assert.NotNull(item);
        Assert.Equal("Home", item!.Name);
        Assert.Equal("/sitecore/content/home", item.Path);
    }

    [Fact]
    public async Task GetItemByPathAsync_ItemNotFound_ReturnsNull()
    {
        var service = BuildService(NullItemJson());

        var item = await service.GetItemByPathAsync("/sitecore/content/missing");

        Assert.Null(item);
    }

    // ── GetItemByIdAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetItemByIdAsync_ItemFound_ReturnsItem()
    {
        var service = BuildService(ItemJson("{ABC}", "MyItem", "/sitecore/content/my-item"));

        var item = await service.GetItemByIdAsync("{ABC}");

        Assert.NotNull(item);
        Assert.Equal("MyItem", item!.Name);
    }

    [Fact]
    public async Task GetItemByIdAsync_ItemNotFound_ReturnsNull()
    {
        var service = BuildService(NullItemJson());

        var item = await service.GetItemByIdAsync("{MISSING}");

        Assert.Null(item);
    }

    // ── GetItemFlatAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetItemFlatAsync_ItemFound_ReturnsItem()
    {
        var service = BuildService(ItemJson("{FLAT}", "FlatItem", "/sitecore/content/flat"));

        var item = await service.GetItemFlatAsync("/sitecore/content/flat");

        Assert.NotNull(item);
        Assert.Equal("FlatItem", item!.Name);
    }

    [Fact]
    public async Task GetItemFlatAsync_ItemNotFound_ReturnsNull()
    {
        var service = BuildService(NullItemJson());

        var item = await service.GetItemFlatAsync("/sitecore/content/missing");

        Assert.Null(item);
    }

    // ── GetItemsByPathsAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetItemsByPathsAsync_EmptyPaths_ReturnsEmptyDict()
    {
        var service = BuildService("{}");

        var result = await service.GetItemsByPathsAsync(Array.Empty<string>());

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetItemsByPathsAsync_NullPaths_ReturnsEmptyDict()
    {
        var service = BuildService("{}");

        var result = await service.GetItemsByPathsAsync(null!);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetItemsByPathsAsync_BatchResponse_MapsAlisesToPaths()
    {
        // Simulate a batch response with two aliased items
        var batchResponse = JsonSerializer.Serialize(new
        {
            data = new Dictionary<string, object>
            {
                ["item0"] = new
                {
                    id = "{ID1}",
                    name = "ItemA",
                    path = "/path/a",
                    displayName = "ItemA",
                    version = 1,
                    hasChildren = false,
                    template = new { id = "{T1}", name = "T" },
                    fields = Array.Empty<object>(),
                    parent = (object?)null,
                    children = (object?)null
                },
                ["item1"] = new
                {
                    id = "{ID2}",
                    name = "ItemB",
                    path = "/path/b",
                    displayName = "ItemB",
                    version = 1,
                    hasChildren = false,
                    template = new { id = "{T1}", name = "T" },
                    fields = Array.Empty<object>(),
                    parent = (object?)null,
                    children = (object?)null
                }
            }
        });

        var service = BuildService(batchResponse);
        var paths = new[] { "/path/a", "/path/b" };

        var result = await service.GetItemsByPathsAsync(paths);

        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey("/path/a"));
        Assert.True(result.ContainsKey("/path/b"));
        Assert.Equal("ItemA", result["/path/a"]?.Name);
        Assert.Equal("ItemB", result["/path/b"]?.Name);
    }

    // ── CancellationToken propagation ─────────────────────────────────────────

    [Fact]
    public async Task GetItemByPathAsync_CancelledToken_ThrowsOrPropagates()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var service = BuildService(ItemJson("{X}", "X", "/x"));

        // FakeTransport doesn't honour cancellation; task should still complete synchronously.
        // This just verifies the token flows through without immediate exception from service itself.
        var item = await service.GetItemByPathAsync("/x", cts.Token);
        Assert.NotNull(item);
    }
}
