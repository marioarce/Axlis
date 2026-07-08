# Templates Guide

Axlis uses hand-written POCO classes — one per Sitecore template — that derive from `ExtendedItem`. They are plain C# classes with no IL weaving, no reflection overhead beyond the initial `Activator.CreateInstance`, and no code generation required (though [SitecoreTemplate] / [SitecoreField] attributes are codegen-ready for a future generator).

---

## Defining a template

```csharp
using Axlis.Attributes;
using Axlis.Core;
using Axlis.Core.FieldTypes;

[SitecoreTemplate("{6D1CD897-1936-4A3A-A511-289A94C2A7B1}")]
public class ArticlePage : ExtendedItem
{
    [SitecoreField("Title")]
    public TextField Title => GetField<TextField>("Title");

    [SitecoreField("Body")]
    public TextField Body => GetField<TextField>("Body");

    [SitecoreField("Hero Image")]
    public ImageField HeroImage => GetField<ImageField>("Hero Image");

    [SitecoreField("Tags")]
    public MultilistField Tags => GetField<MultilistField>("Tags");
}
```

### Rules
- Inherit from `ExtendedItem` (which inherits `BaseItem : IBaseItem`).
- Decorate the class with `[SitecoreTemplate("guid")]` — use the raw Sitecore template GUID.
- Each field property calls `GetField<TField>("Sitecore field name")` and returns the field type.
- Field names are **case-insensitive** at lookup time.
- Properties are `get`-only (computed on each access; the underlying `Item` holds the data).

---

## Field types

| Type | Use for | Key members |
|---|---|---|
| `TextField` | Single-line text, multi-line text, rich text (raw) | `RawValue` |
| `ImageField` | Image fields | `Src`, `Alt`, `Width`, `Height` |
| `MultilistField` | Multilist, Treelist, Checklist | `Items` (`IEnumerable<IItem>`), `ItemIds` |
| `ItemReferenceField` | Droptree, Droplink | `Item` (`IItem?`), `ItemId` |
| `HyperlinkField` | General Link | `Url`, `Text`, `Target`, `LinkType` |
| `BooleanField` | Checkbox | `IsChecked` |
| `FileField` | File | `Src`, `Name` |

All field types inherit `BaseField` and expose `FieldName`, `RawValue`, and `IsEmpty`.

### Accessing field values

```csharp
var article = await _facade.GetItemByPathAsync<ArticlePage>("/content/articles/my-article");

string? title   = article?.Title.RawValue;
string? imgSrc  = article?.HeroImage.Src;
bool    isEmpty = article?.Title.IsEmpty ?? true;

// Multilist gives you typed IItem references
foreach (var tag in article?.Tags.Items ?? [])
    Console.WriteLine(tag.Name);
```

---

## Inheriting templates

Model inheritance is straightforward:

```csharp
[SitecoreTemplate("{...}")]
public class BasePage : ExtendedItem
{
    [SitecoreField("Page Title")]
    public TextField PageTitle => GetField<TextField>("Page Title");
}

[SitecoreTemplate("{...}")]
public class ArticlePage : BasePage
{
    [SitecoreField("Body")]
    public TextField Body => GetField<TextField>("Body");
}
```

---

## Built-in templates

Axlis ships with commonly used Sitecore system templates in `Axlis.Core`:

| Class | Sitecore template |
|---|---|
| `DictionaryEntry` | `/sitecore/templates/System/Dictionary/Dictionary entry` |
| `DictionaryFolder` | `/sitecore/templates/System/Dictionary/Dictionary` |
| `Folder` | `/sitecore/templates/Common/Folder` |

---

## Template IDs

The `[SitecoreTemplate]` attribute accepts either:
- A raw GUID string: `"{6D1CD897-1936-4A3A-A511-289A94C2A7B1}"`
- The template name (used for documentation; not currently used for runtime lookup)

A static `TemplateId` convention is recommended for use with `Axes.GetChildren`:

```csharp
[SitecoreTemplate("{6D1CD897-1936-4A3A-A511-289A94C2A7B1}")]
public class ArticlePage : ExtendedItem
{
    public static readonly string TemplateId = "{6D1CD897-1936-4A3A-A511-289A94C2A7B1}";

    // ... fields
}

// Usage with Axes
var articles = page?.Axes.GetChildren(i =>
    string.Equals(i.InnerItem?.Template?.Id, ArticlePage.TemplateId,
        StringComparison.OrdinalIgnoreCase));
```

---

## Codegen readiness

The `[SitecoreTemplate]` and `[SitecoreField]` attributes are designed so a future Roslyn source generator or CLI tool can:
- Discover all template classes in an assembly via reflection on `[SitecoreTemplate]`.
- Enumerate fields via `[SitecoreField]` without parsing source code.
- Generate typed accessor classes or client-side stubs.

No generator exists in v0.1 — all templates are hand-written. The attribute contract is stable and will not change.
