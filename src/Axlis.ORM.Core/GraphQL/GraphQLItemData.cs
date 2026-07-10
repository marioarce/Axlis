using System.Text.Json.Serialization;

namespace Axlis.Core.GraphQL;

/// <summary>
/// Represents a Sitecore item as returned by the Headless GraphQL API.
/// Used as the deserialization target for <c>ItemConverter.ToItem()</c>.
/// </summary>
public sealed class GraphQLItemData
{
    /// <summary>Gets or sets the GraphQL <c>__typename</c>.</summary>
    [JsonPropertyName("__typename")]
    public string? Typename { get; set; }

    /// <summary>Gets or sets the item ID (raw GUID string).</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the item name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the language-specific display name.</summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    /// <summary>Gets or sets the full Sitecore path.</summary>
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    /// <summary>Gets or sets the item version number.</summary>
    [JsonPropertyName("version")]
    public int Version { get; set; }

    /// <summary>Gets or sets a value indicating whether the item has children in Sitecore.</summary>
    [JsonPropertyName("hasChildren")]
    public bool HasChildren { get; set; }

    /// <summary>Gets or sets the template metadata.</summary>
    [JsonPropertyName("template")]
    public GraphQLTemplateData? Template { get; set; }

    /// <summary>Gets or sets the field collection. All Sitecore field types are merged into <see cref="GraphQLFieldData"/>.</summary>
    [JsonPropertyName("fields")]
    public List<GraphQLFieldData>? Fields { get; set; }

    /// <summary>Gets or sets the parent item (one level up).</summary>
    [JsonPropertyName("parent")]
    public GraphQLItemData? Parent { get; set; }

    /// <summary>Gets or sets the paginated children container.</summary>
    [JsonPropertyName("children")]
    public GraphQLChildrenData? Children { get; set; }
}
