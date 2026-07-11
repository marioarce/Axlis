using Axlis.ORM.Fields;

namespace Axlis.ORM.Core.FieldTypes;

/// <summary>
/// Wraps a Sitecore Checkbox field.
/// </summary>
public class BooleanField : BaseField, IBooleanField
{
    /// <param name="innerField">The raw field data.</param>
    public BooleanField(ItemTemplateField innerField) : base(innerField) { }

    /// <inheritdoc/>
    /// <remarks>
    /// Checks <see cref="ItemTemplateField.BoolValue"/> first; falls back to
    /// comparing the raw string value to "1".
    /// </remarks>
    public bool Value => InnerField.BoolValue ?? string.Equals(RawValue, "1", StringComparison.Ordinal);

    /// <inheritdoc/>
    public override bool IsEmpty => false;
}
