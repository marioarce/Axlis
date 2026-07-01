namespace Axlis.Fields;

/// <summary>
/// Represents a Sitecore Checkbox field.
/// </summary>
public interface IBooleanField : IBaseField
{
    /// <summary>Gets the boolean value of the field (<c>true</c> when the raw value is "1").</summary>
    bool Value { get; }
}
