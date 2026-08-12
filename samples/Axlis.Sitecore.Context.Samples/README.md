# Axlis.Sitecore.Context.Samples

An illustrative Sitecore website project showing how to consume
[`Axlis.Sitecore.Context.Sitecore102`](../../src/Axlis.Sitecore.Context/Axlis.Sitecore.Context.Sitecore102/README.md)
from real Sitecore application code. Targets `net48`, references `Sitecore.Kernel`/`Sitecore.Web`
`[10.2.0,10.3.0)` (same as the package it demonstrates), and takes a project reference on
`Axlis.Sitecore.Context.Sitecore102` directly (not the NuGet package) so it always builds against
whatever is currently in this repo.

This is **not** a shippable NuGet package (`IsPackable=false`) and is **not** part of
`Axlis.Sitecore.Context.sln` or its release CI — it lives in its own
[`Axlis.Sitecore.Context.Samples.sln`](../Axlis.Sitecore.Context.Samples.sln), kept separate so a
mistake in this illustrative project can never block a real package release.

## What's in here

- `Pages/AxlisSitecoreContextDemo.aspx` (+ code-behind) — the actual demo page. On `Page_Load`, it:
  1. Captures `Sitecore.Context.Database` / `HttpContext.Current.Request` (raw) and
     `Axlis.Sitecore.Context.Database` / `.Request` (wrapped) on the original request thread.
  2. Spins up 5 simulated background threads via `Task.Run` and captures the same four values from
     inside each — `Task.Run` is a genuine physical-thread hop, not just an `await` continuation.
  3. Renders both sets of results as HTML tables so you can see, side by side, the raw statics go
     `NULL` on the simulated threads while the Axlis equivalents stay correct — because
     `Task.Run` flows the caller's `ExecutionContext` (and with it, the promoted logical
     `CallContext` data `AmbientContextStore<T>` relies on) into the queued work by default, while
     `Sitecore.Context`/`HttpContext.Current` do not.
- `Global.asax(.cs)` — inherits `Sitecore.Web.Application`, matching how a real Sitecore site's
  Global.asax is wired.
- `Web.config` — a **minimal, illustrative** config, not a real Sitecore site's Web.config. See the
  comment at the top of that file for exactly what's missing and why.

## Why this isn't a full Visual Studio "Web Application Project"

This project uses the same SDK-style `.csproj` format as the rest of this repo
(`Sdk="Microsoft.NET.Sdk"`, matching `Axlis.Customizations.Controls.Sitecore102` and
`Axlis.Sitecore.Context.Sitecore102`) rather than the older Visual Studio Web Application Project
format (`ProjectTypeGuids` + `Microsoft.WebApplication.targets`). That older format exists mainly
for two things this project doesn't need: F5/IIS Express local debugging (there's no local Sitecore
sandbox to debug against — see "Deploying this sample" below) and MSBuild/Web Deploy publish
tooling (irrelevant here, since you'll be copying files into a real Sitecore site by hand, not
publishing this project directly over it). ASP.NET Web Forms compiles `.aspx` markup at runtime
regardless of which project format built the code-behind assembly, so the simpler, lower-risk
format is fully sufficient for this project's actual purpose: illustrative, buildable reference
code.

## Deploying this sample

There is no Sitecore sandbox in this repo's toolchain (same situation as
`Axlis.Customizations.Controls.Sitecore102` and `Axlis.Sitecore.Context.Sitecore102` — see their
READMEs). To actually see this demo run:

1. Register `Axlis.Sitecore.Hosting.SitecoreContextHttpModule` in a real Sitecore 10.2.x site's
   real `Web.config` — see `Axlis.Sitecore.Context.Sitecore102`'s README, "Manual setup" step 1.
2. Copy `Pages/AxlisSitecoreContextDemo.aspx` and its code-behind into that site.
3. Reference `Axlis.Sitecore.Context.Sitecore102` (and `.Abstractions`) from that site's own project.
4. Request the page directly by its deployed URL (it doesn't go through Sitecore's item/layout
   resolution — it's a plain `.aspx` file — so no Sitecore content items or templates need to
   exist for this demo to work).

See [`docs/sitecore-context/Architecture.md`](../../docs/sitecore-context/Architecture.md) for why
the underlying mechanism is thread-safe.
