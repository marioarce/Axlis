namespace Axlis.Transport;

/// <summary>
/// Abstraction for sending GraphQL requests to a Sitecore Headless endpoint.
/// The default implementation in <c>Axlis.GraphQL</c> uses raw <c>HttpClient</c> + <c>System.Text.Json</c>.
/// Replace this with your own implementation via DI if needed.
/// </summary>
public interface IGraphQLTransport
{
    /// <summary>
    /// Executes a GraphQL operation and deserializes the <c>data</c> payload into <typeparamref name="TResponse"/>.
    /// </summary>
    /// <typeparam name="TResponse">The type of the GraphQL <c>data</c> object.</typeparam>
    /// <param name="request">The GraphQL request containing the query and optional variables.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The deserialized data payload, or <c>null</c> if the response contained no data.</returns>
    Task<TResponse?> ExecuteAsync<TResponse>(GraphQLTransportRequest request, CancellationToken ct = default);
}
