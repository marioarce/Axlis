using Axlis.Fields;

namespace Axlis.Core.FieldTypes;

/// <summary>
/// Abstract base for all Axlis field-type wrappers.
/// Holds the underlying <see cref="ItemTemplateField"/> and implements <see cref="IBaseField"/>.
/// </summary>
public abstract class BaseField : IBaseField
{
    /// <param name="innerField">The raw <see cref="ItemTemplateField"/> this wrapper decorates.</param>
    protected BaseField(ItemTemplateField innerField)
    {
        InnerField = innerField;
    }

    /// <summary>Gets the underlying raw field data.</summary>
    public ItemTemplateField InnerField { get; }

    /// <inheritdoc/>
    public string? FieldName => InnerField.FieldName;

    /// <inheritdoc/>
    public string? RawValue => InnerField.StringValue;

    /// <inheritdoc/>
    public virtual bool IsEmpty => string.IsNullOrEmpty(RawValue);
}
