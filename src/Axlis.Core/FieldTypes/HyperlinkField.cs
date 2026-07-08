using Axlis.Fields;

namespace Axlis.Core.FieldTypes;

/// <summary>
/// Wraps a Sitecore General Link field.
/// </summary>
public class HyperlinkField : BaseField, IHyperlinkField
{
    private HyperlinkFieldValue? _cached;

    /// <param name="innerField">The raw field data.</param>
    public HyperlinkField(ItemTemplateField innerField) : base(innerField) { }

    /// <inheritdoc/>
    public override bool IsEmpty => string.IsNullOrEmpty(InnerField.StringValue)
        && string.IsNullOrEmpty(InnerField.LinkType);

    /// <inheritdoc/>
    /// <remarks>Builds <see cref="HyperlinkFieldValue"/> lazily from the JSON value in <see cref="ItemTemplateField"/>.</remarks>
    public HyperlinkFieldValue? Value
    {
        get
        {
            if (IsEmpty)
            {
                return null;
            }

            return _cached ??= BuildValue();
        }
    }

    private HyperlinkFieldValue BuildValue()
    {
        // The hyperlink data may arrive as either a JSON object (jsonValue) or
        // individual scalar fields (text, linkType, etc.) depending on the query shape.
        return new HyperlinkFieldValue
        {
            Href = InnerField.StringValue,
            Text = InnerField.Text,
            Linktype = InnerField.LinkType,
            Title = InnerField.Title
        };
    }
}
