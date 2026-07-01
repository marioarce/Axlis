namespace Axlis.GraphQL.Models;

/// <summary>
/// Exception thrown when a GraphQL operation fails due to an HTTP error, a deserialization
/// failure, or one or more GraphQL-level errors returned by the server.
/// </summary>
public sealed class GraphQLClientException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="GraphQLClientException"/> with a message and no GraphQL errors.
    /// </summary>
    public GraphQLClientException(string message) : base(message)
    {
        Errors = Array.Empty<GraphQLError>();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="GraphQLClientException"/> with a message and a set of GraphQL errors.
    /// </summary>
    public GraphQLClientException(string message, IEnumerable<GraphQLError> errors) : base(message)
    {
        Errors = errors.ToList().AsReadOnly();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="GraphQLClientException"/> wrapping a lower-level exception.
    /// </summary>
    public GraphQLClientException(string message, Exception innerException) : base(message, innerException)
    {
        Errors = Array.Empty<GraphQLError>();
    }

    /// <summary>Gets the GraphQL-level errors returned by the server, if any.</summary>
    public IReadOnlyList<GraphQLError> Errors { get; }
}
