using System.Text.Json;
using System.Text.Json.Serialization;

namespace Axlis.Core.GraphQL;

/// <summary>
/// Represents a single Sitecore field from a GraphQL response.
/// Aggregates all field-type-specific properties into one flat object; properties
/// irrelevant to a given field type will be <c>null</c>.
/// </summary>
public sealed class GraphQLFieldData
{
    /// <summary>Gets or sets the GraphQL <c>__typename</c> (e.g. "TextField", "ImageField", "LookupField").</summary>
    [JsonPropertyName("__typename")]
    public string Typename { get; set; } = string.Empty;

    /// <summary>Gets or sets the field ID.</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the field name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    // ── Common value properties ───────────────────────────────────────────────

    /// <summary>Gets or sets the scalar string value of the field.</summary>
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    /// <summary>Gets or sets the raw JSON value of the field (complex field types).</summary>
    [JsonPropertyName("jsonValue")]
    public JsonElement? JsonValue { get; set; }

    // ── CheckboxField ─────────────────────────────────────────────────────────

    /// <summary>Gets or sets the boolean value (CheckboxField).</summary>
    [JsonPropertyName("boolValue")]
    public bool? BoolValue { get; set; }

    // ── NumberField ───────────────────────────────────────────────────────────

    /// <summary>Gets or sets the number value (NumberField).</summary>
    [JsonPropertyName("numberValue")]
    public double? NumberValue { get; set; }

    // ── DateField ─────────────────────────────────────────────────────────────

    /// <summary>Gets or sets the date as Unix milliseconds (DateField).</summary>
    [JsonPropertyName("dateValue")]
    public long? DateValue { get; set; }

    // ── ImageField ────────────────────────────────────────────────────────────

    /// <summary>Gets or sets the image description / alt text.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>Gets or sets the file extension.</summary>
    [JsonPropertyName("extension")]
    public string? Extension { get; set; }

    /// <summary>Gets or sets the image keywords.</summary>
    [JsonPropertyName("keywords")]
    public string? Keywords { get; set; }

    /// <summary>Gets or sets the image height in pixels.</summary>
    [JsonPropertyName("height")]
    public int? Height { get; set; }

    /// <summary>Gets or sets the image width in pixels.</summary>
    [JsonPropertyName("width")]
    public int? Width { get; set; }

    /// <summary>Gets or sets the file size in bytes.</summary>
    [JsonPropertyName("size")]
    public int? Size { get; set; }

    /// <summary>Gets or sets the image or file source URL.</summary>
    [JsonPropertyName("src")]
    public string? Src { get; set; }

    /// <summary>Gets or sets the image title attribute.</summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    // ── LinkField (HyperlinkField) ────────────────────────────────────────────

    /// <summary>Gets or sets the link type (e.g. "external", "internal").</summary>
    [JsonPropertyName("linkType")]
    public string? LinkType { get; set; }

    /// <summary>Gets or sets the link display text.</summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>Gets or sets the link target attribute (e.g. "_blank").</summary>
    [JsonPropertyName("target")]
    public string? Target { get; set; }

    // ── LookupField (ItemReferenceField) ─────────────────────────────────────

    /// <summary>Gets or sets the referenced item ID (LookupField / Droplink).</summary>
    [JsonPropertyName("targetId")]
    public string? TargetId { get; set; }

    /// <summary>Gets or sets the referenced item (LookupField / Droplink).</summary>
    [JsonPropertyName("targetItem")]
    public GraphQLItemData? TargetItem { get; set; }

    // ── MultilistField ────────────────────────────────────────────────────────

    /// <summary>Gets or sets the count of referenced items.</summary>
    [JsonPropertyName("count")]
    public int? Count { get; set; }

    /// <summary>Gets or sets the referenced item IDs.</summary>
    [JsonPropertyName("targetIds")]
    public List<string?>? TargetIds { get; set; }

    /// <summary>Gets or sets the referenced items (MultilistField).</summary>
    [JsonPropertyName("targetItems")]
    public List<GraphQLItemData>? TargetItems { get; set; }
}
