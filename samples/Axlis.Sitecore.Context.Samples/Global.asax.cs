namespace Axlis.Sitecore.Samples
{
    /// <summary>
    /// Inherits from <see cref="global::Sitecore.Web.Application"/>, matching how a real Sitecore
    /// site's Global.asax is wired — this ensures Sitecore's own pipeline processing hooks in
    /// correctly, which <see cref="global::Axlis.Sitecore.Hosting.SitecoreContextHttpModule"/>
    /// depends on running after.
    /// </summary>
    public class Global : global::Sitecore.Web.Application
    {
    }
}
