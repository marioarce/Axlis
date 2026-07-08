namespace Axlis.Fields;

/// <summary>
/// Represents a Sitecore File field (media library attachment).
/// </summary>
public interface IFileField : IBaseField
{
    /// <summary>Gets the URL or path to the media library file, or <c>null</c> if the field is empty.</summary>
    string? Src { get; }
}
