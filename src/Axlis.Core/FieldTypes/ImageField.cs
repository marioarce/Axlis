using Axlis.Fields;

namespace Axlis.Core.FieldTypes;

/// <summary>
/// Wraps a Sitecore Image field, exposing a <see cref="WebImage"/> value with
/// alternative text and responsive source URLs.
/// </summary>
public class ImageField : BaseField, IImageField
{
    private WebImage? _cached;

    /// <param name="innerField">The raw field data.</param>
    public ImageField(ItemTemplateField innerField) : base(innerField) { }

    /// <inheritdoc/>
    public override bool IsEmpty => string.IsNullOrEmpty(InnerField.Src);

    /// <inheritdoc/>
    /// <remarks>
    /// Builds the <see cref="WebImage"/> lazily on first access.
    /// <see cref="WebImageSources.Small"/> is populated from the GraphQL <c>src</c> value.
    /// </remarks>
    public WebImage? Value
    {
        get
        {
            if (IsEmpty)
            {
                return null;
            }
            return _cached ??= new WebImage
            {
                AlternativeText = InnerField.Description ?? string.Empty,
                Sources = new WebImageSources { Small = InnerField.Src }
            };
        }
    }
}
