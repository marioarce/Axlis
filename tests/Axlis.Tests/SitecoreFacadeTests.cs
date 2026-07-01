using Axlis.Caching;
using Axlis.Core;
using Axlis.Results;
using Axlis.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PowerCSharp.Feature.Cache.Abstractions.NoOp;

namespace Axlis.Tests;

public sealed class SitecoreFacadeTests
{
    // ── Fake ISitecoreService ─────────────────────────────────────────────────

    private sealed class FakeSitecoreService : ISitecoreService
    {
        private readonly IItem? _item;
        private readonly Dictionary<string, IItem?>? _batchItems;

        public FakeSitecoreService(IItem? item) => _item = item;

        public FakeSitecoreService(Dictionary<string, IItem?> batchItems)
        {
            _batchItems = batchItems;
        }

        public Task<IItem?> GetItemByPathAsync(string path, CancellationToken ct = default)
            => Task.FromResult(_item);

        public Task<IItem?> GetItemByIdAsync(string id, CancellationToken ct = default)
            => Task.FromResult(_item);

        public Task<IDictionary<string, IItem?>> GetItemsByPathsAsync(
            IEnumerable<string> paths,
            CancellationToken ct = default)
        {
            IDictionary<string, IItem?> result = _batchItems
                ?? paths.ToDictionary(p => p, _ => _item);
            return Task.FromResult(result);
        }

        public Task<IItem?> GetItemFlatAsync(string path, CancellationToken ct = default)
            => Task.FromResult(_item);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private sealed class SampleItem : BaseItem { }

    private static Item BuildItem(string id = "{A}", string name = "Test", string path = "/test")
        => new(id, path, name, name, 1, false, null, null, null, null);

    private static SitecoreFacade BuildFacade(IItem? item)
    {
        var service = new FakeSitecoreService(item);
        var cache = new SitecoreItemCacheManager(
            new NoOpCacheService(NullLogger<NoOpCacheService>.Instance),
            Options.Create(new AxlisOptions()));
        return new SitecoreFacade(
            service, cache, Options.Create(new AxlisOptions()));
    }

    // ── Clean API ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetItemByPathAsync_ItemFound_ReturnsMappedWrapper()
    {
        var facade = BuildFacade(BuildItem());

        var result = await facade.GetItemByPathAsync<SampleItem>("/test");

        Assert.NotNull(result);
        Assert.Equal("Test", result!.Name);
    }

    [Fact]
    public async Task GetItemByPathAsync_ItemNotFound_ReturnsNull()
    {
        var facade = BuildFacade(null);

        var result = await facade.GetItemByPathAsync<SampleItem>("/missing");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetItemByIdAsync_ItemFound_ReturnsMappedWrapper()
    {
        var facade = BuildFacade(BuildItem(id: "{ABC}"));

        var result = await facade.GetItemByIdAsync<SampleItem>("{ABC}");

        Assert.NotNull(result);
        Assert.Equal("{ABC}", result!.Id);
    }

    [Fact]
    public async Task GetItemFlatAsync_ItemFound_ReturnsMappedWrapper()
    {
        var facade = BuildFacade(BuildItem(name: "Flat"));

        var result = await facade.GetItemFlatAsync<SampleItem>("/flat");

        Assert.NotNull(result);
        Assert.Equal("Flat", result!.Name);
    }

    [Fact]
    public async Task GetItemsByPathsAsync_EmptyPaths_ReturnsEmptyCollection()
    {
        var facade = BuildFacade(null);

        var result = await facade.GetItemsByPathsAsync<SampleItem>(Array.Empty<string>());

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetItemsByPathsAsync_MultiplePaths_ReturnsMappedItems()
    {
        var items = new Dictionary<string, IItem?>
        {
            ["/a"] = BuildItem("{A}", "A", "/a"),
            ["/b"] = BuildItem("{B}", "B", "/b")
        };
        var service = new FakeSitecoreService(items);
        var cache = new SitecoreItemCacheManager(
            new NoOpCacheService(NullLogger<NoOpCacheService>.Instance),
            Options.Create(new AxlisOptions()));
        var facade = new SitecoreFacade(
            service, cache, Options.Create(new AxlisOptions()));

        var results = (await facade.GetItemsByPathsAsync<SampleItem>(new[] { "/a", "/b" })).ToList();

        Assert.Equal(2, results.Count);
        Assert.NotNull(results[0]);
        Assert.NotNull(results[1]);
    }

    // ── Rich (WithResult) API ─────────────────────────────────────────────────

    [Fact]
    public async Task GetItemByPathWithResultAsync_ItemFound_HasValue()
    {
        var facade = BuildFacade(BuildItem());

        var result = await facade.GetItemByPathWithResultAsync<SampleItem>("/test");

        Assert.True(result.HasValue);
        Assert.NotNull(result.Value);
        Assert.NotNull(result.Metadata);
        Assert.Equal("{A}", result.Metadata!.ItemId);
    }

    [Fact]
    public async Task GetItemByPathWithResultAsync_ItemNotFound_EmptyResult()
    {
        var facade = BuildFacade(null);

        var result = await facade.GetItemByPathWithResultAsync<SampleItem>("/missing");

        Assert.False(result.HasValue);
        Assert.Null(result.Value);
        Assert.Null(result.Metadata);
    }

    [Fact]
    public async Task GetItemByPathWithResultAsync_ItemNotFound_DiagnosticsHasWarning()
    {
        var facade = BuildFacade(null);

        var result = await facade.GetItemByPathWithResultAsync<SampleItem>("/missing");

        Assert.NotNull(result.Diagnostics);
        Assert.NotEmpty(result.Diagnostics!.Events);
        Assert.Equal(DiagnosticSeverity.Warning, result.Diagnostics.Events[0].Severity);
    }

    [Fact]
    public async Task GetItemByIdWithResultAsync_ItemFound_HasValueAndMetadata()
    {
        var facade = BuildFacade(BuildItem(id: "{XYZ}", name: "ById", path: "/by-id"));

        var result = await facade.GetItemByIdWithResultAsync<SampleItem>("{XYZ}");

        Assert.True(result.HasValue);
        Assert.Equal("{XYZ}", result.Metadata?.ItemId);
        Assert.Equal("/by-id", result.Metadata?.ItemPath);
        Assert.Equal(1, result.Metadata?.ItemVersion);
    }

    [Fact]
    public async Task GetItemFlatWithResultAsync_ItemFound_HasValue()
    {
        var facade = BuildFacade(BuildItem(name: "FlatResult"));

        var result = await facade.GetItemFlatWithResultAsync<SampleItem>("/flat-result");

        Assert.True(result.HasValue);
        Assert.Equal("FlatResult", result.Value!.Name);
    }
}
