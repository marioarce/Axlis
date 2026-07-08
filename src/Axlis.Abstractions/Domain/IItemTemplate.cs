namespace Axlis;

/// <summary>
/// Represents the template metadata for a Sitecore item.
/// </summary>
public interface IItemTemplate
{
    /// <summary>Gets the template name.</summary>
    string? TemplateName { get; }

    /// <summary>Gets the template unique identifier as a GUID string (e.g. "{6D1CD897-1936-4A3A-A511-289A94C2A7B1}").</summary>
    string? TemplateId { get; }

    /// <summary>Gets the fields defined on this template, or <c>null</c> if not loaded.</summary>
    IReadOnlyCollection<IItemTemplateField>? Fields { get; }
}
