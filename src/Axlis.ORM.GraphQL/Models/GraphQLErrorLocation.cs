using System.Text.Json.Serialization;

namespace Axlis.ORM.GraphQL.Models;

/// <summary>
/// Identifies the line and column in a GraphQL query document where an error was detected.
/// </summary>
public sealed class GraphQLErrorLocation
{
    /// <summary>Gets the 1-based line number.</summary>
    [JsonPropertyName("line")]
    public int Line { get; init; }

    /// <summary>Gets the 1-based column number.</summary>
    [JsonPropertyName("column")]
    public int Column { get; init; }
}
