using System.Text.Json.Serialization;

namespace Axlis.Core.GraphQL;

/// <summary>
/// Template metadata returned inside a <see cref="GraphQLItemData"/> from the Sitecore Headless GraphQL API.
/// </summary>
public sealed class GraphQLTemplateData
{
    /// <summary>Gets or sets the template ID (raw GUID string without braces).</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the template name.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
