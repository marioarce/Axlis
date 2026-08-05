namespace Axlis.Customizations.Abstractions;

/// <summary>
/// Shared literals for the Axlis.Customizations package family: the Sitecore control-source
/// markup prefix and the query-source token convention used across Axlis Sitecore customizations.
/// Kept here (rather than duplicated per customization) so the markup contract and any future
/// tooling/config generation stay in one place as the family grows beyond <c>QueryableTreeList</c>.
/// </summary>
public static class AxlisControlConstants
{
    /// <summary>
    /// The Sitecore <c>controlSources</c> markup prefix registered for Axlis customizations,
    /// e.g. <c>&lt;Axlis:QueryableTreeList runat="server"/&gt;</c>.
    /// </summary>
    public const string ControlPrefix = "Axlis";

    /// <summary>
    /// The source-string prefix that signals a Sitecore query-based data source,
    /// e.g. <c>query:./descendant::*[@@templatename='Example']</c>.
    /// </summary>
    public const string QueryPrefix = "query:";
}
