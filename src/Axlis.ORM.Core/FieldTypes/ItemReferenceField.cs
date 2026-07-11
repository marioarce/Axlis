using Axlis.ORM.Fields;

namespace Axlis.ORM.Core.FieldTypes;

/// <summary>
/// Wraps a Sitecore Droplink or Droptree field that references a single item.
/// </summary>
public class ItemReferenceField : BaseField, IItemReferenceField
{
    private ItemReferenceFieldValue? _cached;

    /// <param name="innerField">The raw field data.</param>
    public ItemReferenceField(ItemTemplateField innerField) : base(innerField) { }

    /// <inheritdoc/>
    public override bool IsEmpty => string.IsNullOrEmpty(InnerField.TargetId)
        && InnerField.TargetItem == null;

    /// <inheritdoc/>
    public ItemReferenceFieldValue? Value
    {
        get
        {
            if (IsEmpty)
            {
                return null;
            }
            return _cached ??= BuildValue();
        }
    }

    /// <inheritdoc/>
    public T? AsItem<T>() where T : class, IBaseItem
    {
        var target = InnerField.TargetItem;
        if (target == null)
        {
            return null;
        }

        var instance = (T?)Activator.CreateInstance(typeof(T));
        instance?.SetInnerItem(target);
        return instance;
    }

    private ItemReferenceFieldValue BuildValue()
    {
        var target = InnerField.TargetItem;
        return new ItemReferenceFieldValue
        {
            Id = target?.Id ?? InnerField.TargetId,
            Name = target?.Name,
            DisplayName = target?.DisplayName,
            Path = target?.Path,
            Url = target?.Url,
            Version = target?.Version ?? 0,
            Template = target?.Template,
            Fields = target?.Fields
        };
    }
}
