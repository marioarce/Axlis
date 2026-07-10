namespace Axlis.ORM.Attributes;

/// <summary>
/// Maps a property on an <c>ExtendedItem</c> subclass to its Sitecore field name.
/// <para>
/// This attribute is the codegen hook — a future Roslyn source generator or CLI tool can read it
/// to scaffold field accessors automatically without any breaking changes to consuming code.
/// </para>
/// </summary>
/// <example>
/// <code>
/// [SitecoreField("Phrase")]
/// public TextField Phrase => GetField&lt;TextField&gt;("Phrase");
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class SitecoreFieldAttribute : Attribute
{
    /// <summary>Gets the Sitecore field name as defined on the template (case-insensitive at runtime).</summary>
    public string FieldName { get; }

    /// <summary>Initializes the attribute with the Sitecore field name.</summary>
    /// <param name="fieldName">The exact Sitecore field name (e.g. "Heading", "Body Text").</param>
    public SitecoreFieldAttribute(string fieldName)
    {
        FieldName = fieldName;
    }
}
