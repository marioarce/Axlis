namespace Axlis.Fields;

/// <summary>
/// Represents a Sitecore Single-Line Text or Multi-Line Text field.
/// </summary>
public interface ITextField : IBaseField
{
    /// <summary>Gets the text value of the field. Equivalent to <see cref="IBaseField.RawValue"/>.</summary>
    string? Value { get; }
}
