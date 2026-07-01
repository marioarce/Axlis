using System.Text.Json.Serialization;

namespace Axlis.GraphQL.Models;

/// <summary>
/// Internal DTO serialized as the JSON body of an HTTP GraphQL request.
/// </summary>
internal sealed class GraphQLRequest
{
    /// <summary>Gets or sets the GraphQL query or mutation string.</summary>
    [JsonPropertyName("query")]
    public string Query { get; init; } = string.Empty;

    /// <summary>Gets or sets the variables for the operation, or <c>null</c> when not needed.</summary>
    [JsonPropertyName("variables")]
    public object? Variables { get; init; }

    /// <summary>Gets or sets the optional operation name (for documents with multiple operations).</summary>
    [JsonPropertyName("operationName")]
    public string? OperationName { get; init; }
}
