using Axlis.Fields;

namespace Axlis.Core.FieldTypes;

/// <summary>
/// Wraps a Sitecore Single-Line Text or Multi-Line Text field.
/// </summary>
public class TextField : BaseField, ITextField
{
    /// <param name="innerField">The raw field data.</param>
    public TextField(ItemTemplateField innerField) : base(innerField) { }

    /// <inheritdoc/>
    public string? Value => RawValue;

    /// <inheritdoc/>
    public override bool IsEmpty => string.IsNullOrEmpty(Value);
}
