using Axlis.Fields;

namespace Axlis.Core.FieldTypes;

/// <summary>
/// Wraps a Sitecore Multilist or Treelist field that references multiple items.
/// </summary>
public class MultilistField : BaseField, IMultilistField
{
    /// <param name="innerField">The raw field data.</param>
    public MultilistField(ItemTemplateField innerField) : base(innerField) { }

    /// <inheritdoc/>
    public override bool IsEmpty => (InnerField.TargetIds?.Count ?? 0) == 0
        && (InnerField.TargetItems?.Count ?? 0) == 0;

    /// <inheritdoc/>
    public IReadOnlyList<string> Ids
    {
        get
        {
            if (InnerField.TargetIds == null)
            {
                return Array.Empty<string>();
            }
            var result = new List<string>();
            foreach (var id in InnerField.TargetIds)
            {
                if (id != null) result.Add(id);
            }
            return result.AsReadOnly();
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<T> As<T>() where T : class, IBaseItem
    {
        var items = InnerField.TargetItems;
        if (items == null || items.Count == 0)
        {
            return Array.Empty<T>();
        }

        var result = new List<T>(items.Count);
        foreach (var item in items)
        {
            var instance = (T?)Activator.CreateInstance(typeof(T));
            if (instance == null) continue;
            instance.SetInnerItem(item);
            result.Add(instance);
        }
        return result.AsReadOnly();
    }
}
