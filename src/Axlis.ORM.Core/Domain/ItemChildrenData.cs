namespace Axlis.ORM.Core;

/// <summary>
/// Holds the children collection for an <see cref="Item"/>, including its loaded state
/// and the authoritative total count (for partial-load detection).
/// </summary>
public sealed class ItemChildrenData
{
    private readonly bool _loaded;
    private readonly IReadOnlyCollection<IItem>? _children;
    private readonly int? _totalCount;

    /// <param name="children">Child items to store.</param>
    /// <param name="totalCount">Authoritative total count from the source; null if unknown.</param>
    public ItemChildrenData(IReadOnlyCollection<IItem>? children, int? totalCount = null)
    {
        var hasData = children?.Count > 0;

        if (hasData)
        {
            _children = children;
            _loaded = true;
        }

        _totalCount = totalCount;
    }

    /// <summary>Gets a value indicating whether the children collection has been loaded and contains data.</summary>
    public bool Loaded => _loaded;

    /// <summary>Gets the child items, or <c>null</c> if not loaded.</summary>
    public IReadOnlyCollection<IItem>? Children => _children;

    /// <summary>
    /// Gets the authoritative total count from the source, or <c>null</c> if unknown.
    /// When set and greater than <see cref="Children"/>.Count, the local collection is a partial load.
    /// </summary>
    public int? TotalCount => _totalCount;
}
