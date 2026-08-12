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
- `Web.Debug.config` / `Web.Release.config` — standard Web Application Project config transforms,
  applied automatically by Visual Studio's Publish workflow based on the selected build
  configuration.
- `Properties/AssemblyInfo.cs` — standard WAP assembly metadata (title/description only; this
  project is never packed or versioned as a NuGet package).
- `Properties/PublishProfiles/FolderProfile.pubxml` — a starting-point profile for
  `Build > Publish > Folder`; see "Deploying this sample" below.

## Project format: a real Visual Studio Web Application Project

This project uses the traditional (non-SDK-style) Visual Studio **Web Application Project** format
— `ProjectTypeGuids` including `{349c5851-65df-11da-9384-00065b846f21}`, an explicit framework
`<Reference>` list, and an import of `Microsoft.WebApplication.targets` — rather than the SDK-style
`Sdk="Microsoft.NET.Sdk"` format used by the rest of this repo's `net48` projects
(`Axlis.Customizations.Controls.Sitecore102`, `Axlis.Sitecore.Context.Sitecore102`). Those other
projects are libraries with no deployable web content of their own, so the simpler SDK-style format
is the right fit for them; this project is a website, and the older WAP format is what unlocks
Visual Studio's native `Build > Publish > Folder` workflow, config transforms
(`Web.Debug.config`/`Web.Release.config`), and an actual `IIS`/`IIS Express` project flavor — all of
which are how you'd realistically get this sample onto a real Sitecore site's IIS instance rather
than copying files by hand. `PackageReference` (for `Sitecore.Kernel`/`Sitecore.Web`) and
`ProjectReference` both work identically in this format, as long as no `packages.config` is present
(it is not).

## Deploying this sample

There is no Sitecore sandbox in this repo's toolchain (same situation as
`Axlis.Customizations.Controls.Sitecore102` and `Axlis.Sitecore.Context.Sitecore102` — see their
READMEs). To actually see this demo run, either:

**Publish via Visual Studio (recommended):**

1. Register `Axlis.Sitecore.Hosting.SitecoreContextHttpModule` in a real Sitecore 10.2.x site's
   real `Web.config` — see `Axlis.Sitecore.Context.Sitecore102`'s README, "Manual setup" step 1
   (this step can't be automated by a publish profile, since it edits a config file this project
   doesn't own).
2. In Visual Studio, right-click this project → **Publish** → **New** → **Folder**, and copy
   [`Properties/PublishProfiles/FolderProfile.pubxml`](Properties/PublishProfiles/FolderProfile.pubxml)
   as a starting point — replace its placeholder `<publishUrl>` with your target site's real path
   (its webroot, or a staging folder you then deploy to IIS by your own process) before publishing.
3. Publish. Request the deployed page directly by its URL (it doesn't go through Sitecore's
   item/layout resolution — it's a plain `.aspx` file — so no Sitecore content items or templates
   need to exist for this demo to work).

**Or copy files by hand:**

1. Register `Axlis.Sitecore.Hosting.SitecoreContextHttpModule` in a real Sitecore 10.2.x site's
   real `Web.config` — see `Axlis.Sitecore.Context.Sitecore102`'s README, "Manual setup" step 1.
2. Copy `Pages/AxlisSitecoreContextDemo.aspx` and its code-behind into that site.
3. Reference `Axlis.Sitecore.Context.Sitecore102` (and `.Abstractions`) from that site's own project.
4. Request the page directly by its deployed URL, same as above.

See [`docs/sitecore-context/Architecture.md`](../../docs/sitecore-context/Architecture.md) for why
the underlying mechanism is thread-safe.
