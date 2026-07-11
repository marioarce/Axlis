namespace Axlis.ORM;

/// <summary>
/// Represents the resolved value of a Sitecore Droplink / Droptree field,
/// exposing the referenced item's key properties without requiring a full <c>Item</c> load.
/// </summary>
public class ItemReferenceFieldValue
{
    /// <summary>Gets the unique identifier of the referenced item.</summary>
    public string? Id { get; init; }

    /// <summary>Gets the name of the referenced item.</summary>
    public string? Name { get; init; }

    /// <summary>Gets the display name of the referenced item.</summary>
    public string? DisplayName { get; init; }

    /// <summary>Gets the Sitecore path of the referenced item.</summary>
    public string? Path { get; init; }

    /// <summary>Gets the URL of the referenced item.</summary>
    public string? Url { get; init; }

    /// <summary>Gets the version number of the referenced item.</summary>
    public int Version { get; init; }

    /// <summary>Gets the template metadata for the referenced item, or <c>null</c> if not loaded.</summary>
    public IItemTemplate? Template { get; init; }

    /// <summary>Gets the field collection for the referenced item, or <c>null</c> if not loaded.</summary>
    public IReadOnlyCollection<IItemTemplateField>? Fields { get; init; }
}
