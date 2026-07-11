namespace Axlis.ORM.Core;

/// <summary>
/// The core Sitecore item domain model, returned by <c>ItemConverter</c>
/// and consumed by <c>ExtendedItem</c>, <c>AxesAdapter</c>, and field-type wrappers.
/// </summary>
public sealed class Item : IItem
{
    private IItem? _parent;
    private ItemChildrenData? _childrenData;
    private IItemTemplate? _template;
    private IReadOnlyCollection<IItemTemplateField>? _fields;

    /// <param name="id">Item GUID string.</param>
    /// <param name="path">Full Sitecore path.</param>
    /// <param name="name">Item name.</param>
    /// <param name="displayName">Language-specific display name.</param>
    /// <param name="version">Item version number.</param>
    /// <param name="hasChildren">Whether Sitecore reports this item as having children.</param>
    /// <param name="itemTemplate">Resolved template, or null.</param>
    /// <param name="parent">Parent item, or null when not fetched or root.</param>
    /// <param name="fields">Field collection, or null when not fetched.</param>
    /// <param name="children">Child items, or null when not fetched.</param>
    /// <param name="childrenTotalCount">Authoritative child count from the source (for partial-load detection).</param>
    public Item(
        string id,
        string path,
        string name,
        string? displayName,
        int version,
        bool hasChildren,
        IItemTemplate? itemTemplate,
        IItem? parent,
        List<ItemTemplateField>? fields,
        List<Item>? children,
        int? childrenTotalCount = null)
    {
        Id = id;
        Path = path;
        Name = name;
        DisplayName = displayName;
        Version = version;
        HasChildren = hasChildren;
        _template = itemTemplate;
        _parent = parent;
        _fields = fields?.AsReadOnly();

        if (children != null)
        {
            _childrenData = new ItemChildrenData(children.AsReadOnly(), childrenTotalCount);
        }
    }

    /// <inheritdoc/>
    public string Id { get; }
    /// <inheritdoc/>
    public string Name { get; }
    /// <inheritdoc/>
    public string? DisplayName { get; }
    /// <inheritdoc/>
    public string? Path { get; }
    /// <inheritdoc/>
    public string? Url { get; internal set; }
    /// <inheritdoc/>
    public int Version { get; }
    /// <inheritdoc/>
    public bool HasChildren { get; }

    /// <inheritdoc/>
    public IItemTemplate? Template => _template;

    /// <inheritdoc/>
    public IReadOnlyCollection<IItemTemplateField>? Fields => _fields;

    /// <inheritdoc/>
    public IItem? Parent => _parent;

    /// <inheritdoc/>
    public IReadOnlyList<IItem>? Children => _childrenData?.Children as IReadOnlyList<IItem>
        ?? _childrenData?.Children?.ToList();

    /// <summary>Gets the raw children data container (includes partial-load metadata).</summary>
    public ItemChildrenData? ChildrenData => _childrenData;

    /// <inheritdoc/>
    public bool AreChildrenLoaded
    {
        get
        {
            if (_childrenData == null)
            {
                return false;
            }
            if (!_childrenData.Loaded)
            {
                return false;
            }
            if (_childrenData.Children == null || _childrenData.Children.Count == 0)
            {
                return false;
            }
            if (_childrenData.TotalCount.HasValue && _childrenData.Children.Count < _childrenData.TotalCount.Value)
            {
                return false;
            }
            return true;
        }
    }

    /// <inheritdoc/>
    public bool IsFullyLoaded
    {
        get
        {
            if (string.IsNullOrEmpty(Id) || string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Path))
                return false;
            if (HasChildren && !AreChildrenLoaded)
                return false;
            if (_parent == null && !IsRootPath(Path))
                return false;
            return true;
        }
    }

    // ── Mutators (used by ItemConverter and AxesAdapter) ─────────────────────

    /// <summary>Sets the parent item (called by <c>ItemConverter</c> to wire up the tree).</summary>
    public void SetParent(IItem? parent) => _parent = parent;

    /// <summary>Sets the template (called by <c>ItemConverter</c>).</summary>
    public void SetTemplate(IItemTemplate? template) => _template = template;

    /// <summary>Sets the field collection (called by <c>ItemConverter</c>).</summary>
    public void SetFields(IReadOnlyCollection<IItemTemplateField>? fields)
    {
        if (fields?.Count > 0)
        {
            _fields = fields;
        }
    }

    /// <summary>Sets or replaces the children data (called by <c>AxesAdapter</c> on lazy re-fetch).</summary>
    public void SetChildrenData(IReadOnlyCollection<IItem>? children, int? totalCount = null)
    {
        if (children != null)
        {
            _childrenData = new ItemChildrenData(children, totalCount);
        }
    }

    /// <summary>Returns the children data container, or null if children were not loaded.</summary>
    public ItemChildrenData? GetChildrenData() => _childrenData;

    // ─────────────────────────────────────────────────────────────────────────

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
