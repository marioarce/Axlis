using System.Web;

namespace Axlis.Sitecore.Hosting;

/// <summary>
/// Captures <see cref="global::Sitecore.Context.Database"/>, the current
/// <see cref="global::System.Web.HttpRequest"/>, the current
/// <see cref="global::System.Web.HttpContext"/>, <see cref="global::Sitecore.Context.ContentDatabase"/>,
/// <see cref="global::Sitecore.Context.Site"/>, <see cref="global::Sitecore.Context.Language"/>,
/// and <see cref="global::Sitecore.Context.User"/> once per request, so that
/// <see cref="Axlis.Sitecore.Context"/> can serve them back thread-safely for the rest of the
/// request's lifetime.
/// </summary>
/// <remarks>
/// <para>
/// Not wired up automatically — register it in your application's <c>Web.config</c>. See the
/// package README for the exact include, and note the ordering caveat: this module must run
/// after Sitecore's own request-initialization modules, so that <c>Sitecore.Context.Database</c>
/// (and the other captured values) are already populated by the time <see cref="Init"/>'s
/// <c>BeginRequest</c> handler runs.
/// </para>
/// <para>
/// All seven values are captured together, once, at <c>BeginRequest</c> — including
/// <c>Site</c>/<c>Language</c>/<c>User</c>, which application code can legitimately change again
/// later in the same request (a site switch, a language override, a login/impersonation). See
/// the snapshot-semantics remarks on <see cref="Axlis.Sitecore.Context"/> for why that's a
/// deliberate trade-off, not a bug.
/// </para>
/// <para>
/// Deliberately minimal: this module's only job is to capture and release context. Unlike the
/// prior reference implementation this mechanism is ported from, it does not carry any
/// application-specific concerns (monitoring, feature toggles, header manipulation, etc.) — those
/// belong in the consuming application's own modules, not in this package.
/// </para>
/// </remarks>
public sealed class SitecoreContextHttpModule : IHttpModule
{
    /// <inheritdoc />
    public void Init(HttpApplication context)
    {
        context.BeginRequest += OnBeginRequest;
        context.EndRequest += OnEndRequest;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // No unmanaged resources or event subscriptions to release beyond what ASP.NET already
        // tears down with the HttpApplication instance itself.
    }

    private static void OnBeginRequest(object sender, System.EventArgs e)
    {
        var httpContext = HttpContext.Current;
        var request = httpContext?.Request;
        var database = global::Sitecore.Context.Database;
        var contentDatabase = global::Sitecore.Context.ContentDatabase;
        var site = global::Sitecore.Context.Site;
        var language = global::Sitecore.Context.Language;
        var user = global::Sitecore.Context.User;

        Axlis.Sitecore.Context.Capture(
            database, request, httpContext, contentDatabase, site, language, user);
    }

    private static void OnEndRequest(object sender, System.EventArgs e)
    {
        Axlis.Sitecore.Context.Release();
    }
}
