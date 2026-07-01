using Axlis.Results;
using Axlis.Services;

namespace Axlis.NoOp;

/// <summary>
/// A safe-off <see cref="ISitecoreFacade"/> that returns <c>null</c> or empty results for all calls.
/// Useful for testing, feature flags, or environments without a Sitecore endpoint.
/// </summary>
public sealed class NoOpSitecoreFacade : ISitecoreFacade
{
    /// <inheritdoc/>
    public Task<T?> GetItemByPathAsync<T>(string path, CancellationToken ct = default)
        where T : class, IBaseItem
        => Task.FromResult<T?>(null);

    /// <inheritdoc/>
    public Task<T?> GetItemByIdAsync<T>(string id, CancellationToken ct = default)
        where T : class, IBaseItem
        => Task.FromResult<T?>(null);

    /// <inheritdoc/>
    public Task<IEnumerable<T?>> GetItemsByPathsAsync<T>(IEnumerable<string> paths, CancellationToken ct = default)
        where T : class, IBaseItem
        => Task.FromResult<IEnumerable<T?>>(Array.Empty<T?>());

    /// <inheritdoc/>
    public Task<T?> GetItemFlatAsync<T>(string path, CancellationToken ct = default)
        where T : class, IBaseItem
        => Task.FromResult<T?>(null);

    /// <inheritdoc/>
    public Task<AxlisResult<T>> GetItemByPathWithResultAsync<T>(string path, CancellationToken ct = default)
        where T : class, IBaseItem
        => Task.FromResult(AxlisResult<T>.Empty);

    /// <inheritdoc/>
    public Task<AxlisResult<T>> GetItemByIdWithResultAsync<T>(string id, CancellationToken ct = default)
        where T : class, IBaseItem
        => Task.FromResult(AxlisResult<T>.Empty);

    /// <inheritdoc/>
    public Task<AxlisResult<IEnumerable<T?>>> GetItemsByPathsWithResultAsync<T>(IEnumerable<string> paths, CancellationToken ct = default)
        where T : class, IBaseItem
        => Task.FromResult(AxlisResult<IEnumerable<T?>>.Empty);

    /// <inheritdoc/>
    public Task<AxlisResult<T>> GetItemFlatWithResultAsync<T>(string path, CancellationToken ct = default)
        where T : class, IBaseItem
        => Task.FromResult(AxlisResult<T>.Empty);
}
