using System.Diagnostics.CodeAnalysis;

namespace Axlis.ORM.Core;

/// <summary>
/// Concrete implementation of <see cref="IItemTemplate"/>.
/// Represents the Sitecore template associated with an <see cref="Item"/>.
/// </summary>
public sealed class ItemTemplate : IItemTemplate
{
    /// <param name="id">Raw template ID (with or without braces — braces are normalized on construction).</param>
    /// <param name="name">Template name.</param>
    public ItemTemplate(string id, string name)
    {
        TemplateId = NormalizeWithBraces(id);
        TemplateName = name;
    }

    /// <inheritdoc/>
    public string? TemplateName { get; }

    /// <inheritdoc/>
    public string? TemplateId { get; }

    /// <inheritdoc/>
    public IReadOnlyCollection<IItemTemplateField>? Fields => null;

    /// <summary>
    /// Compares this template's ID to the supplied ID using GUID normalization (case-insensitive, brace-agnostic).
    /// </summary>
    public bool Equals([NotNullWhen(true)] string? id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }

        return string.Equals(
            NormalizeGuid(TemplateId),
            NormalizeGuid(id),
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Normalizes a GUID string: strips braces, lowercases.</summary>
    public static string NormalizeGuid(string? guid)
        => guid?.Trim('{', '}').ToLowerInvariant() ?? string.Empty;

    private static string NormalizeWithBraces(string id)
    {
        var normalized = id.Trim('{', '}');
        return $"{{{normalized}}}";
    }
}
