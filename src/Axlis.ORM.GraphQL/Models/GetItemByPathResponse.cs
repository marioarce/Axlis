using System.Text.Json.Serialization;
using Axlis.Core.GraphQL;

namespace Axlis.GraphQL.Models;

/// <summary>
/// Internal deserialization root for the <c>GetItemByPath</c> and <c>GetItemById</c> GraphQL queries.
/// </summary>
internal sealed class GetItemByPathResponse
{
    /// <summary>Gets the item data, or <c>null</c> if the item was not found.</summary>
    [JsonPropertyName("item")]
    public GraphQLItemData? Item { get; init; }
}
