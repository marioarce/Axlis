namespace Axlis.Core;

/// <summary>
/// Equality comparer for <see cref="ExtendedItem"/> instances based on their Sitecore item ID (GUID, case-insensitive).
/// Useful for deduplication in collections of fetched items.
/// </summary>
public sealed class ExtendedItemIdComparer : IEqualityComparer<ExtendedItem>
{
    /// <summary>Gets the singleton instance.</summary>
    public static readonly ExtendedItemIdComparer Instance = new();

    private ExtendedItemIdComparer() { }

    /// <inheritdoc/>
    public bool Equals(ExtendedItem? x, ExtendedItem? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }
        if (x is null || y is null)
        {
            return false;
        }
        return string.Equals(
            ItemTemplate.NormalizeGuid(x.Id),
            ItemTemplate.NormalizeGuid(y.Id),
            StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public int GetHashCode(ExtendedItem obj)
        => ItemTemplate.NormalizeGuid(obj.Id)?.GetHashCode(StringComparison.OrdinalIgnoreCase) ?? 0;
}
