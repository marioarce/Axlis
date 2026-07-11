namespace Axlis.ORM.Core;

/// <summary>
/// Synthesis-style <see cref="IAxes"/> implementation that wraps an <see cref="Item"/>
/// and provides strongly-typed tree traversal.
/// Lazy-fetches missing parent/children data via <see cref="IItemLazyLoader"/> when needed.
/// </summary>
public sealed class AxesAdapter : IAxes
{
    private readonly Item _item;
    private readonly IItemLazyLoader? _loader;

    /// <param name="item">The item whose axes this adapter represents.</param>
    /// <param name="loader">Optional lazy-loader for on-demand parent/children re-fetch.</param>
    public AxesAdapter(Item item, IItemLazyLoader? loader = null)
    {
        _item = item;
        _loader = loader;
    }

    /// <inheritdoc/>
    public IExtendedItem? Parent
    {
        get
        {
            if (_item.Parent == null && !IsRootPath(_item.Path))
            {
                TryUpdateAxes();
            }

            var parent = _item.Parent;
            if (parent is not Item parentItem)
            {
                return null;
            }

            var extended = new ExtendedItem(parentItem);
            return extended;
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<IExtendedItem>? Children
    {
        get
        {
            if (_item.HasChildren && !_item.AreChildrenLoaded)
            {
                TryUpdateAxes();
            }

            var children = _item.ChildrenData?.Children;
            if (children == null)
            {
                return null;
            }

            var result = new List<IExtendedItem>(children.Count);
            foreach (var child in children)
            {
                if (child is Item childItem)
                {
                    result.Add(new ExtendedItem(childItem));
                }
            }
            return result.AsReadOnly();
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<IExtendedItem>? Siblings
    {
        get
        {
            var parentChildren = Parent?.Axes.Children;
            if (parentChildren == null)
            {
                return null;
            }

            var result = new List<IExtendedItem>(parentChildren.Count);
            foreach (var sibling in parentChildren)
            {
                if (sibling == null) continue;
                result.Add(sibling);
            }
            return result.AsReadOnly();
        }
    }

    /// <inheritdoc/>
    public T? ClosestParent<T>() where T : class, IExtendedItem
    {
        IExtendedItem? cursor = new ExtendedItem(_item);
        while (cursor != null)
        {
            if (cursor is T typed)
            {
                return typed;
            }

            // Try to cast via re-instantiation
            if (cursor.InnerItem is Item cursorItem)
            {
                var asT = TryCast<T>(cursorItem);
                if (asT != null)
                {
                    return asT;
                }
            }

            cursor = cursor.Axes.Parent;
        }

        return null;
    }

    /// <inheritdoc/>
    public IReadOnlyList<T> GetChildren<T>(Func<T, bool>? predicate = null) where T : class, IExtendedItem
    {
        var children = Children;
        if (children == null)
        {
            return Array.Empty<T>();
        }

        var result = new List<T>();
        foreach (var child in children)
        {
            if (child.InnerItem is not Item inner) continue;
            var typed = TryCast<T>(inner);
            if (typed == null) continue;
            if (predicate == null || predicate(typed)) result.Add(typed);
        }
        return result.AsReadOnly();
    }

    /// <inheritdoc/>
    public IReadOnlyList<T> GetDescendants<T>(Func<T, bool>? predicate = null) where T : class, IExtendedItem
    {
        var result = new List<T>();
        CollectDescendants(_item, result, predicate);
        return result.AsReadOnly();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void TryUpdateAxes()
    {
        if (_item.IsFullyLoaded || _loader == null)
        {
            return;
        }

        try
        {
            var fetched = _loader.LoadItem(_item.Id);
            if (fetched == null)
            {
                return;
            }

            var fetchedChildren = fetched.ChildrenData?.Children;
            var fetchedTotal = fetched.ChildrenData?.TotalCount;

            var cachedIsPartial = _item.ChildrenData?.TotalCount.HasValue == true
                && _item.ChildrenData.Children?.Count < _item.ChildrenData.TotalCount;

            if ((_item.ChildrenData?.Children == null || cachedIsPartial) && fetchedChildren != null)
            {
                _item.SetChildrenData(fetchedChildren, fetchedTotal);
            }

            if (fetched.Parent != null)
            {
                _item.SetParent(fetched.Parent);
            }
        }
        catch
        {
            // Swallow — lazy-fetch is best-effort; caller receives null/empty
        }
    }

    private static void CollectDescendants<T>(Item item, List<T> result, Func<T, bool>? predicate)
        where T : class, IExtendedItem
    {
        var wrapper = new ExtendedItem(item);
        var children = wrapper.Axes.Children;
        if (children == null)
        {
            return;
        }

        foreach (var child in children)
        {
            if (child?.InnerItem is not Item childItem) continue;
            var typed = TryCast<T>(childItem);
            if (typed != null && (predicate == null || predicate(typed)))
            {
                result.Add(typed);
            }
            CollectDescendants(childItem, result, predicate);
        }
    }

    private static T? TryCast<T>(Item item) where T : class, IExtendedItem
    {
        if (typeof(T) == typeof(ExtendedItem) || typeof(T).IsAssignableFrom(typeof(ExtendedItem)))
        {
            var ext = new ExtendedItem(item);
            return ext as T;
        }

        var instance = Activator.CreateInstance(typeof(T)) as T;
        instance?.SetInnerItem(item);
        return instance;
    }

    private static bool IsRootPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }
        var normalized = path.TrimEnd('/');
        if (normalized.Length == 0)
        {
            normalized = "/";
        }
        return string.Equals(normalized, "/sitecore", StringComparison.OrdinalIgnoreCase);
    }
}
