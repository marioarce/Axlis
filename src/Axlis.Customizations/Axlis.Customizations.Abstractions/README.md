# Axlis.Customizations.Abstractions

![Axlis Banner](https://raw.githubusercontent.com/marioarce/Axlis/refs/heads/main/assets/banner.png)

Shared, Sitecore-free contracts and constants for the [Axlis.Customizations](https://github.com/marioarce/Axlis) family — Sitecore field type and Shell control customizations.

Targets `netstandard2.0` + `net8.0`. Zero third-party dependencies — safe to reference from any project tier, including non-Sitecore tooling.

## What's in here

- `AxlisControlConstants` — the `controlSources` markup prefix (`Axlis`) and the query-source token (`query:`) shared across every customization in the family.

This package is deliberately thin today: it holds only what's genuinely Sitecore-independent. `Axlis.Customizations.Controls.Sitecore102`'s `QueryableTreeList` still depends directly on `Sitecore.Web`/`Sitecore.Kernel` for its own parsing (see that package's README) — nothing was reimplemented here just to keep this package "busy." Expect this package to grow real logic as future customizations (`Axlis.Customizations.{xyz}`) find genuinely shared, Sitecore-free needs.

## Install

```
dotnet add package Axlis.Customizations.Abstractions
```

See the [Axlis repository](https://github.com/marioarce/Axlis) for full documentation.
