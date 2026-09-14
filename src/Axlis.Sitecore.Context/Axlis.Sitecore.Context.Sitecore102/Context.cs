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
/// class captures <see cref="Database"/>, <see cref="Request"/>, <see cref="HttpContext"/>,
/// <see cref="ContentDatabase"/>, <see cref="Site"/>, <see cref="Language"/>, and
/// <see cref="User"/> once per request — via <see cref="Hosting.SitecoreContextHttpModule"/> —
/// and serves them back from a per-logical-call store (<see cref="AmbientContextStore{T}"/>)
/// that correctly follows the logical thread of execution instead of a single physical thread.
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
/// <para>
/// <b>Snapshot semantics for <see cref="Site"/>, <see cref="Language"/>, and <see cref="User"/>.</b>
/// All seven properties on this class are captured exactly once, at the very start of the
/// request (<c>BeginRequest</c>), and every subsequent read for the rest of that request returns
/// that same captured snapshot — this is what makes them thread-safe. <see cref="Database"/>,
/// <see cref="Request"/>, and <see cref="HttpContext"/> are effectively fixed for a request's
/// lifetime already, so this is not a practical trade-off for them. <see cref="Site"/>,
/// <see cref="Language"/>, and especially <see cref="User"/> are different: application code
/// commonly changes the real <c>Sitecore.Context.Language</c> (rendering a specific language
/// variant), <c>Sitecore.Context.Site</c> (a deliberate site switch), or
/// <c>Sitecore.Context.User</c> (a login or impersonation) partway through a request. If your
/// code does that and then needs to observe the new value later in the *same* request, read the
/// raw <c>Sitecore.Context.Site</c> / <c>.Language</c> / <c>.User</c> directly at that call
/// site — the corresponding property here will keep returning the value captured at
/// <c>BeginRequest</c> for the rest of the request, by design, for consistency with
/// <see cref="Database"/>/<see cref="Request"/>/<see cref="HttpContext"/>.
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
    /// The current request's Sitecore content-editing database, or <see langword="null"/> if
    /// unavailable.
    /// </summary>
    /// <remarks>
    /// Same resolution order as <see cref="Database"/>, falling back to
    /// <c>global::Sitecore.Context.ContentDatabase</c>. See the type-level remarks on this class
    /// for the snapshot semantics shared with <see cref="Site"/>/<see cref="Language"/>/
    /// <see cref="User"/> — though in practice <c>ContentDatabase</c> changes mid-request no more
    /// often than <see cref="Database"/> does.
    /// </remarks>
    public static global::Sitecore.Data.Database? ContentDatabase =>
        Store.Current?.ContentDatabase ?? SafeFallbackContentDatabase();

    /// <summary>
    /// The current request's <see cref="global::Sitecore.Sites.SiteContext"/>, or
    /// <see langword="null"/> if unavailable.
    /// </summary>
    /// <remarks>
    /// Same resolution order as <see cref="Database"/>, falling back to
    /// <c>global::Sitecore.Context.Site</c>. <b>Captured once at <c>BeginRequest</c></b> — see
    /// the type-level remarks on this class. If application code deliberately switches the site
    /// later in the request (e.g. via a site-context switcher), this property will keep returning
    /// the site captured at the start of the request; read <c>global::Sitecore.Context.Site</c>
    /// directly if you need to observe that change.
    /// </remarks>
    public static global::Sitecore.Sites.SiteContext? Site =>
        Store.Current?.Site ?? SafeFallbackSite();

    /// <summary>
    /// The current request's <see cref="global::Sitecore.Globalization.Language"/>, or
    /// <see langword="null"/> if unavailable.
    /// </summary>
    /// <remarks>
    /// Same resolution order as <see cref="Database"/>, falling back to
    /// <c>global::Sitecore.Context.Language</c>. <b>Captured once at <c>BeginRequest</c></b> —
    /// see the type-level remarks on this class. If application code deliberately changes the
    /// language later in the request (e.g. to render a specific language variant), this property
    /// will keep returning the language captured at the start of the request; read
    /// <c>global::Sitecore.Context.Language</c> directly if you need to observe that change.
    /// </remarks>
    public static global::Sitecore.Globalization.Language? Language =>
        Store.Current?.Language ?? SafeFallbackLanguage();

    /// <summary>
    /// The current request's <see cref="global::Sitecore.Security.Accounts.User"/>, or
    /// <see langword="null"/> if unavailable.
    /// </summary>
    /// <remarks>
    /// Same resolution order as <see cref="Database"/>, falling back to
    /// <c>global::Sitecore.Context.User</c>. <b>Captured once at <c>BeginRequest</c></b> — see
    /// the type-level remarks on this class. A login or impersonation performed by application
    /// code later in the request will not be reflected here for the rest of that request; read
    /// <c>global::Sitecore.Context.User</c> directly at the point you need the post-login/
    /// post-impersonation identity.
    /// </remarks>
    public static global::Sitecore.Security.Accounts.User? User =>
        Store.Current?.User ?? SafeFallbackUser();

    /// <summary>
    /// Captures the current request's context. Called once per request by
    /// <see cref="Hosting.SitecoreContextHttpModule"/> on <c>BeginRequest</c> — not intended to
    /// be called directly by application code.
    /// </summary>
    internal static void Capture(
        global::Sitecore.Data.Database? database,
        global::System.Web.HttpRequest? request,
        global::System.Web.HttpContext? httpContext,
        global::Sitecore.Data.Database? contentDatabase,
        global::Sitecore.Sites.SiteContext? site,
        global::Sitecore.Globalization.Language? language,
        global::Sitecore.Security.Accounts.User? user)
    {
        Store.Set(new CapturedContext(
            database, request, httpContext, contentDatabase, site, language, user));
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

    private static global::Sitecore.Data.Database? SafeFallbackContentDatabase()
    {
        try
        {
            return global::Sitecore.Context.ContentDatabase;
        }
        catch
        {
            return null;
        }
    }

    private static global::Sitecore.Sites.SiteContext? SafeFallbackSite()
    {
        try
        {
            return global::Sitecore.Context.Site;
        }
        catch
        {
            return null;
        }
    }

    private static global::Sitecore.Globalization.Language? SafeFallbackLanguage()
    {
        try
        {
            return global::Sitecore.Context.Language;
        }
        catch
        {
            return null;
        }
    }

    private static global::Sitecore.Security.Accounts.User? SafeFallbackUser()
    {
        try
        {
            return global::Sitecore.Context.User;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// The immutable snapshot captured per request. Kept internal — consumers read
    /// <see cref="Database"/> / <see cref="Request"/> / <see cref="HttpContext"/> /
    /// <see cref="ContentDatabase"/> / <see cref="Site"/> / <see cref="Language"/> /
    /// <see cref="User"/> directly.
    /// </summary>
    private sealed class CapturedContext
    {
        public CapturedContext(
            global::Sitecore.Data.Database? database,
            global::System.Web.HttpRequest? request,
            global::System.Web.HttpContext? httpContext,
            global::Sitecore.Data.Database? contentDatabase,
            global::Sitecore.Sites.SiteContext? site,
            global::Sitecore.Globalization.Language? language,
            global::Sitecore.Security.Accounts.User? user)
        {
            Database = database;
            Request = request;
            HttpContext = httpContext;
            ContentDatabase = contentDatabase;
            Site = site;
            Language = language;
            User = user;
        }

        public global::Sitecore.Data.Database? Database { get; }

        public global::System.Web.HttpRequest? Request { get; }

        public global::System.Web.HttpContext? HttpContext { get; }

        public global::Sitecore.Data.Database? ContentDatabase { get; }

        public global::Sitecore.Sites.SiteContext? Site { get; }

        public global::Sitecore.Globalization.Language? Language { get; }

        public global::Sitecore.Security.Accounts.User? User { get; }
    }
}
