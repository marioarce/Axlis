using Axlis.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Axlis.GraphQL.Transport;

/// <summary>
/// Default <see cref="IGraphQLTransportFactory"/> that creates <see cref="HttpGraphQLTransport"/>
/// instances backed by named <see cref="HttpClient"/>s registered via <see cref="IHttpClientFactory"/>.
/// <para>
/// Single-site deployments use the named client <c>"Axlis.GraphQL"</c>.<br/>
/// Multi-site deployments use <c>"Axlis.GraphQL.{siteKey}"</c>; the consuming application must
/// register those additional named clients (e.g. via <c>AddAxlisGraphQL</c> overloads or manually).
/// </para>
/// </summary>
public sealed class HttpGraphQLTransportFactory : IGraphQLTransportFactory
{
    /// <summary>The named <see cref="HttpClient"/> used for the default single-site endpoint.</summary>
    public const string DefaultClientName = "Axlis.GraphQL";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggerFactory? _loggerFactory;

    /// <summary>
    /// Initializes a new instance of <see cref="HttpGraphQLTransportFactory"/>.
    /// </summary>
    /// <param name="httpClientFactory">Factory for creating named <see cref="HttpClient"/>s.</param>
    /// <param name="loggerFactory">Optional logger factory; passed through to each transport instance.</param>
    public HttpGraphQLTransportFactory(
        IHttpClientFactory httpClientFactory,
        ILoggerFactory? loggerFactory = null)
    {
        _httpClientFactory = httpClientFactory;
        _loggerFactory = loggerFactory;
    }

    /// <inheritdoc/>
    public IGraphQLTransport Create(string? siteKey = null)
    {
        var clientName = string.IsNullOrEmpty(siteKey)
            ? DefaultClientName
            : $"{DefaultClientName}.{siteKey}";

        var httpClient = _httpClientFactory.CreateClient(clientName);
        var logger = _loggerFactory?.CreateLogger<HttpGraphQLTransport>();
        return new HttpGraphQLTransport(httpClient, logger);
    }
}
