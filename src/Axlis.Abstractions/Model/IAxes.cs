namespace Axlis;

/// <summary>
/// Provides Synthesis-style tree traversal operations for a Sitecore item.
/// Accessed via <see cref="IExtendedItem.Axes"/>.
/// </summary>
public interface IAxes
{
    /// <summary>Gets the parent item as an <see cref="IExtendedItem"/>, or <c>null</c> if this is the root.</summary>
    IExtendedItem? Parent { get; }

    /// <summary>Gets the direct children of the item.</summary>
    IReadOnlyList<IExtendedItem>? Children { get; }

    /// <summary>Gets the siblings of the item (other children of the same parent, excluding this item).</summary>
    IReadOnlyList<IExtendedItem>? Siblings { get; }

    /// <summary>
    /// Walks up the ancestor chain and returns the first ancestor that can be cast to <typeparamref name="T"/>,
    /// or <c>null</c> if none is found.
    /// </summary>
    /// <typeparam name="T">The strongly-typed item class derived from <see cref="IExtendedItem"/>.</typeparam>
    T? ClosestParent<T>() where T : class, IExtendedItem;

    /// <summary>
    /// Returns the direct children that can be cast to <typeparamref name="T"/>,
    /// optionally filtered by <paramref name="predicate"/>.
    /// </summary>
    /// <typeparam name="T">The strongly-typed item class derived from <see cref="IExtendedItem"/>.</typeparam>
    /// <param name="predicate">Optional filter applied after the type cast.</param>
    IReadOnlyList<T> GetChildren<T>(Func<T, bool>? predicate = null) where T : class, IExtendedItem;

    /// <summary>
    /// Returns all descendants (recursive) that can be cast to <typeparamref name="T"/>,
    /// optionally filtered by <paramref name="predicate"/>.
    /// </summary>
    /// <typeparam name="T">The strongly-typed item class derived from <see cref="IExtendedItem"/>.</typeparam>
    /// <param name="predicate">Optional filter applied after the type cast.</param>
    IReadOnlyList<T> GetDescendants<T>(Func<T, bool>? predicate = null) where T : class, IExtendedItem;
}
