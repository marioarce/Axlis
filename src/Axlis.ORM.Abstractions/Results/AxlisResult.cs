namespace Axlis.ORM.Results;

/// <summary>
/// The rich result envelope returned by the <c>WithResult</c> API on <see cref="Axlis.Services.ISitecoreFacade"/>.
/// Bundles the mapped item, provenance metadata, and captured diagnostics into one object,
/// mirroring the PowerCSharp Cache <c>GetWithResult</c> pattern.
/// </summary>
/// <typeparam name="T">The mapped item type, or <c>IEnumerable&lt;T?&gt;</c> for multi-item calls.</typeparam>
public sealed class AxlisResult<T>
{
    /// <summary>Gets the mapped value, or <c>null</c> if the item was not found or mapping failed.</summary>
    public T? Value { get; init; }

    /// <summary>Gets the provenance metadata for the fetched item, or <c>null</c> if not available.</summary>
    public SitecoreMetadata? Metadata { get; init; }

    /// <summary>Gets the diagnostics collected during the operation, or <c>null</c> if diagnostics are disabled.</summary>
    public AxlisDiagnostics? Diagnostics { get; init; }

    /// <summary>Gets a value indicating whether <see cref="Value"/> is non-null.</summary>
    public bool HasValue => Value is not null;

    /// <summary>Returns an empty result with no value, metadata, or diagnostics.</summary>
    public static AxlisResult<T> Empty => new();
}
