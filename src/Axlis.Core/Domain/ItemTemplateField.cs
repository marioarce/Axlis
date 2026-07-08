using System.Text.Json;

namespace Axlis.Core;

/// <summary>
/// Concrete implementation of <see cref="IItemTemplateField"/> carrying all field data
/// returned by the Sitecore Headless GraphQL API, including typed values for every
/// Sitecore field category (text, boolean, image, link, lookup, multilist, date, number).
/// </summary>
public sealed class ItemTemplateField : IItemTemplateField
{
    private ItemTemplateField(string id, string name)
    {
        Id = id;
        Name = name;
        Typename = string.Empty;
    }

    /// <summary>
    /// Full constructor used by <c>ItemConverter</c>.
    /// </summary>
    public ItemTemplateField(
        string id,
        string name,
        string? value,
        JsonElement? jsonElement,
        string typename,
        bool? boolValue = null,
        double? numberValue = null,
        long? dateValue = null,
        string? description = null,
        string? extension = null,
        string? keywords = null,
        int? height = null,
        int? width = null,
        int? size = null,
        string? src = null,
        string? title = null,
        string? linkType = null,
        string? text = null,
        string? target = null,
        int? count = null,
        IReadOnlyList<string?>? targetIds = null,
        IReadOnlyList<Item>? targetItems = null,
        string? targetId = null,
        Item? targetItem = null,
        bool isTargetItemsLoaded = true,
        bool isTargetItemLoaded = true)
    {
        Id = id;
        Name = name;
        StringValue = value;
        JsonValue = jsonElement;
        Typename = typename;
        BoolValue = boolValue;
        NumberValue = numberValue;
        DateValue = dateValue;
        Description = description;
        Extension = extension;
        Keywords = keywords;
        Height = height;
        Width = width;
        Size = size;
        Src = src;
        Title = title;
        LinkType = linkType;
        Text = text;
        Target = target;
        Count = count;
        TargetIds = targetIds;
        TargetItems = targetItems;
        TargetId = targetId;
        TargetItem = targetItem;
        IsTargetItemsLoaded = isTargetItemsLoaded;
        IsTargetItemLoaded = isTargetItemLoaded;
    }

    /// <summary>Returns a sentinel empty field for the given field name (used when a field is missing on an item).</summary>
    public static ItemTemplateField Empty(string fieldName) => new(string.Empty, fieldName);

    // IItemTemplateField
    /// <inheritdoc/>
    public string? FieldId => Id;
    /// <inheritdoc/>
    public string? FieldName => Name;
    /// <inheritdoc/>
    public string? FieldValue => StringValue;
    /// <inheritdoc/>
    public string? FieldType => Typename;
    /// <inheritdoc/>
    public bool Shared => false;
    /// <inheritdoc/>
    public bool Unversioned => false;

    // Raw/internal properties accessed by field-type wrappers
    /// <summary>Gets the field ID.</summary>
    public string Id { get; }
    /// <summary>Gets the field name.</summary>
    public string Name { get; }
    /// <summary>Gets the string value of the field.</summary>
    public string? StringValue { get; }
    /// <summary>Gets the GraphQL __typename (e.g. "TextField", "ImageField").</summary>
    public string Typename { get; }
    /// <summary>Gets the raw JSON element of the field, if available.</summary>
    public JsonElement? JsonValue { get; }

    // Typed values
    /// <summary>Gets the boolean value (CheckboxField).</summary>
    public bool? BoolValue { get; }
    /// <summary>Gets the number value (NumberField).</summary>
    public double? NumberValue { get; }
    /// <summary>Gets the date value as Unix ms (DateField).</summary>
    public long? DateValue { get; }

    // ImageField
    /// <summary>Gets the image description / alt text.</summary>
    public string? Description { get; }
    /// <summary>Gets the file extension.</summary>
    public string? Extension { get; }
    /// <summary>Gets the image keywords.</summary>
    public string? Keywords { get; }
    /// <summary>Gets the image height in pixels.</summary>
    public int? Height { get; }
    /// <summary>Gets the image width in pixels.</summary>
    public int? Width { get; }
    /// <summary>Gets the file size in bytes.</summary>
    public int? Size { get; }
    /// <summary>Gets the image or file source URL.</summary>
    public string? Src { get; }
    /// <summary>Gets the image title attribute.</summary>
    public string? Title { get; }

    // LinkField (HyperlinkField)
    /// <summary>Gets the link type (e.g. "external", "internal").</summary>
    public string? LinkType { get; }
    /// <summary>Gets the link display text.</summary>
    public string? Text { get; }
    /// <summary>Gets the link target attribute (e.g. "_blank").</summary>
    public string? Target { get; }

    // MultilistField
    /// <summary>Gets the count of referenced items.</summary>
    public int? Count { get; }
    /// <summary>Gets the referenced item IDs (MultilistField).</summary>
    public IReadOnlyList<string?>? TargetIds { get; }
    /// <summary>Gets the referenced items (MultilistField), already converted to <see cref="Item"/>.</summary>
    public IReadOnlyList<Item>? TargetItems { get; }

    // LookupField (ItemReferenceField)
    /// <summary>Gets the referenced item ID (LookupField / Droplink).</summary>
    public string? TargetId { get; }
    /// <summary>Gets the referenced item (LookupField / Droplink), already converted to <see cref="Item"/>.</summary>
    public Item? TargetItem { get; }

    /// <summary>Gets a value indicating whether <see cref="TargetItems"/> was fetched (vs. truncated by query depth).</summary>
    public bool IsTargetItemsLoaded { get; }
    /// <summary>Gets a value indicating whether <see cref="TargetItem"/> was fetched (vs. truncated by query depth).</summary>
    public bool IsTargetItemLoaded { get; }
}
