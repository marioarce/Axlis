namespace Axlis;

/// <summary>
/// Represents a single field on a Sitecore item template, including its value and metadata.
/// </summary>
public interface IItemTemplateField
{
    /// <summary>Gets the field unique identifier.</summary>
    string? FieldId { get; }

    /// <summary>Gets the field name.</summary>
    string? FieldName { get; }

    /// <summary>Gets the raw string value of the field.</summary>
    string? FieldValue { get; }

    /// <summary>Gets the Sitecore field type (e.g. "Single-Line Text", "Image", "Droplink").</summary>
    string? FieldType { get; }

    /// <summary>Gets a value indicating whether the field is shared across all languages and versions.</summary>
    bool Shared { get; }

    /// <summary>Gets a value indicating whether the field is unversioned (shared across versions but language-specific).</summary>
    bool Unversioned { get; }
}
