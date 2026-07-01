namespace Axlis.Fields;

/// <summary>
/// Represents a Sitecore Multilist or Treelist field that references multiple items.
/// </summary>
public interface IMultilistField : IBaseField
{
    /// <summary>Gets the raw list of referenced item IDs (GUID strings).</summary>
    IReadOnlyList<string> Ids { get; }

    /// <summary>
    /// Returns the referenced items cast to strongly-typed <typeparamref name="T"/> instances.
    /// Items that cannot be resolved are excluded from the result.
    /// </summary>
    /// <typeparam name="T">The strongly-typed item class derived from <see cref="IBaseItem"/>.</typeparam>
    IReadOnlyList<T> As<T>() where T : class, IBaseItem;
}
