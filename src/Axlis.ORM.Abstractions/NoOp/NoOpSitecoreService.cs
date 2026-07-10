using Axlis.ORM.Services;

namespace Axlis.ORM.NoOp;

/// <summary>
/// A safe-off <see cref="ISitecoreService"/> that returns <c>null</c> or empty results for all calls.
/// Useful for testing, feature flags, or environments without a Sitecore endpoint.
/// </summary>
public sealed class NoOpSitecoreService : ISitecoreService
{
    /// <inheritdoc/>
    public Task<IItem?> GetItemByPathAsync(string path, CancellationToken ct = default)
        => Task.FromResult<IItem?>(null);

    /// <inheritdoc/>
    public Task<IItem?> GetItemByIdAsync(string id, CancellationToken ct = default)
        => Task.FromResult<IItem?>(null);

    /// <inheritdoc/>
    public Task<IDictionary<string, IItem?>> GetItemsByPathsAsync(IEnumerable<string> paths, CancellationToken ct = default)
        => Task.FromResult<IDictionary<string, IItem?>>(new Dictionary<string, IItem?>());

    /// <inheritdoc/>
    public Task<IItem?> GetItemFlatAsync(string path, CancellationToken ct = default)
        => Task.FromResult<IItem?>(null);
}
