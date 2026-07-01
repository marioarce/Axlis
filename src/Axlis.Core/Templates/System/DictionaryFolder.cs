using Axlis.Attributes;

namespace Axlis.Core.Templates.System;

/// <summary>
/// Strongly-typed wrapper for the Sitecore <b>Dictionary Folder</b> template
/// (<c>{267D9AC7-5D85-4E9D-AF89-1ABD3E7E9C3F}</c>).
/// Acts as a container for <see cref="DictionaryEntry"/> items.
/// </summary>
[SitecoreTemplate("{267D9AC7-5D85-4E9D-AF89-1ABD3E7E9C3F}")]
public class DictionaryFolder : ExtendedItem
{
    /// <summary>Template ID constant.</summary>
    public const string TemplateIdValue = "{267D9AC7-5D85-4E9D-AF89-1ABD3E7E9C3F}";
}
