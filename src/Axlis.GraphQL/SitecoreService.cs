using System.Text.Json;
using System.Text.Json.Serialization;
using Axlis.Core;
using Axlis.Core.GraphQL;
using Axlis.GraphQL.Models;
using Axlis.GraphQL.Queries;
using Axlis.GraphQL.Transport;
using Axlis.Services;
using Axlis.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Axlis.GraphQL;

/// <summary>
/// Default <see cref="ISitecoreService"/> implementation that fetches Sitecore items via the
/// Axlis GraphQL transport layer. Performs no caching; caching is handled by
/// <c>SitecoreItemCacheManager</c> in the <c>Axlis</c> facade package.
/// </summary>
public sealed class SitecoreService : ISitecoreService
{
    private const string Tag = "graphql.service";

    private readonly IGraphQLTransportFactory _transportFactory;
    private readonly int _batchSize;
    private readonly ILogger<SitecoreService>? _logger;

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
    /// Initializes a new instance of <see cref="SitecoreService"/>.
    /// </summary>
    /// <param name="transportFactory">Factory used to obtain an <see cref="IGraphQLTransport"/> per operation.</param>
    /// <param name="options">GraphQL transport options (endpoint, batch size, etc.).</param>
    /// <param name="logger">Optional logger; pass <c>null</c> to disable logging.</param>
    public SitecoreService(
        IGraphQLTransportFactory transportFactory,
        IOptions<AxlisGraphQLOptions> options,
        ILogger<SitecoreService>? logger = null)
    {
        _transportFactory = transportFactory;
        _batchSize = options.Value.BatchSize > 0
            ? options.Value.BatchSize
            : AxlisGraphQLOptions.DefaultBatchSize;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IItem?> GetItemByPathAsync(string path, CancellationToken ct = default)
    {
        _logger?.LogDebug("{Tag}: GetItemByPath [{Path}]", Tag, path);

        var transport = _transportFactory.Create();
        var request = new GraphQLTransportRequest
        {
            Query = GetItemByPathQuery.Query,
            Variables = new { path }
        };

        var response = await transport
            .ExecuteAsync<GetItemByPathResponse>(request, ct)
            .ConfigureAwait(false);

        var item = ItemConverter.ToItem(response?.Item);

        LogItemResult(item, path);
        return item;
    }

    /// <inheritdoc/>
    public async Task<IItem?> GetItemByIdAsync(string id, CancellationToken ct = default)
    {
        _logger?.LogDebug("{Tag}: GetItemById [{Id}]", Tag, id);

        var transport = _transportFactory.Create();

        // Sitecore's item(path:) argument accepts both paths and GUIDs.
        var request = new GraphQLTransportRequest
        {
            Query = GetItemByPathQuery.Query,
            Variables = new { path = id }
        };

        var response = await transport
            .ExecuteAsync<GetItemByPathResponse>(request, ct)
            .ConfigureAwait(false);

        var item = ItemConverter.ToItem(response?.Item);

        LogItemResult(item, id);
        return item;
    }

    /// <inheritdoc/>
    public async Task<IDictionary<string, IItem?>> GetItemsByPathsAsync(
        IEnumerable<string> paths,
        CancellationToken ct = default)
    {
        var pathList = paths?.ToList() ?? new List<string>();
        var result = new Dictionary<string, IItem?>(pathList.Count, StringComparer.OrdinalIgnoreCase);

        if (pathList.Count == 0)
        {
            return result;
        }

        _logger?.LogDebug(
            "{Tag}: GetItemsByPaths — {Count} path(s), batch size {BatchSize}",
            Tag, pathList.Count, _batchSize);

        // Chunk paths and process each batch, preserving order in the result dict.
        var batches = ChunkList(pathList, _batchSize);
        var batchCount = batches.Count;

        var batchTasks = batches.Select((batch, index) =>
            ExecuteBatchAsync(batch, index, batchCount, ct));

        var batchResults = await Task.WhenAll(batchTasks).ConfigureAwait(false);

        foreach (var batchResult in batchResults)
        {
            foreach (var (path, item) in batchResult)
            {
                result[path] = item;
            }
        }

        _logger?.LogDebug(
            "{Tag}: GetItemsByPaths — {Found}/{Total} items found.",
            Tag, result.Count(r => r.Value != null), pathList.Count);

        return result;
    }

    /// <inheritdoc/>
    public async Task<IItem?> GetItemFlatAsync(string path, CancellationToken ct = default)
    {
        _logger?.LogDebug("{Tag}: GetItemFlat [{Path}]", Tag, path);

        var transport = _transportFactory.Create();
        var request = new GraphQLTransportRequest
        {
            Query = GetItemFlatQuery.Query,
            Variables = new { path }
        };

        var response = await transport
            .ExecuteAsync<GetItemFlatResponse>(request, ct)
            .ConfigureAwait(false);

        var item = ItemConverter.ToItem(response?.Item);

        LogItemResult(item, path);
        return item;
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    /// <summary>
    /// Executes a single batch query for a subset of paths using GraphQL aliases.
    /// </summary>
    private async Task<Dictionary<string, IItem?>> ExecuteBatchAsync(
        List<string> paths,
        int batchIndex,
        int totalBatches,
        CancellationToken ct)
    {
        var result = new Dictionary<string, IItem?>(paths.Count, StringComparer.OrdinalIgnoreCase);

        _logger?.LogDebug(
            "{Tag}: Batch [{Current}/{Total}] — {Count} item(s).",
            Tag, batchIndex + 1, totalBatches, paths.Count);

        var (query, aliasToPath) = GraphQLQueryBuilder.BuildBatchQuery(paths);

        if (string.IsNullOrEmpty(query))
        {
            foreach (var path in paths) result[path] = null;
            return result;
        }

        var transport = _transportFactory.Create();
        var request = new GraphQLTransportRequest { Query = query };

        // The batch query response data is a flat dictionary of alias → raw JSON element.
        var data = await transport
            .ExecuteAsync<Dictionary<string, JsonElement>>(request, ct)
            .ConfigureAwait(false);

        if (data == null)
        {
            foreach (var path in paths) result[path] = null;
            return result;
        }

        foreach (var (alias, path) in aliasToPath)
        {
            if (data.TryGetValue(alias, out var element) &&
                element.ValueKind != JsonValueKind.Null)
            {
                var itemData = JsonSerializer.Deserialize<GraphQLItemData>(
                    element.GetRawText(), _jsonOptions);

                result[path] = ItemConverter.ToItem(itemData);
            }
            else
            {
                result[path] = null;
            }
        }

        return result;
    }

    private void LogItemResult(IItem? item, string key)
    {
        if (item == null)
            _logger?.LogDebug("{Tag}: Item not found — [{Key}]", Tag, key);
        else
            _logger?.LogInformation("{Tag}: Item found — [{Key}]", Tag, key);
    }

    private static List<List<T>> ChunkList<T>(List<T> source, int chunkSize)
    {
        var chunks = new List<List<T>>();
        for (var i = 0; i < source.Count; i += chunkSize)
        {
            chunks.Add(source.GetRange(i, Math.Min(chunkSize, source.Count - i)));
        }
        return chunks;
    }
}
