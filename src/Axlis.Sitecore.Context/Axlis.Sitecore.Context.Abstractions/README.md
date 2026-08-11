# Axlis.Sitecore.Context.Abstractions

![Axlis Banner](https://raw.githubusercontent.com/marioarce/Axlis/refs/heads/main/assets/banner.png)

Sitecore-free, per-logical-call ambient context storage for the [Axlis.Sitecore.Context](https://github.com/marioarce/Axlis) family.

Targets `net48`. Unlike most `*.Abstractions` packages in the Axlis ecosystem, this one is **not** multi-targeted (`netstandard2.0`/`net8.0`) — the propagation mechanism it provides is built on `System.Runtime.Remoting.Messaging` (`CallContext`, `ILogicalThreadAffinative`), which is a .NET Framework-only API surface with no equivalent shipped on `netstandard2.0` or `net8.0`. This package's only job is that mechanism, so there is no framework-agnostic subset left to split out.

## What's in here

- `AmbientContextStore<T>` — a generic, per-logical-call value store. This is the actual thread-safety mechanism behind `Axlis.Sitecore.Context.Database` / `.Request` / `.HttpContext` (see [`Axlis.Sitecore.Context.Sitecore102`](../Axlis.Sitecore.Context.Sitecore102/README.md)), but it has no dependency on Sitecore or `System.Web` — it stores and retrieves any reference type, isolated per logical call (i.e. per request, correctly flowing across `await` continuations), with no locking.

## Why no lock

A prior reference implementation this package's mechanism is ported from guarded its accessor with a global `lock`. That lock protects nothing here: the data `AmbientContextStore<T>` holds is already isolated per logical call context by construction, so two unrelated requests never contend for the same slot. A global lock would only serialize otherwise-independent work — and reintroducing unnecessary locking is exactly the kind of thing that causes the deadlocks this whole package family exists to avoid.

## Install

```
dotnet add package Axlis.Sitecore.Context.Abstractions
```

See the [Axlis repository](https://github.com/marioarce/Axlis) for full documentation, and the dedicated [architecture doc](../../../docs/sitecore-context/Architecture.md) for a full explanation of why the `MarshalByRefObject` + `ILogicalThreadAffinative` + `CallContext` combination is actually thread-safe.
