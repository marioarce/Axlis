# Axlis.Sitecore.Context.Samples

> **Requires Windows Visual Studio** (2022+, with the **ASP.NET and web development** workload).
> This is the one project in the whole `Axlis` repo that will not open, build, or publish from
> Visual Studio for Mac, VS Code/OmniSharp, Rider on non-Windows, or the cross-platform `dotnet`
> CLI — see "Project format" below for why. Opening the solution's folder in Windows Visual Studio
> with the required workload missing will prompt to install it automatically, via the checked-in
> [`.vsconfig`](../.vsconfig).

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

**This is a hard, Windows-only requirement, not just a recommendation.** `Microsoft.WebApplication.targets`
ships exclusively with Windows Visual Studio's ASP.NET and web development workload — it does not
exist on any other platform or IDE. Confirmed in practice: this solution fails to load correctly in
Visual Studio for Mac, because Visual Studio for Mac's project system has no equivalent import and
never supported classic ASP.NET Web Forms/Web Application Projects at all (it only ever supported
ASP.NET Core/SDK-style web projects, Xamarin, and .NET MAUI). If you're editing files day-to-day on
macOS or Linux, that's fine — the source files themselves are plain text — but building, opening the
solution for real, or publishing needs a Windows machine (or VM) with Visual Studio installed. The
[`.vsconfig`](../.vsconfig) file next to the `.sln` declares the required workload so Windows
Visual Studio can offer to install it automatically if missing.

**Visual Studio 2026 (v18.0) note.** Even with the ASP.NET and web development workload installed,
VS2026's `devenv.exe.config` `VSToolsPath` fallback search paths don't yet account for that
version's own MSBuild install layout, so the default resolution can miss
`Microsoft.WebApplication.targets` even though it's present on disk. The `.csproj` works around this
with an explicit fallback that re-derives `VSToolsPath` from `$(MSBuildExtensionsPath)` (the
currently-running VS installation's own MSBuild folder) whenever the auto-resolved path doesn't
actually contain the targets file — see the comment above the `<VSToolsPath>` property in the
`.csproj` for the full explanation. This has been verified working end-to-end on VS2026.

**Verified.** This sample has been built, published, and run against a real Sitecore 10.2.x CM
instance on Windows with Visual Studio 2026: the demo page correctly showed the raw
`Sitecore.Context.Database`/`HttpContext.Current` statics going `NULL` on all five simulated
background threads while `Axlis.Sitecore.Context.Database`/`.Request` stayed correctly populated —
confirming the underlying propagation mechanism works against a real Sitecore instance, not just in
isolation. The publish output also had to be corrected once during that process: the project excludes
NuGet-restored assemblies (`Sitecore.Kernel`, `Sitecore.Web`, and their transitive dependencies) from
its own build/publish output via the `AxlisExcludeSitecoreProvidedAssembliesFromOutput` MSBuild
target, since a real Sitecore site already ships its own matched set of those assemblies —
publishing this project's independently-restored copies on top caused a `FileLoadException` from
assembly version drift (see the target's comment in the `.csproj` for specifics). Deploying by hand
(copying only the `.aspx`/code-behind files, per the second option below) never had this problem,
since it never copies a `bin/` folder at all.

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
