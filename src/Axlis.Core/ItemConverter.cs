using Axlis.Core.GraphQL;

namespace Axlis.Core;

/// <summary>
/// Converts <see cref="GraphQLItemData"/> trees into <see cref="Item"/> domain objects.
/// Handles circular-reference protection via a <c>processedIds</c> hash set
/// that is threaded through recursive calls.
/// Cache integration is handled externally (by <c>SitecoreService</c> in <c>Axlis.GraphQL</c>).
/// </summary>
public static class ItemConverter
{
    /// <summary>
    /// Converts a top-level <see cref="GraphQLItemData"/> to a domain <see cref="Item"/>.
    /// </summary>
    /// <param name="itemData">The GraphQL item data to convert.</param>
    /// <returns>The converted <see cref="Item"/>, or <c>null</c> if <paramref name="itemData"/> is <c>null</c>.</returns>
    public static Item? ToItem(GraphQLItemData? itemData)
        => ToItem(itemData, processedIds: null, currentDepth: 1);

    /// <summary>
    /// Converts a list of ancestor <see cref="GraphQLItemData"/> to a list of <see cref="Item"/> objects.
    /// </summary>
    public static List<Item>? ToAncestorItems(List<GraphQLItemData>? ancestors)
    {
        if (ancestors == null || ancestors.Count == 0) return null;

        return ancestors
            .Where(a => a != null)
            .Select(a => ToItem(a)!)
            .Where(i => i != null)
            .ToList();
    }

    internal static Item? ToItem(
        GraphQLItemData? itemData,
        HashSet<string>? processedIds,
        int currentDepth)
    {
        if (itemData == null) return null;

        processedIds ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Circular reference guard — return null (caller can substitute from cache if available)
        if (!string.IsNullOrEmpty(itemData.Id) && processedIds.Contains(itemData.Id))
        {
            return null;
        }

        // Template
        ItemTemplate? template = null;
        if (itemData.Template != null)
        {
            template = new ItemTemplate(itemData.Template.Id, itemData.Template.Name);
        }

        // Fields
        var fields = ConvertFields(itemData.Fields, processedIds, currentDepth);

        // Parent (goes up — depth does not increase)
        var parent = ToItem(itemData.Parent, processedIds, currentDepth);

        // Children (go down — depth increases)
        List<Item>? children = null;
        int? childrenTotalCount = null;

        if (itemData.Children?.Results != null)
        {
            children = itemData.Children.Results
                .Where(c => c != null)
                .Select(c => ToItem(c, processedIds, currentDepth + 1)!)
                .Where(c => c != null)
                .ToList();

            childrenTotalCount = itemData.Children.Total;
        }

        var item = new Item(
            itemData.Id,
            itemData.Path ?? string.Empty,
            itemData.Name,
            itemData.DisplayName,
            itemData.Version,
            itemData.HasChildren,
            template,
            parent,
            fields,
            children,
            childrenTotalCount);

        if (!string.IsNullOrEmpty(itemData.Id))
        {
            processedIds.Add(itemData.Id);
        }

        return item;
    }

    private static List<ItemTemplateField>? ConvertFields(
        List<GraphQLFieldData>? fields,
        HashSet<string>? processedIds,
        int currentDepth)
    {
        if (fields == null || fields.Count == 0) return null;

        var result = new List<ItemTemplateField>(fields.Count);
        foreach (var fieldData in fields)
        {
            if (fieldData == null) continue;
            result.Add(ConvertField(fieldData, processedIds, currentDepth));
        }

        return result.Count > 0 ? result : null;
    }

    private static ItemTemplateField ConvertField(
        GraphQLFieldData fieldData,
        HashSet<string>? processedIds,
        int currentDepth)
    {
        var targetItems = fieldData.TargetItems?
            .Where(ti => ti != null)
            .Select(ti => ToItem(ti, processedIds, currentDepth + 1)!)
            .Where(i => i != null)
            .ToList()
            .AsReadOnly();

        var targetItem = fieldData.TargetItem != null
            ? ToItem(fieldData.TargetItem, processedIds, currentDepth + 1)
            : null;

        return new ItemTemplateField(
            id: fieldData.Id,
            name: fieldData.Name ?? string.Empty,
            value: fieldData.Value,
            jsonElement: fieldData.JsonValue,
            typename: fieldData.Typename,
            boolValue: fieldData.BoolValue,
            numberValue: fieldData.NumberValue,
            dateValue: fieldData.DateValue,
            description: fieldData.Description,
            extension: fieldData.Extension,
            keywords: fieldData.Keywords,
            height: fieldData.Height,
            width: fieldData.Width,
            size: fieldData.Size,
            src: fieldData.Src,
            title: fieldData.Title,
            linkType: fieldData.LinkType,
            text: fieldData.Text,
            target: fieldData.Target,
            count: fieldData.Count,
            targetIds: fieldData.TargetIds?.AsReadOnly(),
            targetItems: targetItems,
            targetId: fieldData.TargetId,
            targetItem: targetItem,
            isTargetItemsLoaded: targetItems?.Count > 0,
            isTargetItemLoaded: targetItem != null);
    }
}
