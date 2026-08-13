# Axlis.Sitecore.Context.Sitecore102

![Axlis Banner](https://raw.githubusercontent.com/marioarce/Axlis/refs/heads/main/assets/banner.png)

A thread-safe, per-request replacement for `Sitecore.Context.Database`, `Sitecore.Context.Request`, and `HttpContext.Current`, for [Axlis.Sitecore.Context](https://github.com/marioarce/Axlis), compiled against **Sitecore 10.2.x** (`Sitecore.Kernel` + `Sitecore.Web`, both `[10.2.0,10.3.0)`).

Targets `net48`. This is a Sitecore-version-gated package by design — a future Sitecore 10.3/10.4/etc. line ships as a sibling package (`Axlis.Sitecore.Context.SitecoreXXX`), not a wider version range on this one.

## The problem this solves

`Sitecore.Context.Database` and `HttpContext.Current` are not reliable in multi-threaded scenarios. Once your code hops off the physical thread ASP.NET started a request on — which can legitimately happen across an `await` — these ambient statics can return `null`, producing hard-to-trace null-reference bugs and, in some cases, deadlocks.

## What's in here

- `Axlis.Sitecore.Context` (namespace `Axlis.Sitecore`, class `Context`) — the consumer-facing static surface:
  - `Axlis.Sitecore.Context.Database` — replaces `Sitecore.Context.Database`
  - `Axlis.Sitecore.Context.Request` — replaces `Sitecore.Context.Request` (raw `System.Web.HttpRequest`)
  - `Axlis.Sitecore.Context.HttpContext` — replaces `HttpContext.Current` (raw `System.Web.HttpContext`)
- `Axlis.Sitecore.Hosting.SitecoreContextHttpModule` — the `IHttpModule` that captures these once per request.

All three properties fall back gracefully if no request context was ever captured (e.g. code running on a background thread or at application start-up): they try the real `Sitecore.Context`/`HttpContext.Current` directly, and return `null` only if that is also unavailable. No exceptions are thrown for the "nothing available" case.

**A note on the namespace.** Every other package in the Axlis ecosystem uses a C# namespace equal to its NuGet `PackageId` (minus any Sitecore-version suffix). This package is the one deliberate exception: the whole point of its API is to be a near drop-in replacement for `Sitecore.Context.Database`, so the type is `Axlis.Sitecore.Context` (namespace `Axlis.Sitecore`, class `Context`) rather than `Axlis.Sitecore.Context.Context`. If you need both `Sitecore.Context` and `Axlis.Sitecore.Context` in the same file while migrating, use a `using` alias, e.g. `using AxlisContext = Axlis.Sitecore.Context;`.

## Install

This package depends on `Sitecore.Kernel` and `Sitecore.Web`, which are not on nuget.org. Add Sitecore's public feed to your own `nuget.config` (or global NuGet sources) before restoring:

```xml
<packageSources>
  <add key="sitecore" value="https://nuget.sitecore.com/resources/v3/index.json" />
</packageSources>
```

```
dotnet add package Axlis.Sitecore.Context.Sitecore102
```

## Manual setup (required — nothing below is automated by installing the package)

**1. Register the HTTP module — in your site's real `Web.config`, not an `App_Config` include.** `SitecoreContextHttpModule` implements the standard `System.Web.IHttpModule` interface, so IIS/ASP.NET itself needs to know about it — that registration has to live in the actual `Web.config` that IIS reads (`<system.webServer><modules>`). Sitecore's own `App_Config/Include/*.config` layering system is a *separate* mechanism that only feeds Sitecore's own `<sitecore>` config tree (pipelines, factories, settings); it has no effect on which `IHttpModule`s IIS loads, so a module registration placed there is silently inert. A ready-to-paste snippet ships inside the package as `content/Setup/WebConfigModulesSnippet.xml.template` (inert — the `.template` suffix keeps it from being picked up automatically by anything). Merge its contents into your site's `Web.config`, inside the existing `<system.webServer><modules>` section (create that section if it doesn't already exist):

```xml
<add name="AxlisSitecoreContextHttpModule" type="Axlis.Sitecore.Hosting.SitecoreContextHttpModule, Axlis.Sitecore.Context.Sitecore102" />
```

**2. Confirm module ordering.** This module must run *after* Sitecore's own request-initialization modules — otherwise it may capture `Sitecore.Context.Database` before Sitecore has populated it for the request. `<modules>` entries in `Web.config` run in the order they're listed, so place this `<add>` after Sitecore's own module entries.

**3. Replace call sites.** Swap `Sitecore.Context.Database` → `Axlis.Sitecore.Context.Database`, `Sitecore.Context.Request` → `Axlis.Sitecore.Context.Request`, `HttpContext.Current` → `Axlis.Sitecore.Context.HttpContext`, one call site at a time — there is no compile-time way to flag remaining call sites, so a text search for the old statics after migrating is worth doing.

**4. Verify** (manual — there is no automated test coverage for this package; see "Testing" below):
- Install into a real Sitecore 10.2.x instance with the module registered as above.
- Confirm `Axlis.Sitecore.Context.Database` / `.Request` / `.HttpContext` are non-null mid-request.
- Confirm they return `null` (not throw) outside any request once the `Sitecore.Context`/`HttpContext.Current` fallback is also unavailable.
- Confirm no state bleeds between two rapid, back-to-back requests served by the same pooled thread.

## Testing

There is no Sitecore FakeDb (or equivalent) in this project's toolchain, and this package's logic is inherently coupled to `Sitecore.Context`, `Sitecore.Data.Database`, and `System.Web` — none of which run outside a real or faked Sitecore instance. Automated coverage is intentionally scoped to [`Axlis.Sitecore.Context.Abstractions`](../Axlis.Sitecore.Context.Abstractions/README.md) instead — the generic, Sitecore-free propagation mechanism this package is a thin adapter over. This package itself is verified manually per the checklist above before each release.

See the [Axlis repository](https://github.com/marioarce/Axlis) for full documentation, and the [architecture doc](../../../docs/sitecore-context/Architecture.md) for why the underlying mechanism is thread-safe.
