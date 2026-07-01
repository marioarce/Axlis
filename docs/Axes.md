# Axes Guide

`IAxes` (exposed as `ExtendedItem.Axes`) gives you Synthesis-style tree traversal — the same API Sitecore developers are familiar with — over Headless GraphQL items.

---

## Available members

| Member | Returns | Description |
|---|---|---|
| `Parent` | `IItem?` | The immediate parent item. |
| `Children` | `IEnumerable<IItem>` | Direct children of this item. |
| `Siblings` | `IEnumerable<IItem>` | Items sharing the same parent (excludes self). |
| `GetChildren(predicate?)` | `IEnumerable<IItem>` | Direct children, optionally filtered by a predicate. |
| `GetDescendants(predicate?)` | `IEnumerable<IItem>` | All descendants depth-first, optionally filtered. |

---

## Usage

```csharp
var home = await _facade.GetItemByPathAsync<MyPage>("/sitecore/content/home");

// Parent
var parent = home?.Axes.Parent;

// All children
var children = home?.Axes.Children;

// Filtered children — only ArticlePage items
var articles = home?.Axes.GetChildren(i =>
    string.Equals(i.InnerItem?.Template?.Id, ArticlePage.TemplateId,
        StringComparison.OrdinalIgnoreCase));

// Siblings
var siblings = home?.Axes.Siblings;

// All descendants (caution: can be large)
var allDescendants = home?.Axes.GetDescendants();

// Descendants matching a predicate
var allArticles = home?.Axes.GetDescendants(i =>
    string.Equals(i.InnerItem?.Template?.Id, ArticlePage.TemplateId,
        StringComparison.OrdinalIgnoreCase));
```

---

## How lazy-loading works

`GetItemByPathAsync` fetches a full item graph: 2 levels of ancestry (parent + grandparent) and the first 50 direct children. When `Axes.Parent` or `Axes.Children` are accessed, the data is already present in the item — no extra round-trip.

If an `Axes` member requests data that was **not** fetched (e.g. `Parent.Parent.Parent`, or more than 50 children), `SitecoreItemLazyLoader` transparently fetches the missing item via `ISitecoreService`, routing through `SitecoreItemCacheManager` to avoid redundant network calls.

```
item.Axes.Parent          → data already in item (no fetch)
item.Axes.Parent.Axes.Parent → lazy-fetch Parent.Parent via SitecoreItemLazyLoader
```

Lazy fetches are **synchronous** (the property getter cannot be async). In ASP.NET Core this is safe because there is no ambient synchronization context. In non-ASP environments (e.g. console, WPF), avoid deep tree traversal in hot paths — prefer pre-fetching via `GetItemsByPathsAsync` instead.

---

## Flat fetch (`GetItemFlatAsync`)

When you only need field values and never touch `Axes`, use `GetItemFlatAsync`. It runs a lighter GraphQL query with no parent/children data:

```csharp
// Cheaper — no parent/children in the response
var entry = await _facade.GetItemFlatAsync<DictionaryEntry>(
    "/sitecore/content/dictionary/my-key");

Console.WriteLine(entry?.Phrase.RawValue);
// entry?.Axes.Parent would trigger a lazy-fetch here
```

---

## Batch pre-fetch pattern

For pages that need many items (e.g. a navigation menu), prefer `GetItemsByPathsAsync` over individual fetches + Axes traversal. It issues one batched GraphQL request with aliased sub-queries:

```csharp
var paths = new[]
{
    "/sitecore/content/home",
    "/sitecore/content/about",
    "/sitecore/content/contact"
};

var items = await _facade.GetItemsByPathsAsync<MyPage>(paths);

foreach (var page in items.Where(p => p != null))
    Console.WriteLine(page!.Title.RawValue);
```
