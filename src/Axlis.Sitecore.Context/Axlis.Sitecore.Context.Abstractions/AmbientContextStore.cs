using System;
using System.Runtime.Remoting.Messaging;

namespace Axlis.Sitecore.Abstractions;

/// <summary>
/// Generic, Sitecore-free, per-logical-call ambient storage for a single value of type
/// <typeparamref name="T"/>.
/// </summary>
/// <remarks>
/// <para>
/// This is the propagation primitive behind the whole Axlis.Sitecore.Context family. It solves
/// the same problem that makes <c>Sitecore.Context.Database</c> and <c>HttpContext.Current</c>
/// unreliable in multi-threaded scenarios: a plain static field is tied to a single physical
/// thread's mutable state (or worse, shared across all threads), and ASP.NET's classic pipeline
/// does not guarantee your code keeps running on the same physical thread for the lifetime of a
/// request — it can legitimately hop threads across <c>await</c> continuations.
/// </para>
/// <para>
/// The fix is <see cref="CallContext"/>'s logical call context, which flows with the *logical*
/// thread of execution (i.e. the call, including everything it awaits) rather than a physical
/// OS thread. <see cref="CallContext.SetData"/> and <see cref="CallContext.GetData"/> normally
/// store data in the *illogical* call context, which does not flow across thread hops — but the
/// CLR automatically promotes any value stored this way into the logical call context instead,
/// if that value's type implements <see cref="ILogicalThreadAffinative"/>. That marker interface
/// is what makes <see cref="AmbientContextBox"/> flow correctly; extending
/// <see cref="MarshalByRefObject"/> here mirrors the original reference pattern this package
/// ports (a per-logical-call accessor object), though no actual cross-AppDomain marshaling
/// happens in this in-process usage.
/// </para>
/// <para>
/// There is deliberately no locking anywhere in this type. The data it holds is isolated per
/// logical call context by construction — two unrelated requests never share a slot — so a lock
/// here would only serialize unrelated work without protecting anything. See the architecture
/// doc for the full reasoning.
/// </para>
/// </remarks>
/// <typeparam name="T">The reference type to store. Must be a reference type because
/// <see langword="null"/> is used to represent "nothing captured yet" — deliberately, since the
/// consumer-facing contract for this whole family is "fall back gracefully to null", not throw.
/// </typeparam>
public sealed class AmbientContextStore<T> : MarshalByRefObject where T : class
{
    private readonly string _slotName;

    /// <summary>
    /// Creates a new store bound to the given logical call context slot name.
    /// </summary>
    /// <param name="slotName">
    /// A name unique to this store within the process. Two <see cref="AmbientContextStore{T}"/>
    /// instances sharing the same <paramref name="slotName"/> would read/write the same slot —
    /// callers should use a fully-qualified, package-specific name (e.g.
    /// <c>"Axlis.Sitecore.Context"</c>) to avoid collisions with other logical-call-context users
    /// in the same process.
    /// </param>
    public AmbientContextStore(string slotName)
    {
        if (string.IsNullOrWhiteSpace(slotName))
        {
            throw new ArgumentException("Slot name must not be null or empty.", nameof(slotName));
        }

        _slotName = slotName;
    }

    /// <summary>
    /// Gets the value captured for the current logical call, or <see langword="null"/> if
    /// nothing has been captured (e.g. no request context was ever initialized on this logical
    /// call, or <see cref="Clear"/> was called).
    /// </summary>
    public T? Current
    {
        get
        {
            var box = CallContext.GetData(_slotName) as AmbientContextBox;
            return box?.Value;
        }
    }

    /// <summary>
    /// Captures <paramref name="value"/> for the current logical call. Subsequent reads of
    /// <see cref="Current"/> — including from code that runs later on a different physical
    /// thread as part of the same logical call (e.g. after an <c>await</c>) — will see it.
    /// </summary>
    public void Set(T value)
    {
        CallContext.SetData(_slotName, new AmbientContextBox(value));
    }

    /// <summary>
    /// Clears the captured value for the current logical call. Callers should invoke this at the
    /// end of a request (e.g. from <c>HttpApplication.EndRequest</c>) so that no state can leak
    /// into whatever the underlying physical thread picks up next once it is returned to the
    /// pool — this is cheap insurance, not a requirement of how <see cref="CallContext"/> itself
    /// behaves.
    /// </summary>
    public void Clear()
    {
        CallContext.FreeNamedDataSlot(_slotName);
    }

    /// <summary>
    /// The serializable, <see cref="ILogicalThreadAffinative"/> wrapper that makes a value stored
    /// via <see cref="CallContext.SetData"/> flow with the logical thread instead of staying
    /// pinned to a single physical thread. See the type-level remarks on
    /// <see cref="AmbientContextStore{T}"/> for why this specific combination of marker
    /// interface and attribute is what makes the propagation work.
    /// </summary>
    [Serializable]
    private sealed class AmbientContextBox : ILogicalThreadAffinative
    {
        public AmbientContextBox(T value)
        {
            Value = value;
        }

        public T Value { get; }
    }
}
