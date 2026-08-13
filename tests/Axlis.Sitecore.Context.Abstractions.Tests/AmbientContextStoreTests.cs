using System.Linq;
using System.Threading.Tasks;
using Axlis.Sitecore.Abstractions;

namespace Axlis.Sitecore.Context.Abstractions.Tests;

public class AmbientContextStoreTests
{
    private sealed class Payload
    {
        public Payload(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }

    [Fact]
    public void Current_WithNothingSet_ReturnsNull()
    {
        var store = new AmbientContextStore<Payload>(nameof(Current_WithNothingSet_ReturnsNull));

        Assert.Null(store.Current);
    }

    [Fact]
    public void Set_ThenCurrent_ReturnsTheSameValue()
    {
        var store = new AmbientContextStore<Payload>(nameof(Set_ThenCurrent_ReturnsTheSameValue));
        var payload = new Payload("hello");

        store.Set(payload);

        Assert.Same(payload, store.Current);
    }

    [Fact]
    public void Clear_RemovesTheValue()
    {
        var store = new AmbientContextStore<Payload>(nameof(Clear_RemovesTheValue));
        store.Set(new Payload("hello"));

        store.Clear();

        Assert.Null(store.Current);
    }

    [Fact]
    public async Task Set_SurvivesAcrossAnAwaitContinuation()
    {
        // Regression guard for the exact failure mode this package exists to prevent: a plain
        // [ThreadStatic] or static field would lose this value the moment the continuation below
        // resumes on a different pooled thread. AmbientContextStore<T> must not.
        var store = new AmbientContextStore<Payload>(nameof(Set_SurvivesAcrossAnAwaitContinuation));
        var payload = new Payload("survives-the-hop");
        store.Set(payload);

        await Task.Delay(10).ConfigureAwait(false);
        await Task.Yield();

        Assert.Same(payload, store.Current);
    }

    [Fact]
    public async Task ConcurrentLogicalCalls_DoNotLeakIntoEachOther()
    {
        // The core thread-safety claim: many logically-independent calls sharing the same slot
        // name never see each other's captured value, because each async call gets its own
        // execution-context branch from the point it is invoked — the same guarantee that makes
        // AsyncLocal<T> (and, in .NET Framework, the promoted logical CallContext) safe to use
        // across concurrent requests in the first place.
        var store = new AmbientContextStore<Payload>(nameof(ConcurrentLogicalCalls_DoNotLeakIntoEachOther));

        async Task<string> RunOneAsync(string expected)
        {
            store.Set(new Payload(expected));
            await Task.Delay(5).ConfigureAwait(false);
            return store.Current?.Value;
        }

        var tasks = Enumerable.Range(0, 20)
            .Select(i => RunOneAsync("call-" + i))
            .ToArray();

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);

        for (var i = 0; i < results.Length; i++)
        {
            Assert.Equal("call-" + i, results[i]);
        }
    }
}
