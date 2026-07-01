# Axlis.Core

Domain model and ORM core for the [Axlis](https://github.com/marioarce/Axlis) Sitecore Headless GraphQL ORM. Targets `net8.0`.

## What's in here

- `Item`, `ItemTemplate`, `ItemTemplateField` — de-branded Sitecore domain model
- **Field types:** `TextField`, `ImageField`, `MultilistField`, `ItemReferenceField`, `HyperlinkField`, `BooleanField`, `FileField`
- `ExtendedItem` — base class for all strongly-typed Sitecore template POCOs
- `AxesAdapter` — Synthesis-style `Parent`, `Children`, `Siblings`, `GetChildren(predicate)`, `GetDescendants(predicate)`
- `ItemConverter` — GraphQL JSON → `Item` mapping
- **Built-in templates:** `DictionaryEntry`, `DictionaryFolder`, `MediaFolder`, `Folder`

## Install

```
dotnet add package Axlis.Core
```

See the [Axlis repository](https://github.com/marioarce/Axlis) for full documentation.
