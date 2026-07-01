namespace Axlis.Fields;

/// <summary>
/// Represents a Sitecore General Link field.
/// </summary>
public interface IHyperlinkField : IBaseField
{
    /// <summary>Gets the parsed hyperlink value, or <c>null</c> if the field is empty.</summary>
    HyperlinkFieldValue? Value { get; }
}
