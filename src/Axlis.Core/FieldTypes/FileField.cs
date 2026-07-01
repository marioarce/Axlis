using Axlis.Fields;

namespace Axlis.Core.FieldTypes;

/// <summary>
/// Wraps a Sitecore File field (media library attachment).
/// </summary>
public class FileField : BaseField, IFileField
{
    /// <param name="innerField">The raw field data.</param>
    public FileField(ItemTemplateField innerField) : base(innerField) { }

    /// <inheritdoc/>
    public string? Src => InnerField.Src;

    /// <inheritdoc/>
    public override bool IsEmpty => string.IsNullOrEmpty(Src);
}
