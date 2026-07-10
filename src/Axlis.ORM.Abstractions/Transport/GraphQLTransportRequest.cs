namespace Axlis.Transport;

/// <summary>
/// Represents a GraphQL operation to be sent by an <see cref="IGraphQLTransport"/>.
/// </summary>
public sealed class GraphQLTransportRequest
{
    /// <summary>Gets or sets the GraphQL query or mutation string.</summary>
    public string Query { get; init; } = string.Empty;

    /// <summary>Gets or sets the variables for the operation, or <c>null</c> for operations without variables.</summary>
    public object? Variables { get; init; }

    /// <summary>Gets or sets the optional operation name (for documents with multiple operations).</summary>
    public string? OperationName { get; init; }
}
