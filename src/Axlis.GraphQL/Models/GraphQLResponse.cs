using System.Text.Json.Serialization;

namespace Axlis.GraphQL.Models;

/// <summary>
/// Internal envelope for a GraphQL HTTP response. The <c>data</c> field is deserialized into
/// <typeparamref name="T"/>; the <c>errors</c> field captures any GraphQL-level errors.
/// </summary>
/// <typeparam name="T">The type of the <c>data</c> payload.</typeparam>
internal sealed class GraphQLResponse<T>
{
    /// <summary>Gets the response data payload.</summary>
    [JsonPropertyName("data")]
    public T? Data { get; init; }

    /// <summary>Gets the list of GraphQL errors, or <c>null</c> when the operation succeeded.</summary>
    [JsonPropertyName("errors")]
    public List<GraphQLError>? Errors { get; init; }

    /// <summary>Gets a value indicating whether the response contains any GraphQL errors.</summary>
    [JsonIgnore]
    public bool HasErrors => Errors != null && Errors.Count > 0;
}
