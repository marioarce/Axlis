namespace Axlis;

/// <summary>
/// Represents the raw data model for a Sitecore item as returned from the GraphQL API.
/// This is the low-level domain type; consumers typically work with <see cref="IBaseItem"/>
/// or a strongly-typed <c>ExtendedItem</c> subclass instead.
/// </summary>
public interface IItem
{
    /// <summary>Gets the unique identifier of the item.</summary>
    string Id { get; }

    /// <summary>Gets the item name.</summary>
    string Name { get; }

    /// <summary>Gets the display name of the item, if available.</summary>
    string? DisplayName { get; }

    /// <summary>Gets the full Sitecore path of the item, if available.</summary>
    string? Path { get; }

    /// <summary>Gets the URL of the item, if available.</summary>
    string? Url { get; }

    /// <summary>Gets the version number of the item.</summary>
    int Version { get; }

    /// <summary>Gets whether the item has child items.</summary>
    bool HasChildren { get; }

    /// <summary>Gets the template metadata for the item, or <c>null</c> if not loaded.</summary>
    IItemTemplate? Template { get; }

    /// <summary>Gets the flat field collection for the item, or <c>null</c> if not loaded.</summary>
    IReadOnlyCollection<IItemTemplateField>? Fields { get; }

    /// <summary>Gets the parent item, or <c>null</c> if this is the root or parent was not fetched.</summary>
    IItem? Parent { get; }

    /// <summary>Gets the direct children of this item, or <c>null</c> if children were not fetched.</summary>
    IReadOnlyList<IItem>? Children { get; }

    /// <summary>
    /// Gets a value indicating whether this item has been fully loaded (all nested data fetched).
    /// When <c>true</c>, null nested values mean no content exists.
    /// When <c>false</c>, null may mean the data was not fetched due to query depth limits.
    /// </summary>
    bool IsFullyLoaded { get; }

    /// <summary>
    /// Gets a value indicating whether the <see cref="Children"/> collection has been loaded.
    /// </summary>
    bool AreChildrenLoaded { get; }
}
