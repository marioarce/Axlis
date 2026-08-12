using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Axlis.Sitecore.Samples.Pages
{
    /// <summary>
    /// Demonstrates <c>Axlis.Sitecore.Context.Database</c> / <c>.Request</c> staying correctly
    /// populated across a simulated thread hop (<see cref="Task.Run(Action)"/>), side-by-side with
    /// the raw, non-thread-safe <c>Sitecore.Context.Database</c> / <c>HttpContext.Current</c>,
    /// which do not.
    /// </summary>
    /// <remarks>
    /// This file consistently uses fully-qualified (<c>global::</c>) names for
    /// <c>Sitecore.Context</c>, <c>Axlis.Sitecore.Context</c>, and <c>System.Web.HttpContext</c>.
    /// That is not optional style here: this file's own namespace, <c>Axlis.Sitecore.Samples</c>,
    /// is lexically nested under <c>Axlis.Sitecore</c> — the same namespace <c>Axlis.Sitecore.Context</c>
    /// (our wrapper class) lives in. A bare <c>Sitecore.Context.Database</c> from inside this file
    /// would incorrectly resolve to <em>our own</em> <c>Axlis.Sitecore.Context.Database</c> instead
    /// of the real Sitecore API, because C# simple-name lookup prefers an enclosing namespace
    /// (<c>Axlis.Sitecore</c>, reachable here as bare <c>Sitecore</c>) over the global one. See
    /// <c>Axlis.Sitecore.Context</c>'s own XML doc remarks in
    /// <c>Axlis.Sitecore.Context.Sitecore102</c> for the same caveat at its source.
    /// </remarks>
    public partial class AxlisSitecoreContextDemo : global::System.Web.UI.Page
    {
        /// <summary>
        /// How many simulated background threads to capture context from.
        /// </summary>
        private const int SimulatedThreadCount = 5;

        // Populated by the ASP.NET Web Forms markup-to-code-behind wiring before Page_Load ever
        // runs (the <asp:Literal runat="server"> declarations in the .aspx markup) — guaranteed
        // non-null by the time any method below reads them, hence the null-forgiving initializer
        // rather than a nullable declaration.
        protected global::System.Web.UI.WebControls.Literal BaselineLiteral = null!;
        protected global::System.Web.UI.WebControls.Literal ThreadResultsLiteral = null!;

        protected void Page_Load(object sender, EventArgs e)
        {
            var baseline = Capture("Original request thread");
            BaselineLiteral.Text = WrapTable(RenderRowGroup(baseline));

            var tasks = Enumerable.Range(1, SimulatedThreadCount)
                .Select(i => Task.Run(() => Capture("Simulated thread #" + i)))
                .ToArray();

            Task.WaitAll(tasks);

            var rows = new StringBuilder();
            foreach (var task in tasks)
            {
                rows.Append(RenderRowGroup(task.Result));
            }

            ThreadResultsLiteral.Text = WrapTable(rows.ToString());
        }

        /// <summary>
        /// Reads the raw ambient statics and the Axlis.Sitecore.Context equivalents from whatever
        /// thread this method happens to run on.
        /// </summary>
        private static Snapshot Capture(string label)
        {
            string rawDatabase;
            try
            {
                // Deliberately the raw, non-thread-safe static — see the type-level remarks on why
                // this is fully qualified.
                rawDatabase = global::Sitecore.Context.Database?.Name ?? "NULL";
            }
            catch (Exception ex)
            {
                rawDatabase = "EXCEPTION: " + ex.Message;
            }

            string rawRequestUrl;
            try
            {
                rawRequestUrl = global::System.Web.HttpContext.Current?.Request?.Url?.ToString() ?? "NULL";
            }
            catch (Exception ex)
            {
                rawRequestUrl = "EXCEPTION: " + ex.Message;
            }

            string axlisDatabase;
            try
            {
                axlisDatabase = global::Axlis.Sitecore.Context.Database?.Name ?? "NULL";
            }
            catch (Exception ex)
            {
                axlisDatabase = "EXCEPTION: " + ex.Message;
            }

            string axlisRequestUrl;
            try
            {
                axlisRequestUrl = global::Axlis.Sitecore.Context.Request?.Url?.ToString() ?? "NULL";
            }
            catch (Exception ex)
            {
                axlisRequestUrl = "EXCEPTION: " + ex.Message;
            }

            return new Snapshot(
                label,
                Thread.CurrentThread.ManagedThreadId,
                rawDatabase,
                rawRequestUrl,
                axlisDatabase,
                axlisRequestUrl);
        }

        private static string RenderRowGroup(Snapshot snapshot)
        {
            return
                "<tr><th colspan=\"2\">" + Html(snapshot.Label) + " (managed thread " + snapshot.ManagedThreadId + ")</th></tr>" +
                "<tr><td>Sitecore.Context.Database (raw)</td><td>" + Html(snapshot.RawDatabase) + "</td></tr>" +
                "<tr><td>Axlis.Sitecore.Context.Database</td><td>" + Html(snapshot.AxlisDatabase) + "</td></tr>" +
                "<tr><td>HttpContext.Current.Request.Url (raw)</td><td>" + Html(snapshot.RawRequestUrl) + "</td></tr>" +
                "<tr><td>Axlis.Sitecore.Context.Request.Url</td><td>" + Html(snapshot.AxlisRequestUrl) + "</td></tr>";
        }

        private static string WrapTable(string rowsHtml)
        {
            return "<table border=\"1\" cellpadding=\"4\" cellspacing=\"0\">" + rowsHtml + "</table>";
        }

        private static string Html(string value)
        {
            return global::System.Web.HttpUtility.HtmlEncode(value);
        }

        /// <summary>
        /// An immutable snapshot of what one thread saw. A constructor (rather than an
        /// object-initializer over auto-properties) is used deliberately, so every property is
        /// guaranteed non-null at construction under this repo's Nullable-enabled build — see
        /// <c>Axlis.Sitecore.Context</c>'s private <c>CapturedContext</c> class (in
        /// Axlis.Sitecore.Context.Sitecore102) for the same pattern.
        /// </summary>
        private sealed class Snapshot
        {
            public Snapshot(
                string label,
                int managedThreadId,
                string rawDatabase,
                string rawRequestUrl,
                string axlisDatabase,
                string axlisRequestUrl)
            {
                Label = label;
                ManagedThreadId = managedThreadId;
                RawDatabase = rawDatabase;
                RawRequestUrl = rawRequestUrl;
                AxlisDatabase = axlisDatabase;
                AxlisRequestUrl = axlisRequestUrl;
            }

            public string Label { get; }

            public int ManagedThreadId { get; }

            public string RawDatabase { get; }

            public string RawRequestUrl { get; }

            public string AxlisDatabase { get; }

            public string AxlisRequestUrl { get; }
        }
    }
}
