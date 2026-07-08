using Axlis.Caching;
using Axlis.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PowerCSharp.Feature.Cache.Abstractions.NoOp;

namespace Axlis.Tests;

public sealed class SitecoreItemCacheManagerTests
{
    private static Item BuildItem(string id = "{A}", string path = "/a")
        => new(id, path, "Item", "Item", 1, false, null, null, null, null);

    private static SitecoreItemCacheManager BuildManager()
        => new(new NoOpCacheService(NullLogger<NoOpCacheService>.Instance), Options.Create(new AxlisOptions()));

    // ── Factory invocation ────────────────────────────────────────────────────

    [Fact]
    public async Task GetOrCreateAsync_CacheMiss_InvokesFactory()
    {
        var manager = BuildManager();
        var called = false;

        await manager.GetOrCreateAsync("/a", () =>
        {
            called = true;
            return Task.FromResult<IItem?>(BuildItem());
        });

        Assert.True(called);
    }

    [Fact]
    public async Task GetOrCreateAsync_NullResult_DoesNotThrow()
    {
        var manager = BuildManager();

        var result = await manager.GetOrCreateAsync("/missing",
            () => Task.FromResult<IItem?>(null));

        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrCreateAsync_ItemFound_ReturnsItem()
    {
        var manager = BuildManager();
        var item = BuildItem("{X}", "/x");

        var result = await manager.GetOrCreateAsync("/x",
            () => Task.FromResult<IItem?>(item));

        Assert.NotNull(result);
        Assert.Equal("{X}", result!.Id);
    }

    // ── NoOpCache always misses, so factory is always called ─────────────────

    [Fact]
    public async Task GetOrCreateAsync_WithNoOpCache_AlwaysCallsFactory()
    {
        var manager = BuildManager();
        var callCount = 0;

        await manager.GetOrCreateAsync("/x", () =>
        {
            callCount++;
            return Task.FromResult<IItem?>(BuildItem());
        });

        await manager.GetOrCreateAsync("/x", () =>
        {
            callCount++;
            return Task.FromResult<IItem?>(BuildItem());
        });

        // NoOp cache never stores, so factory is called each time
        Assert.Equal(2, callCount);
    }

    // ── Invalidation ──────────────────────────────────────────────────────────

    [Fact]
    public async Task InvalidateAsync_DoesNotThrow()
    {
        var manager = BuildManager();
        var ex = await Record.ExceptionAsync(() => manager.InvalidateAsync("/some-key"));
        Assert.Null(ex);
    }
}
