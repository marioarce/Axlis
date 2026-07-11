# Axlis.Core

![Axlis Banner](https://raw.githubusercontent.com/marioarce/Axlis/refs/heads/main/assets/banner.png)

Domain model and ORM core for the [Axlis](https://github.com/marioarce/Axlis) Sitecore Headless GraphQL ORM. Targets `net8.0`.

## Install

```
dotnet add package Axlis.Core
```

## What's included

**Domain model**
- `Item` — de-branded Sitecore item with `Id`, `Name`, `Path`, `Version`, `Language`, `Fields`, `Template`, `Parent`, `Children`
- `ItemTemplate` / `ItemTemplateField` — template and field metadata

**Field types**
- `TextField` — single-line, multi-line, rich text (`RawValue`)
- `ImageField` — `Src`, `Alt`, `Width`, `Height`
- `MultilistField` — `Items` (`IEnumerable<IItem>`), `ItemIds`
- `ItemReferenceField` — `Item` (`IItem?`), `ItemId`
- `HyperlinkField` — `Url`, `Text`, `Target`, `LinkType`
- `BooleanField` — `IsChecked`
- `FileField` — `Src`, `Name`

**ORM base classes**
- `ExtendedItem` — base class for all strongly-typed template POCOs; exposes `Axes`, `GetCacheKeyValue()`, and `GetField<TField>()`. Initialized with `IItemLazyLoader` via `ExtendedItem.Initialize()`.
- `BaseItem` — lightweight base wrapping a raw `Item`; for non-Axes use cases.
- `AxesAdapter` — Synthesis-style `Parent`, `Children`, `Siblings`, `GetChildren(predicate)`, `GetDescendants(predicate)`

**Conversion**
- `ItemConverter` — maps `GraphQLItemData` JSON responses to `Item` graphs, including nested parents and children

**Built-in templates**
- `DictionaryEntry`, `DictionaryFolder` — `/sitecore/templates/System/Dictionary/*`
- `Folder` — `/sitecore/templates/Common/Folder`

## Defining a template

```csharp
[SitecoreTemplate("{6D1CD897-1936-4A3A-A511-289A94C2A7B1}")]
public class ArticlePage : ExtendedItem
{
    [SitecoreField("Title")]
    public TextField Title => GetField<TextField>("Title");

    [SitecoreField("Hero Image")]
    public ImageField HeroImage => GetField<ImageField>("Hero Image");
}
```

See the [Templates Guide](https://github.com/marioarce/Axlis/blob/main/docs/Templates.md) and the [Axlis repository](https://github.com/marioarce/Axlis) for full documentation.
