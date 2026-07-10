using Axlis.Caching;
using Axlis.Results;
using Axlis.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Axlis;

/// <summary>
/// Default <see cref="ISitecoreFacade"/> implementation.
/// Provides two API flavors:
/// <list type="bullet">
///   <item><description><b>Clean</b> — <c>Get*Async&lt;T&gt;</c> returns <c>T?</c>; errors are logged internally.</description></item>
///   <item><description><b>Rich</b> — <c>Get*WithResultAsync&lt;T&gt;</c> returns <see cref="AxlisResult{T}"/> with item, metadata, and diagnostics.</description></item>
/// </list>
/// Caching is handled by <see cref="SitecoreItemCacheManager"/>; the underlying data access
/// is delegated to <see cref="ISitecoreService"/>.
/// </summary>
public sealed class SitecoreFacade : ISitecoreFacade
{
    private const string Tag = "axlis.facade";

    private readonly ISitecoreService _sitecoreService;
    private readonly SitecoreItemCacheManager _cacheManager;
    private readonly IAxlisDiagnosticsSink? _diagnosticsSink;
    private readonly bool _diagnosticsEnabled;
    private readonly ILogger<SitecoreFacade>? _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="SitecoreFacade"/>.
    /// </summary>
    public SitecoreFacade(
        ISitecoreService sitecoreService,
        SitecoreItemCacheManager cacheManager,
        IOptions<AxlisOptions> options,
        IAxlisDiagnosticsSink? diagnosticsSink = null,
        ILogger<SitecoreFacade>? logger = null)
    {
        _sitecoreService = sitecoreService;
        _cacheManager = cacheManager;
        _diagnosticsSink = diagnosticsSink;
        _diagnosticsEnabled = options.Value.EnableDiagnostics;
        _logger = logger;
    }

    // ── Clean API ─────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<T?> GetItemByPathAsync<T>(string path, CancellationToken ct = default)
        where T : class, IBaseItem
    {
        _logger?.LogDebug("{Tag}: GetItemByPath<{Type}> [{Path}]", Tag, typeof(T).Name, path);

        try
        {
            var item = await _cacheManager.GetOrCreateAsync(
                path,
                () => _sitecoreService.GetItemByPathAsync(path, ct),
                ct).ConfigureAwait(false);

            return MapToTyped<T>(item);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "{Tag}: GetItemByPath<{Type}> [{Path}] failed", Tag, typeof(T).Name, path);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<T?> GetItemByIdAsync<T>(string id, CancellationToken ct = default)
        where T : class, IBaseItem
    {
        _logger?.LogDebug("{Tag}: GetItemById<{Type}> [{Id}]", Tag, typeof(T).Name, id);

        try
        {
            var item = await _cacheManager.GetOrCreateAsync(
                id,
                () => _sitecoreService.GetItemByIdAsync(id, ct),
                ct).ConfigureAwait(false);

            return MapToTyped<T>(item);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "{Tag}: GetItemById<{Type}> [{Id}] failed", Tag, typeof(T).Name, id);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T?>> GetItemsByPathsAsync<T>(
        IEnumerable<string> paths,
        CancellationToken ct = default)
        where T : class, IBaseItem
    {
        var pathList = paths?.ToList() ?? new List<string>();

        _logger?.LogDebug(
            "{Tag}: GetItemsByPaths<{Type}> — {Count} path(s)",
            Tag, typeof(T).Name, pathList.Count);

        if (pathList.Count == 0)
        {
            return Enumerable.Empty<T?>();
        }

        try
        {
            var batchResult = await _sitecoreService
                .GetItemsByPathsAsync(pathList, ct)
                .ConfigureAwait(false);

            var mapped = new List<T?>(pathList.Count);
            foreach (var path in pathList)
            {
                var item = batchResult.TryGetValue(path, out var raw) ? raw : null;

                if (item != null)
                {
                    await _cacheManager.GetOrCreateAsync(path, () => Task.FromResult<IItem?>(item), ct)
                        .ConfigureAwait(false);
                }

                mapped.Add(MapToTyped<T>(item));
            }

            return mapped;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "{Tag}: GetItemsByPaths<{Type}> failed", Tag, typeof(T).Name);
            return Enumerable.Empty<T?>();
        }
    }

    /// <inheritdoc/>
    public async Task<T?> GetItemFlatAsync<T>(string path, CancellationToken ct = default)
        where T : class, IBaseItem
    {
        _logger?.LogDebug("{Tag}: GetItemFlat<{Type}> [{Path}]", Tag, typeof(T).Name, path);

        try
        {
            var item = await _cacheManager.GetOrCreateAsync(
                path,
                () => _sitecoreService.GetItemFlatAsync(path, ct),
                ct).ConfigureAwait(false);

            return MapToTyped<T>(item);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "{Tag}: GetItemFlat<{Type}> [{Path}] failed", Tag, typeof(T).Name, path);
            return null;
        }
    }

    // ── Rich (WithResult) API ─────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<AxlisResult<T>> GetItemByPathWithResultAsync<T>(
        string path,
        CancellationToken ct = default)
        where T : class, IBaseItem
    {
        var diagnostics = new AxlisDiagnostics();

        try
        {
            var item = await _cacheManager.GetOrCreateAsync(
                path,
                () => _sitecoreService.GetItemByPathAsync(path, ct),
                ct).ConfigureAwait(false);

            return BuildResult<T>(item, path, diagnostics);
        }
        catch (Exception ex)
        {
            RecordError(diagnostics, ex, path);
            return AxlisResult<T>.Empty;
        }
    }

    /// <inheritdoc/>
    public async Task<AxlisResult<T>> GetItemByIdWithResultAsync<T>(
        string id,
        CancellationToken ct = default)
        where T : class, IBaseItem
    {
        var diagnostics = new AxlisDiagnostics();

        try
        {
            var item = await _cacheManager.GetOrCreateAsync(
                id,
                () => _sitecoreService.GetItemByIdAsync(id, ct),
                ct).ConfigureAwait(false);

            return BuildResult<T>(item, id, diagnostics);
        }
        catch (Exception ex)
        {
            RecordError(diagnostics, ex, id);
            return AxlisResult<T>.Empty;
        }
    }

    /// <inheritdoc/>
    public async Task<AxlisResult<IEnumerable<T?>>> GetItemsByPathsWithResultAsync<T>(
        IEnumerable<string> paths,
        CancellationToken ct = default)
        where T : class, IBaseItem
    {
        var pathList = paths?.ToList() ?? new List<string>();
        var diagnostics = new AxlisDiagnostics();

        try
        {
            var batchResult = await _sitecoreService
                .GetItemsByPathsAsync(pathList, ct)
                .ConfigureAwait(false);

            var mapped = new List<T?>(pathList.Count);
            foreach (var path in pathList)
            {
                var item = batchResult.TryGetValue(path, out var raw) ? raw : null;
                if (item == null)
                {
                    AddDiagnostic(diagnostics, $"Item not found: {path}", DiagnosticSeverity.Warning);
                }
                mapped.Add(MapToTyped<T>(item));
            }

            return new AxlisResult<IEnumerable<T?>>
            {
                Value = mapped,
                Diagnostics = diagnostics
            };
        }
        catch (Exception ex)
        {
            RecordError(diagnostics, ex, string.Join(", ", pathList));
            return AxlisResult<IEnumerable<T?>>.Empty;
        }
    }

    /// <inheritdoc/>
    public async Task<AxlisResult<T>> GetItemFlatWithResultAsync<T>(
        string path,
        CancellationToken ct = default)
        where T : class, IBaseItem
    {
        var diagnostics = new AxlisDiagnostics();

        try
        {
            var item = await _cacheManager.GetOrCreateAsync(
                path,
                () => _sitecoreService.GetItemFlatAsync(path, ct),
                ct).ConfigureAwait(false);

            return BuildResult<T>(item, path, diagnostics);
        }
        catch (Exception ex)
        {
            RecordError(diagnostics, ex, path);
            return AxlisResult<T>.Empty;
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new strongly-typed wrapper of type <typeparamref name="T"/> around a raw <see cref="IItem"/>.
    /// Returns <c>null</c> when <paramref name="item"/> is null.
    /// </summary>
    private static T? MapToTyped<T>(IItem? item) where T : class, IBaseItem
    {
        if (item == null)
        {
            return null;
        }
        var wrapper = Activator.CreateInstance<T>();
        wrapper.SetInnerItem(item);
        return wrapper;
    }

    private AxlisResult<T> BuildResult<T>(IItem? item, string key, AxlisDiagnostics diagnostics)
        where T : class, IBaseItem
    {
        var mapped = MapToTyped<T>(item);

        if (mapped == null)
        {
            AddDiagnostic(diagnostics, $"Item not found: {key}", DiagnosticSeverity.Warning);
        }

        var metadata = item == null ? null : new SitecoreMetadata
        {
            ItemId = item.Id,
            ItemPath = item.Path,
            ItemVersion = item.Version,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        return new AxlisResult<T>
        {
            Value = mapped,
            Metadata = metadata,
            Diagnostics = diagnostics
        };
    }

    private void RecordError(AxlisDiagnostics diagnostics, Exception ex, string key)
    {
        _logger?.LogError(ex, "{Tag}: Operation failed for key [{Key}]", Tag, key);
        AddDiagnostic(diagnostics, $"Operation failed for [{key}]: {ex.Message}", DiagnosticSeverity.Error);
    }

    private void AddDiagnostic(AxlisDiagnostics diagnostics, string message, DiagnosticSeverity severity)
    {
        if (!_diagnosticsEnabled)
        {
            return;
        }
        diagnostics.Add(message, severity);
        _diagnosticsSink?.OnEvent(new AxlisDiagnosticEvent
        {
            Message = message,
            Severity = severity,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });
    }
}
