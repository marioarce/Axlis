using System.Text.Json.Serialization;

namespace Axlis.Core.GraphQL;

/// <summary>
/// Paginated children container returned by the Sitecore Headless GraphQL API.
/// </summary>
public sealed class GraphQLChildrenData
{
    /// <summary>Gets or sets the authoritative total number of children (from Sitecore).</summary>
    [JsonPropertyName("total")]
    public int Total { get; set; }

    /// <summary>Gets or sets the child items in this page of results.</summary>
    [JsonPropertyName("results")]
    public List<GraphQLItemData>? Results { get; set; }
}
