namespace Axlis.ORM;

/// <summary>
/// Base contract for all strongly-typed Sitecore item wrappers.
/// Wraps a raw <see cref="IItem"/> and exposes typed field access and item metadata.
/// Decorate implementing classes with <see cref="Axlis.Attributes.SitecoreTemplateAttribute"/>
/// and properties with <see cref="Axlis.Attributes.SitecoreFieldAttribute"/> for codegen-readiness.
/// </summary>
public interface IBaseItem
{
    /// <summary>Gets the unique identifier of the item.</summary>
    string? Id { get; }

    /// <summary>Gets the item name.</summary>
    string? Name { get; }

    /// <summary>Gets the full Sitecore path of the item.</summary>
    string? Path { get; }

    /// <summary>Gets the underlying raw <see cref="IItem"/> (lazy-loaded on first access).</summary>
    IItem? InnerItem { get; }

    /// <summary>Associates the underlying raw item with this wrapper instance.</summary>
    /// <param name="item">The raw item to wrap.</param>
    void SetInnerItem(IItem item);

    /// <summary>
    /// Returns the <see cref="IItemTemplateField"/> for the given field name,
    /// or <c>null</c> if the field is not present on the item.
    /// </summary>
    /// <param name="fieldName">The Sitecore field name (case-insensitive).</param>
    IItemTemplateField? GetField(string fieldName);
}
