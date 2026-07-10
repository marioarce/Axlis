using Axlis.ORM.Attributes;
using Axlis.ORM.Core.FieldTypes;

namespace Axlis.ORM.Core.Templates.System;

/// <summary>
/// Strongly-typed wrapper for the Sitecore <b>Dictionary Entry</b> template
/// (<c>{6D1CD897-1936-4A3A-A511-289A94C2A7B1}</c>).
/// Provides access to the Key and Phrase fields used for dictionary-driven translations.
/// </summary>
[SitecoreTemplate("{6D1CD897-1936-4A3A-A511-289A94C2A7B1}")]
public class DictionaryEntry : ExtendedItem
{
    /// <summary>Template ID constant.</summary>
    public const string TemplateIdValue = "{6D1CD897-1936-4A3A-A511-289A94C2A7B1}";

    /// <summary>The dictionary key (used as the lookup identifier).</summary>
    [SitecoreField("Key")]
    public TextField Key => GetField<TextField>("Key");

    /// <summary>The translated phrase for this dictionary entry.</summary>
    [SitecoreField("Phrase")]
    public TextField Phrase => GetField<TextField>("Phrase");
}
