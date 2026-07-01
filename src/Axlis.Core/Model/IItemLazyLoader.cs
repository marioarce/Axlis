namespace Axlis.Core;

/// <summary>
/// Abstraction for on-demand item fetching, used by <see cref="ExtendedItem"/>
/// and <see cref="AxesAdapter"/> to lazy-load missing parent/children data.
/// Replaces the ambient static <c>SitecoreFactory</c> from the original implementation,
/// making the lazy-fetch seam testable via DI.
/// Register an implementation in DI (provided by the <c>Axlis</c> package) and
/// call <see cref="ExtendedItem.Initialize"/> on application startup.
/// </summary>
public interface IItemLazyLoader
{
    /// <summary>
    /// Fetches a fully-loaded item by its ID or path, returning it as a raw <see cref="Item"/>,
    /// or <c>null</c> if not found.
    /// </summary>
    /// <param name="idOrPath">The Sitecore item ID (GUID string) or full path.</param>
    Item? LoadItem(string idOrPath);
}
