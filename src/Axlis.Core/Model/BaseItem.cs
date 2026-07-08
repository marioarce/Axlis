namespace Axlis.Core;

/// <summary>
/// Abstract base for all strongly-typed Sitecore item wrappers.
/// Implements <see cref="IBaseItem"/>: holds the underlying <see cref="Item"/>,
/// exposes core properties (Id, Name, Path) and provides field lookup.
/// </summary>
public abstract class BaseItem : IBaseItem
{
    private Item? _innerItem;

    /// <summary>Initializes an empty instance (inner item set later via <see cref="SetInnerItem(IItem)"/>).</summary>
    protected BaseItem() { }

    /// <summary>Initializes with an already-loaded <see cref="Item"/>.</summary>
    /// <param name="item">The raw item to wrap.</param>
    protected BaseItem(Item item)
    {
        _innerItem = item;
    }

    /// <inheritdoc/>
    public virtual string? Id => _innerItem?.Id;
    /// <inheritdoc/>
    public virtual string? Name => _innerItem?.Name;
    /// <inheritdoc/>
    public virtual string? Path => _innerItem?.Path;

    /// <summary>Gets the underlying raw <see cref="Item"/> (may be null if not yet loaded).</summary>
    IItem? IBaseItem.InnerItem => _innerItem;

    /// <summary>Gets the underlying raw <see cref="Item"/> as the concrete type.</summary>
    protected internal Item? RawInnerItem => _innerItem;

    /// <inheritdoc/>
    public void SetInnerItem(IItem item)
    {
        _innerItem = (Item)item;
    }

    /// <summary>Sets the inner item directly from the concrete type.</summary>
    public void SetInnerItem(Item item)
    {
        _innerItem = item;
    }

    /// <inheritdoc/>
    public IItemTemplateField? GetField(string fieldName)
    {
        return _innerItem?.Fields?
            .FirstOrDefault(f => string.Equals(f.FieldName, fieldName, StringComparison.OrdinalIgnoreCase));
    }
}
