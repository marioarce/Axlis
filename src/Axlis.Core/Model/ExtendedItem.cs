using System.Text;
using Axlis.Core.FieldTypes;

namespace Axlis.Core;

/// <summary>
/// Base class for all strongly-typed Sitecore template POCOs.
/// Extends <see cref="BaseItem"/> with <see cref="Axes"/> tree traversal,
/// <see cref="GetCacheKeyValue"/> cache-key support, and a
/// <see cref="GetField{TField}(string)"/> helper for declarative field accessors.
/// <para>
/// Decorate subclasses with <c>[SitecoreTemplate]</c> and properties with
/// <c>[SitecoreField]</c> for codegen-readiness.
/// </para>
/// </summary>
public class ExtendedItem : BaseItem, IExtendedItem
{
    // Ambient lazy-loader injected by the Axlis (facade) package on startup.
    private static IItemLazyLoader? _lazyLoader;

    /// <summary>Initializes the ambient lazy-loader used by <see cref="ExtendedItem.RawInnerItem"/> and <see cref="AxesAdapter"/>.</summary>
    /// <remarks>Call this once at application startup, typically inside <c>AddAxlis()</c>.</remarks>
    public static void Initialize(IItemLazyLoader loader) => _lazyLoader = loader;

    /// <summary>Clears the ambient lazy-loader (for testing).</summary>
    public static void Reset() => _lazyLoader = null;

    /// <summary>Initializes an empty instance (inner item set later).</summary>
    public ExtendedItem() { }

    /// <summary>Initializes with an already-loaded <see cref="Item"/>.</summary>
    public ExtendedItem(Item item) : base(item) { }

    /// <summary>
    /// Gets the underlying raw <see cref="Item"/>, lazy-fetching via <see cref="IItemLazyLoader"/>
    /// if the item was not fully loaded at construction time.
    /// </summary>
    public new Item? RawInnerItem
    {
        get
        {
            var inner = base.RawInnerItem;
            if (inner == null && _lazyLoader != null && Id != null)
            {
                inner = _lazyLoader.LoadItem(Id);
                if (inner != null) SetInnerItem(inner);
            }
            return inner;
        }
    }

    /// <inheritdoc/>
    public IAxes Axes => new AxesAdapter(RawInnerItem!, _lazyLoader);

    /// <inheritdoc/>
    public string GetCacheKeyValue()
    {
        var sb = new StringBuilder(64);
        sb.Append(GetType().Name);
        sb.Append('|');
        var normalizedId = ItemTemplate.NormalizeGuid(Id);
        sb.Append(string.IsNullOrEmpty(normalizedId) ? "null" : normalizedId);
        sb.Append('|');
        sb.Append(Name?.ToLowerInvariant() ?? "null");
        sb.Append('|');
        sb.Append('v');
        sb.Append(RawInnerItem?.Version.ToString() ?? "null");
        return sb.ToString();
    }

    /// <summary>
    /// Returns a strongly-typed field wrapper of type <typeparamref name="TField"/> for the given field name.
    /// Returns an empty/sentinel field when the item has no field by that name.
    /// </summary>
    /// <typeparam name="TField">A <see cref="BaseField"/> subtype (e.g. <see cref="TextField"/>).</typeparam>
    /// <param name="fieldName">The Sitecore field name (case-insensitive).</param>
    protected TField GetField<TField>(string fieldName) where TField : BaseField
    {
        var rawField = RawInnerItem?.Fields?
            .OfType<ItemTemplateField>()
            .FirstOrDefault(f => string.Equals(f.FieldName, fieldName, StringComparison.OrdinalIgnoreCase))
            ?? ItemTemplateField.Empty(fieldName);

        return (TField)Activator.CreateInstance(typeof(TField), rawField)!;
    }
}
