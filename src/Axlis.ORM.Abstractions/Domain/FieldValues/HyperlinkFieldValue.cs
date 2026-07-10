namespace Axlis.ORM;

/// <summary>
/// Represents the strongly-typed value of a Sitecore General Link (hyperlink) field.
/// </summary>
public class HyperlinkFieldValue
{
    /// <summary>Gets or sets the URL or path the hyperlink points to.</summary>
    public string? Href { get; set; }

    /// <summary>Gets or sets the visible link text.</summary>
    public string? Text { get; set; }

    /// <summary>Gets or sets the page anchor fragment (without the leading #).</summary>
    public string? Anchor { get; set; }

    /// <summary>Gets or sets the link type (e.g. "external", "internal", "anchor", "mailto").</summary>
    public string? Linktype { get; set; }

    /// <summary>Gets or sets the CSS class name applied to the rendered link.</summary>
    public string? Class { get; set; }

    /// <summary>Gets or sets the link title attribute (rendered as a tooltip).</summary>
    public string? Title { get; set; }

    /// <summary>Gets or sets the query string appended to the URL (without the leading ?).</summary>
    public string? Querystring { get; set; }

    /// <summary>Gets or sets the Sitecore internal identifier for this link field.</summary>
    public string? Id { get; set; }
}
