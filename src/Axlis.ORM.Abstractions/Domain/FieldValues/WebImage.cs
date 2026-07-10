namespace Axlis;

/// <summary>
/// Represents a responsive image value from a Sitecore Image field,
/// with alternative text and multiple source URLs for different breakpoints.
/// </summary>
public class WebImage
{
    /// <summary>Gets or sets the alternative text for the image (accessibility / SEO).</summary>
    public string AlternativeText { get; set; } = string.Empty;

    /// <summary>Gets or sets the responsive source URLs for this image.</summary>
    public WebImageSources Sources { get; set; } = new WebImageSources();
}
