namespace Axlis.ORM.Fields;

/// <summary>
/// Base contract for all strongly-typed Sitecore field wrappers.
/// </summary>
public interface IBaseField
{
    /// <summary>Gets the field name as defined on the Sitecore template.</summary>
    string? FieldName { get; }

    /// <summary>Gets the raw string value of the field as returned by the GraphQL API.</summary>
    string? RawValue { get; }

    /// <summary>Gets a value indicating whether the field has no meaningful content.</summary>
    bool IsEmpty { get; }
}
