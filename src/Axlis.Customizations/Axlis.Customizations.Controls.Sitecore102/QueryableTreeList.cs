using Axlis.Customizations.Abstractions;
using Microsoft.Extensions.Logging;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.DependencyInjection;
using Sitecore.Web.UI.HtmlControls.Data;
using System;
using System.Linq;

namespace Axlis.Customizations.Controls;

/* DevNote
 * Configuration (manual — no Field Type item or config include ships with this package, see README):
 *   Namespace : Axlis.Customizations.Controls
 *   Assembly  : Axlis.Customizations.Controls.Sitecore102
 *   Class     : Axlis.Customizations.Controls.QueryableTreeList
 *   Control   : Axlis:QueryableTreeList
 *
 * Ported from a prior client engagement's internal "QueryableTreeList" TreeList customization.
 * Behavior is a faithful 1:1 port (namespace/branding removed per Axlis's de-branding policy;
 * see the Axlis.Customizations Phase 1/2 plans for the full decision record), with one
 * intentional fix: the original computed an "extraParams" local that was never used anywhere
 * in the method — dead code removed here since it would otherwise trip this repo's
 * zero-warnings build gate (CS0219).
 */

/// <summary>
/// Extends the standard Sitecore TreeList field to support query-based data sources and template filtering.
/// Allows dynamic resolution of the data source using a Sitecore query and supports additional parameters for display and selection templates.
/// </summary>
public class QueryableTreeList : Sitecore.Shell.Applications.ContentEditor.TreeList
{
    private static readonly char[] separator = new[] { '&' };

    /// <summary>
    /// Gets or sets the source string for the TreeList, supporting additional parameters for template filtering.
    /// </summary>
    public new string Source
    {
        get => base.Source;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                base.Source = null;
                return;
            }

            // Extract parameters
            var dataSource = StringUtil.ExtractParameter("DataSource", value);
            var includeTemplatesForDisplay = StringUtil.ExtractParameter("IncludeTemplatesForDisplay", value);
            var includeTemplatesForSelection = StringUtil.ExtractParameter("IncludeTemplatesForSelection", value);

            // Build the base query (e.g., from DataSource)
            var finalSource = dataSource;

            // Append IncludeTemplatesForDisplay if present
            if (!string.IsNullOrEmpty(includeTemplatesForDisplay))
            {
                finalSource += $"&IncludeTemplatesForDisplay={includeTemplatesForDisplay}";
                IncludeTemplatesForDisplay = includeTemplatesForDisplay;
            }

            // Append IncludeTemplatesForSelection if present
            if (!string.IsNullOrEmpty(includeTemplatesForSelection))
            {
                finalSource += $"&IncludeTemplatesForSelection={includeTemplatesForSelection}";
                IncludeTemplatesForSelection = includeTemplatesForSelection;
            }

            base.Source = finalSource;
        }
    }

    /// <summary>
    /// Gets the resolved data source for the TreeList, executing queries and normalizing template IDs as needed.
    /// </summary>
    public override string DataSource
    {
        get
        {
            var rawSource = this.Source;

            if (string.IsNullOrEmpty(rawSource))
            {
                return string.Empty;
            }

            if (rawSource.StartsWith(AxlisControlConstants.QueryPrefix, StringComparison.OrdinalIgnoreCase))
            {
                if (Sitecore.Context.ContentDatabase == null || base.ItemID == null)
                {
                    return string.Empty;
                }

                var current = Sitecore.Context.ContentDatabase.GetItem(base.ItemID);
                if (current == null)
                {
                    return string.Empty;
                }

                // Split into query part + extra params
                var parts = rawSource.Split(separator, 2);
                var queryPart = parts[0];

                // Normalize Template IDs inside query to uppercase
                queryPart = System.Text.RegularExpressions.Regex.Replace(
                    queryPart,
                    @"\{[0-9a-fA-F\-]{36}\}",
                    m => m.Value.ToUpperInvariant()
                );

                Item obj = null;

                try
                {
                    obj = LookupSources
                        .GetItems(current, queryPart)
                        .FirstOrDefault();
                }
                catch (Exception ex)
                {
                    var logger = ServiceLocator.ServiceProvider.GetService(typeof(ILogger<QueryableTreeList>))
                        as ILogger<QueryableTreeList>;
                    logger?.LogError("QueryableTreeList field failed to execute query. Exception: {Exception}", ex);
                }

                if (obj == null)
                {
                    return string.Empty;
                }

                // Return resolved path
                var itemFullPath = obj.Paths.FullPath;
                return itemFullPath;
            }

            var includeTemplatesForDisplay = StringUtil.ExtractParameter("IncludeTemplatesForDisplay", rawSource);
            var includeTemplatesForSelection = StringUtil.ExtractParameter("IncludeTemplatesForSelection", rawSource);

            var result = rawSource
                .Replace($"&IncludeTemplatesForDisplay={includeTemplatesForDisplay}", string.Empty)
                .Replace($"&IncludeTemplatesForSelection={includeTemplatesForSelection}", string.Empty);

            return result;
        }
    }

    /// <summary>
    /// Overrides the header value generation to add validation warnings for items not in the selection list.
    /// This method is called by the base TreeList when rendering selected items.
    /// </summary>
    /// <param name="item">The item being rendered.</param>
    /// <returns>Header text for the list item, with warning if item is not valid.</returns>
    protected override string GetHeaderValue(Item item)
    {
        if (item == null)
        {
            return base.GetHeaderValue(item);
        }

        var headerValue = base.GetHeaderValue(item);

        // Check if this item is valid according to the query
        if (!IsItemInSelectionList(item))
        {
            headerValue += " [Not in selection list]";
        }

        return headerValue;
    }

    /// <summary>
    /// Checks if the given item is valid according to the data source query.
    /// </summary>
    /// <param name="item">The item to validate.</param>
    /// <returns>True if the item is in the selection list, false otherwise.</returns>
    private bool IsItemInSelectionList(Item item)
    {
        try
        {
            if (item == null)
            {
                return false;
            }

            var dataSourcePath = this.DataSource;
            if (string.IsNullOrEmpty(dataSourcePath))
            {
                return true; // If no data source, all items are valid
            }

            var rootItem = Sitecore.Context.ContentDatabase?.GetItem(dataSourcePath);
            if (rootItem == null)
            {
                return true; // If root not found, don't show warnings
            }

            // Check if item is the root or a descendant of the root
            if (item.ID == rootItem.ID)
            {
                return true;
            }

            // Check if item is under the root path
            return item.Paths.FullPath.StartsWith(rootItem.Paths.FullPath, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            var logger = ServiceLocator.ServiceProvider.GetService(typeof(ILogger<QueryableTreeList>))
                as ILogger<QueryableTreeList>;
            logger?.LogError("QueryableTreeList validation failed for item {ItemId}. Exception: {Exception}", item?.ID, ex);
            return true; // On error, don't show warning
        }
    }
}
