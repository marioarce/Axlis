using System.Text.Json.Serialization;
using Axlis.Core.GraphQL;

namespace Axlis.GraphQL.Models;

/// <summary>
/// Internal deserialization root for the <c>GetItemFlat</c> GraphQL query.
/// Returns only item data — no parent, ancestors, or children.
/// </summary>
internal sealed class GetItemFlatResponse
{
    /// <summary>Gets the item data, or <c>null</c> if the item was not found.</summary>
    [JsonPropertyName("item")]
    public GraphQLItemData? Item { get; init; }
}
