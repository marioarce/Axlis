namespace Axlis.ORM;

/// <summary>
/// Extends <see cref="IBaseItem"/> with Synthesis-style tree traversal via <see cref="IAxes"/>
/// and cache-key support via <see cref="ICacheKeyValue"/>.
/// All strongly-typed Sitecore template classes should derive from <c>ExtendedItem</c>
/// which implements this interface.
/// </summary>
public interface IExtendedItem : IBaseItem, ICacheKeyValue
{
    /// <summary>
    /// Gets the axes adapter providing <c>Parent</c>, <c>Children</c>, <c>Siblings</c>,
    /// <c>GetChildren</c>, and <c>GetDescendants</c> tree traversal.
    /// </summary>
    IAxes Axes { get; }
}
