namespace Axlis.Attributes;

/// <summary>
/// Marks a class as a strongly-typed Sitecore template and records its template GUID.
/// Applied to classes that derive from <c>ExtendedItem</c>.
/// <para>
/// This attribute is the codegen hook — a future Roslyn source generator or CLI tool can read it
/// to scaffold template classes automatically without any breaking changes to consuming code.
/// </para>
/// </summary>
/// <example>
/// <code>
/// [SitecoreTemplate("{6D1CD897-1936-4A3A-A511-289A94C2A7B1}")]
/// public class DictionaryEntry : ExtendedItem { ... }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class SitecoreTemplateAttribute : Attribute
{
    /// <summary>Gets the Sitecore template GUID string (e.g. "{6D1CD897-1936-4A3A-A511-289A94C2A7B1}").</summary>
    public string TemplateId { get; }

    /// <summary>Initializes the attribute with the template's GUID string.</summary>
    /// <param name="templateId">The Sitecore template GUID, with or without braces.</param>
    public SitecoreTemplateAttribute(string templateId)
    {
        TemplateId = templateId;
    }
}
