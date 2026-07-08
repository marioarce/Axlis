using System.Text.Json.Serialization;

namespace Axlis.GraphQL.Models;

/// <summary>
/// Represents a single error object returned by a Sitecore GraphQL endpoint.
/// </summary>
public sealed class GraphQLError
{
    /// <summary>Gets the human-readable error message.</summary>
    [JsonPropertyName("message")]
    public string? Message { get; init; }

    /// <summary>Gets the source locations in the query where the error occurred.</summary>
    [JsonPropertyName("locations")]
    public List<GraphQLErrorLocation>? Locations { get; init; }

    /// <summary>Gets the field path to the value that caused the error.</summary>
    [JsonPropertyName("path")]
    public List<object>? Path { get; init; }

    /// <summary>Gets implementation-specific error extensions.</summary>
    [JsonPropertyName("extensions")]
    public Dictionary<string, object>? Extensions { get; init; }
}
