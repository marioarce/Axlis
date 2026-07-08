namespace Axlis.Fields;

/// <summary>
/// Represents a Sitecore Droplink or Droptree field that references a single item.
/// </summary>
public interface IItemReferenceField : IBaseField
{
    /// <summary>Gets the resolved item reference, or <c>null</c> if the field is empty.</summary>
    ItemReferenceFieldValue? Value { get; }

    /// <summary>
    /// Casts the referenced item to a strongly-typed <typeparamref name="T"/> instance.
    /// Returns <c>null</c> if the field is empty or the reference cannot be resolved.
    /// </summary>
    /// <typeparam name="T">The strongly-typed item class derived from <see cref="IBaseItem"/>.</typeparam>
    T? AsItem<T>() where T : class, IBaseItem;
}
