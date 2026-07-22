# Module Spec — `ItemConverter`

**File:** `src/Axlis.ORM.Core/ItemConverter.cs`
**Consumed by:** `Axlis.ORM.GraphQL.SitecoreService` (every single fetch method routes through this)
**Status:** Fragile zone — referenced from root [`CLAUDE.md`](../../CLAUDE.md) §4 and this project's `CLAUDE.md`.

## Purpose

`ItemConverter` is the sole boundary between raw GraphQL JSON (`GraphQLItemData`) and the typed domain graph (`Item`). It is a `static class` — a pure, side-effect-free function library. No I/O, no caching, no logging.

## Invariants (do not break these silently)

1. **Null-in, null-out.** `ToItem(null)` returns `null`. Every recursive call site checks for null before recursing.
2. **Circular-reference guard via `processedIds`.** A `HashSet<string>` (case-insensitive) of item IDs already converted in the current call tree. If an item's ID is already in the set, conversion returns `null` for that branch instead of recursing infinitely. This means: **if a GraphQL response has a genuine cycle (e.g. an item that lists itself as a descendant), the second occurrence silently becomes `null`**, not an error. Callers must not assume a non-null child/parent means "no cycle was hit here" — it means "this specific branch wasn't the second occurrence."
3. **Depth semantics differ for parent vs. children.** Walking to `Parent` does **not** increment `currentDepth`; walking to `Children` **does** (`currentDepth + 1`). This is deliberate: ancestor chains are typically bounded (root path length) while descendant trees can be arbitrarily wide/deep. `currentDepth` itself is tracked but not currently used to cap recursion — if you add a max-depth cutoff, respect this asymmetry (don't apply the same limit to ancestors and descendants without re-checking this assumption).
4. **`processedIds.Add(itemData.Id)` happens *after* the item is fully constructed**, at the end of the top-level `ToItem` recursive call — meaning a node is only marked "processed" once its own conversion (including its parent's recursive conversion) has completed. Do not move this line earlier without checking whether it changes which branch "wins" a circular reference.
5. **Fields with reference-style data (`TargetItem`, `TargetItems`) recurse one level deeper** (`currentDepth + 1`), and set `isTargetItemLoaded`/`isTargetItemsLoaded` based on whether the recursive conversion actually produced a non-null result — these flags are how `AxesAdapter`/consumers distinguish "not loaded yet" from "loaded and legitimately empty."

## Known Limitations (by design, not bugs)

- `ToAncestorItems` silently drops any ancestor that fails to convert (`.Where(i => i != null)`) rather than surfacing a partial-conversion signal. If ancestor completeness matters for a new feature, this is the place to reconsider, not a place to patch downstream.
- This class does not validate `GraphQLItemData` shape beyond null checks — a malformed but non-null payload (e.g. missing `Id` on a template) is converted as-is. Validation, if ever added, belongs in `SitecoreService` or a new explicit validation step, not silently inside the converter.

## When Modifying

1. Read the full method before touching it — the recursion is compact but the depth/parent/children asymmetry is easy to miss on a skim.
2. Any change to circular-reference behavior, depth handling, or field conversion must come with a new/updated test in `tests/Axlis.ORM.Core.Tests/ItemConverterTests.cs` that exercises the specific scenario (a real cycle, a deep child tree, a field with `TargetItems`).
3. Do not add caching, logging, or async/I/O to this class — see this project's `CLAUDE.md` "Hard Constraints."
4. If you add a new field to `GraphQLFieldData` / `ItemTemplateField`, make sure `ConvertField` maps it — a silently-unmapped field is the most common way this class causes a "why is this value missing" bug report.

## Verification Checklist for Any Change Here

- [ ] Existing `ItemConverterTests.cs` suite still passes.
- [ ] New test covers the specific scenario changed (cycle, depth, new field, ancestor list).
- [ ] Manually traced: does the change affect `AxesAdapter`'s assumptions about `IsFullyLoaded` / `AreChildrenLoaded`? (See [`AxesLazyLoading.md`](AxesLazyLoading.md).)
- [ ] No I/O, logging, or caching was introduced into this file.
