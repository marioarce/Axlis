using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Axlis.GraphQL.Models;
using Axlis.Transport;
using Microsoft.Extensions.Logging;

namespace Axlis.GraphQL.Transport;

/// <summary>
/// Default <see cref="IGraphQLTransport"/> implementation that uses a raw <see cref="HttpClient"/>
/// and <see cref="System.Text.Json"/> to communicate with a Sitecore Headless GraphQL endpoint.
/// No third-party GraphQL library is required.
/// </summary>
public sealed class HttpGraphQLTransport : IGraphQLTransport
{
    private const string Tag = "graphql.transport";
    private const string MediaType = "application/json";

    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpGraphQLTransport>? _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = null,
        MaxDepth = 256,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Initializes a new instance of <see cref="HttpGraphQLTransport"/>.
    /// </summary>
    /// <param name="httpClient">
    /// An <see cref="HttpClient"/> pre-configured with the GraphQL endpoint as its
    /// <see cref="HttpClient.BaseAddress"/>. Lifetime is managed by <see cref="IHttpClientFactory"/>.
    /// </param>
    /// <param name="logger">Optional logger; pass <c>null</c> to disable logging.</param>
    public HttpGraphQLTransport(HttpClient httpClient, ILogger<HttpGraphQLTransport>? logger = null)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<TResponse?> ExecuteAsync<TResponse>(
        GraphQLTransportRequest request,
        CancellationToken ct = default)
    {
        var internalRequest = new GraphQLRequest
        {
            Query = request.Query,
            Variables = request.Variables,
            OperationName = request.OperationName
        };

        string? content = null;
        var statusCode = 0;

        try
        {
            var json = JsonSerializer.Serialize(internalRequest, _jsonOptions);
            using var httpContent = new StringContent(json, Encoding.UTF8, MediaType);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, (string?)null)
            {
                Content = httpContent
            };

            _logger?.LogDebug(
                "{Tag}: Executing GraphQL request. Variables: {@Variables}",
                Tag, request.Variables);

            using var httpResponse = await _httpClient
                .SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, ct)
                .ConfigureAwait(false);

            statusCode = (int)httpResponse.StatusCode;

            content = await httpResponse.Content
                .ReadAsStringAsync(ct)
                .ConfigureAwait(false);

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger?.LogError(
                    "{Tag}: HTTP {StatusCode} — {Preview}",
                    Tag, statusCode,
                    content?.Length > 500 ? content[..500] : content);

                throw new GraphQLClientException(
                    $"HTTP request failed with status {statusCode}: {httpResponse.ReasonPhrase}");
            }

            var response = JsonSerializer.Deserialize<GraphQLResponse<TResponse>>(content, _jsonOptions)
                ?? throw new GraphQLClientException("Failed to deserialize GraphQL response: null envelope.");

            if (response.HasErrors)
            {
                var errors = response.Errors!;
                var errorMessages = string.Join("; ", errors.Select(e => e.Message));

                _logger?.LogWarning(
                    "{Tag}: GraphQL errors returned: {Errors}",
                    Tag, errorMessages);

                throw new GraphQLClientException(
                    $"GraphQL operation failed: {errorMessages}", errors);
            }

            _logger?.LogDebug("{Tag}: GraphQL request succeeded.", Tag);

            return response.Data;
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "{Tag}: HTTP request failed. Status: {Status}", Tag, statusCode);
            throw new GraphQLClientException("HTTP request to GraphQL endpoint failed.", ex);
        }
        catch (JsonException ex)
        {
            _logger?.LogError(ex,
                "{Tag}: Deserialization failed. Content length: {Length}. Preview: {Preview}",
                Tag, content?.Length ?? 0,
                content?.Length > 200 ? content[..200] : content);

            throw new GraphQLClientException("Failed to deserialize GraphQL response.", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger?.LogError(ex, "{Tag}: Request timed out or was cancelled.", Tag);
            throw new GraphQLClientException("GraphQL request timed out or was cancelled.", ex);
        }
        catch (Exception ex) when (ex is not GraphQLClientException)
        {
            _logger?.LogError(ex,
                "{Tag}: Unexpected error executing GraphQL request. Type: {Type}",
                Tag, ex.GetType().Name);

            throw new GraphQLClientException($"Unexpected error: {ex.Message}", ex);
        }
    }
}
