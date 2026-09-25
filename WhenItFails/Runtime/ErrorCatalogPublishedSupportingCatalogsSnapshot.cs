namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>
/// Detached supporting catalog data from one selected store publication.
/// </summary>
/// <remarks>
/// StoreId and Generation identify the selected context publication, not a
/// synchronized runtime-status activation. The snapshot does not lock the
/// source context against concurrent in-place mutation.
/// </remarks>
public sealed class ErrorCatalogPublishedSupportingCatalogsSnapshot
{
    internal ErrorCatalogPublishedSupportingCatalogsSnapshot(
        Guid storeId,
        long generation,
        ErrorSupportingCatalogsSnapshot snapshot)
    {
        if (storeId == Guid.Empty)
        {
            throw new ArgumentException(
                "A published snapshot requires a nonempty store ID.",
                nameof(storeId));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(generation);
        ArgumentNullException.ThrowIfNull(snapshot);

        StoreId = storeId;
        Generation = generation;
        Snapshot = snapshot;
    }

    /// <summary>Gets the identifier of the selected context store instance.</summary>
    public Guid StoreId { get; }

    /// <summary>Gets the publication generation within that store.</summary>
    public long Generation { get; }

    /// <summary>Gets the detached category, owner, code group and profile catalogs.</summary>
    public ErrorSupportingCatalogsSnapshot Snapshot { get; }
}
