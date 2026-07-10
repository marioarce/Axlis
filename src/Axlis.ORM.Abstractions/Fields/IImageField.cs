namespace Axlis.Fields;

/// <summary>
/// Represents a Sitecore Image field with responsive source URLs.
/// </summary>
public interface IImageField : IBaseField
{
    /// <summary>Gets the parsed image value with alternative text and responsive sources, or <c>null</c> if the field is empty.</summary>
    WebImage? Value { get; }
}
