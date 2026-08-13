using Axlis.Sitecore.Abstractions;

namespace Axlis.Sitecore;

/// <summary>
/// Thread-safe, per-request replacement for the ambient <c>Sitecore.Context</c> statics and
/// <c>HttpContext.Current</c>.
/// </summary>
/// <remarks>
/// <para>
/// <c>Sitecore.Context.Database</c> and <c>HttpContext.Current</c> are not reliable in
/// multi-threaded scenarios — they can return <see langword="null"/> mid-request once code hops
/// off the physical thread ASP.NET started the request on (e.g. across an <c>await</c>). This
/// class captures <see cref="Database"/>, <see cref="Request"/>, and <see cref="HttpContext"/>
/// once per request — via <see cref="Hosting.SitecoreContextHttpModule"/> — and serves them back
/// from a per-logical-call store (<see cref="AmbientContextStore{T}"/>) that correctly follows
/// the logical thread of execution instead of a single physical thread.
/// </para>
/// <para>
/// This type intentionally mirrors <c>Sitecore.Context</c>'s own shape
/// (<c>Axlis.Sitecore.Context.Database</c> next to <c>Sitecore.Context.Database</c>) so that
/// adopting it is close to a find-and-replace. Because of that, this file deliberately references
/// <c>Sitecore.Context</c> and <c>System.Web.HttpContext</c> by their fully-qualified
/// (<c>global::</c>) names throughout — a bare <c>HttpContext</c> or <c>Context</c> inside this
/// class would otherwise resolve to this class's own members, not the BCL/Sitecore types.
/// </para>
/// <para>
/// Requires <see cref="Hosting.SitecoreContextHttpModule"/> to be registered in your
/// application's <c>Web.config</c> — see the package README for the exact include. Without it,
/// every property below still works, but only via the "no captured context" fallback path.
/// </para>
/// </remarks>
public static class Context
{
    private const string SlotName = "Axlis.Sitecore.Context";

    private static readonly AmbientContextStore<CapturedContext> Store =
        new AmbientContextStore<CapturedContext>(SlotName);

    /// <summary>
    /// The current request's Sitecore content database, or <see langword="null"/> if
    /// unavailable.
    /// </summary>
    /// <remarks>
    /// Resolution order: (1) the value captured for this request by
    /// <see cref="Hosting.SitecoreContextHttpModule"/>; (2) a direct, defensive call to
    /// <c>global::Sitecore.Context.Database</c> as a fallback for code running outside any
    /// captured request (background threads, scheduled tasks, application start-up); (3)
    /// <see langword="null"/> if even that fallback is unavailable. No exception is ever thrown
    /// for "no context available" — callers keep the same null-check discipline they already
    /// have today.
    /// </remarks>
    public static global::Sitecore.Data.Database? Database =>
        Store.Current?.Database ?? SafeFallbackDatabase();

    /// <summary>
    /// The current request's <see cref="global::System.Web.HttpRequest"/>, or
    /// <see langword="null"/> if unavailable.
    /// </summary>
    /// <remarks>Same resolution order as <see cref="Database"/>, falling back to
    /// <c>global::System.Web.HttpContext.Current.Request</c>.</remarks>
    public static global::System.Web.HttpRequest? Request =>
        Store.Current?.Request ?? SafeFallbackRequest();

    /// <summary>
    /// The current request's <see cref="global::System.Web.HttpContext"/> — the thread-safe
    /// replacement for <c>HttpContext.Current</c> — or <see langword="null"/> if unavailable.
    /// </summary>
    /// <remarks>Same resolution order as <see cref="Database"/>, falling back to
    /// <c>global::System.Web.HttpContext.Current</c>.</remarks>
    public static global::System.Web.HttpContext? HttpContext =>
        Store.Current?.HttpContext ?? SafeFallbackHttpContext();

    /// <summary>
    /// Captures the current request's context. Called once per request by
    /// <see cref="Hosting.SitecoreContextHttpModule"/> on <c>BeginRequest</c> — not intended to
    /// be called directly by application code.
    /// </summary>
    internal static void Capture(
        global::Sitecore.Data.Database? database,
        global::System.Web.HttpRequest? request,
        global::System.Web.HttpContext? httpContext)
    {
        Store.Set(new CapturedContext(database, request, httpContext));
    }

    /// <summary>
    /// Releases the current request's captured context. Called once per request by
    /// <see cref="Hosting.SitecoreContextHttpModule"/> on <c>EndRequest</c>, so that no state can
    /// leak into whatever the underlying physical thread picks up next once it returns to the
    /// pool.
    /// </summary>
    internal static void Release()
    {
        Store.Clear();
    }

    private static global::Sitecore.Data.Database? SafeFallbackDatabase()
    {
        try
        {
            return global::Sitecore.Context.Database;
        }
        catch
        {
            return null;
        }
    }

    private static global::System.Web.HttpRequest? SafeFallbackRequest()
    {
        try
        {
            return global::System.Web.HttpContext.Current?.Request;
        }
        catch
        {
            return null;
        }
    }

    private static global::System.Web.HttpContext? SafeFallbackHttpContext()
    {
        try
        {
            return global::System.Web.HttpContext.Current;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// The immutable snapshot captured per request. Kept internal — consumers read
    /// <see cref="Database"/> / <see cref="Request"/> / <see cref="HttpContext"/> directly.
    /// </summary>
    private sealed class CapturedContext
    {
        public CapturedContext(
            global::Sitecore.Data.Database? database,
            global::System.Web.HttpRequest? request,
            global::System.Web.HttpContext? httpContext)
        {
            Database = database;
            Request = request;
            HttpContext = httpContext;
        }

        public global::Sitecore.Data.Database? Database { get; }

        public global::System.Web.HttpRequest? Request { get; }

        public global::System.Web.HttpContext? HttpContext { get; }
    }
}
