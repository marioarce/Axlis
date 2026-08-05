# Axlis.Customizations.Controls.Sitecore102

![Axlis Banner](https://raw.githubusercontent.com/marioarce/Axlis/refs/heads/main/assets/banner.png)

Sitecore Content Editor / Shell control customizations for [Axlis.Customizations](https://github.com/marioarce/Axlis), compiled against **Sitecore 10.2.x** (`Sitecore.Kernel` + `Sitecore.Web`, both `[10.2.0,10.3.0)`).

Targets `net48`. This is a Sitecore-version-gated package by design — a future Sitecore 10.3/10.4/etc. line ships as a sibling package (`Axlis.Customizations.Controls.SitecoreXXX`), not a wider version range on this one.

## What's in here

- `QueryableTreeList` — extends the standard Sitecore `TreeList` field to support:
  - Query-based data sources (`query:` prefix), resolved via `LookupSources.GetItems` against the current content database.
  - `IncludeTemplatesForDisplay` / `IncludeTemplatesForSelection` parameters, layered on top of the resolved `DataSource`.
  - A `[Not in selection list]` header warning on items that fall outside the resolved data-source scope.

## Install

This package depends on `Sitecore.Kernel` and `Sitecore.Web`, which are not on nuget.org. Add Sitecore's public feed to your own `nuget.config` (or global NuGet sources) before restoring:

```xml
<packageSources>
  <add key="sitecore" value="https://nuget.sitecore.com/resources/v3/index.json" />
</packageSources>
```

```
dotnet add package Axlis.Customizations.Controls.Sitecore102
```

## Manual setup (required — nothing below is automated by installing the package)

**1. Config include.** A sample control-source registration ships as `content/App_Config/zzz.Axlis.Customizations.QueryableTreeList.config.template` inside the package (inert — the `.template` suffix keeps Sitecore's config watcher from loading it). Copy it into your `App_Config/Include/` folder and drop the `.template` suffix:

```xml
<configuration xmlns:patch="http://www.sitecore.net/xmlconfig/">
  <sitecore>
    <controlSources>
      <source
        mode="on"
        namespace="Axlis.Customizations.Controls"
        assembly="Axlis.Customizations.Controls.Sitecore102"
        prefix="Axlis" />
    </controlSources>
  </sitecore>
</configuration>
```

**2. Field Type item.** This package intentionally does **not** ship a serialized Sitecore item (no assumed TDS/Unicorn/Sitecore CLI setup — Axlis is consumed across projects with different serialization tooling). Create it yourself under whichever `/sitecore/system/Field types` location fits your own tree, with:

| Field | Value |
|---|---|
| `Assembly` | `Axlis.Customizations.Controls.Sitecore102` |
| `Class` | `Axlis.Customizations.Controls.QueryableTreeList` |
| `Control` | `Axlis:QueryableTreeList` |

**3. Verify** (manual — there is no automated test coverage for this package; see "Testing" below):
- Install into a real Sitecore 10.2.x instance with the config include and Field Type item above in place.
- Confirm the control renders in the Content Editor on a field using this Field Type.
- Confirm a `query:`-prefixed source resolves correctly against a real query.
- Confirm the `[Not in selection list]` header warning appears for an item outside the resolved data source.

## Testing

There is no Sitecore FakeDb (or equivalent) in this project's toolchain, and this package's logic is inherently coupled to `Sitecore.Context`, `Item`, and Sitecore's `ServiceLocator` — none of which run outside a real or faked Sitecore instance. Automated coverage is intentionally scoped to [`Axlis.Customizations.Abstractions`](../Axlis.Customizations.Abstractions/README.md) instead; this package is verified manually per the checklist above before each release.

See the [Axlis repository](https://github.com/marioarce/Axlis) for full documentation.
