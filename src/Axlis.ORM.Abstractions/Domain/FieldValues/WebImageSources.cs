namespace Axlis.ORM;

/// <summary>
/// Holds responsive source URLs for a <see cref="WebImage"/> across common breakpoints.
/// </summary>
public class WebImageSources
{
    /// <summary>Gets or sets the small-breakpoint image URL (mobile / narrow viewports). Used as the fallback.</summary>
    public string? Small { get; set; }

    /// <summary>Gets or sets the medium-breakpoint image URL (tablet).</summary>
    public string? Medium { get; set; }

    /// <summary>Gets or sets the large-breakpoint image URL (desktop).</summary>
    public string? Large { get; set; }

    /// <summary>Gets or sets the extra-large-breakpoint image URL (wide/retina displays).</summary>
    public string? XLarge { get; set; }
}
