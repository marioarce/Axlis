# Axlis.Sitecore.Context — Architecture

This doc explains *why* `Axlis.Sitecore.Context.Database` / `.Request` / `.HttpContext` are actually thread-safe, in more depth than the package READMEs. Read this before touching `AmbientContextStore<T>` or `SitecoreContextHttpModule` — this is the most subtle part of the whole Axlis ecosystem, and getting it wrong silently reintroduces the exact bug class this package exists to eliminate.

## The problem

`Sitecore.Context.Database` and `HttpContext.Current` are ambient statics. In classic ASP.NET, a request is not guaranteed to run start-to-finish on a single physical OS thread — it can hop threads across an `await`, or across certain pipeline transitions. Code that assumes "the thread I'm on now is the thread that started this request" can silently pick up `null` (or, worse, another request's state) once that assumption breaks.

A plain static field or `[ThreadStatic]` field doesn't fix this: a plain static is shared across every concurrent request (actively wrong), and `[ThreadStatic]` is pinned to a single physical thread (loses its value the moment the request hops threads — the same failure mode as the original problem).

## The mechanism

Three pieces, working together:

**1. `System.Runtime.Remoting.Messaging.CallContext`.** In .NET Framework, `CallContext` provides two categories of per-call storage: the *illogical* call context (tied to a single physical thread, does not flow anywhere) and the *logical* call context (flows with the logical thread of execution — the call and everything it `await`s — across physical thread hops). `CallContext.SetData`/`GetData` write to the illogical store by default.

**2. `ILogicalThreadAffinative`.** This marker interface changes that default: when the object passed to `CallContext.SetData` implements `ILogicalThreadAffinative`, the CLR automatically promotes it into the *logical* call context instead of the illogical one — even though the call site used the plain (non-`Logical`-prefixed) `SetData`/`GetData` methods. This is the entire trick: `AmbientContextStore<T>`'s internal `AmbientContextBox` implements this interface purely so that `CallContext.SetData` behaves like the "logical" variant for it.

**3. Execution-context scoping around `async`/`await`.** The logical call context is implemented on top of `ExecutionContext`, which the C#-compiler-generated async state machine explicitly captures and restores around every suspension point. This gives two guarantees that this package's correctness depends on:

- A value set before an `await`, in a call that later resumes on a *different* physical thread, is still visible after it resumes — this is what makes the value survive a thread hop within one request.
- A value set inside a called async method does **not** leak back into the caller once that method suspends and returns control up the stack — this is what keeps two logically-independent requests (or two concurrent calls sharing the same store) from seeing each other's data, even though they may run their synchronous portions back-to-back on the very same physical thread.

`AmbientContextStore<T>.AmbientContextBox` also extends `MarshalByRefObject`, mirroring the reference implementation this mechanism is ported from — in that original, `MarshalByRefObject` mattered for genuine cross-AppDomain marshaling. In this package's fully in-process usage it does no actual marshaling; it's kept for fidelity to the proven pattern and because it costs nothing.

## Why there's no lock

The reference implementation this package ports guards its static accessor with a process-wide `lock`. `Axlis.Sitecore.Context.Abstractions` deliberately does not replicate that lock. The data a lock would protect — the current logical call's stored value — is already isolated per logical call by the mechanism above; no two concurrent, unrelated logical calls ever contend for the same underlying slot. A lock here would add pure serialization overhead across otherwise-independent requests, and a shared global lock is itself a deadlock precursor — precisely the failure category this package exists to remove, not reintroduce.

## Why `EndRequest` explicitly clears the store

`SitecoreContextHttpModule` clears the store on `HttpApplication.EndRequest`, symmetric with capturing it on `BeginRequest`. This is not strictly required by how `CallContext`/`ExecutionContext` behave — in practice, ASP.NET's classic pipeline establishes a fresh logical call context per request, so implicit cleanup is the common case. But that is host/pipeline-dependent behavior, not a documented guarantee this package controls. Explicitly clearing costs one cheap call and removes an entire category of doubt: if it were ever wrong, the resulting bug would look exactly like the original ambient-static problem this package exists to prevent, which would be a particularly bad failure mode to ship silently.

## What is deliberately *not* here

- **No "Future Content" concept.** A prior reference implementation this mechanism was ported from included an environment/feature-toggle-driven override that could force a specific database regardless of the real `Sitecore.Context.Database`. That behavior is specific to that original project and is explicitly out of scope here — `Database` is always a straight, unconditional capture of `Sitecore.Context.Database` at `BeginRequest`, nothing layered on top.
- **No application-specific concerns in the HTTP module.** The reference implementation's module also handled monitoring, feature toggles, and response header manipulation. None of that belongs in a shared package — `SitecoreContextHttpModule`'s only job is capture-on-`BeginRequest` / clear-on-`EndRequest`.

## Testing implications

`AmbientContextStore<T>` is fully testable without Sitecore or `System.Web` — see `Axlis.Sitecore.Context.Abstractions.Tests`, which exercises exactly the guarantees above (survives an `await` hop; concurrent logical calls sharing a slot name don't leak into each other). `Axlis.Sitecore.Context.Sitecore102`, by contrast, has no automated coverage — there is no Sitecore FakeDb in this repo's toolchain, and this package's only remaining job once you trust `AmbientContextStore<T>` is a thin, one-line-per-property wire-up to real Sitecore/`System.Web` types. That thin adapter is verified manually per its README's checklist before each release.
