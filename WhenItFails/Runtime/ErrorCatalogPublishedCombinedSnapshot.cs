namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>
/// Detached combined data associated with one store publication identity.
/// </summary>
/// <remarks>
/// StoreId and Generation identify the context publication, not a synchronized
/// context-plus-runtime-status activation. The projection contains no live
/// context reference and does not guarantee a transaction against concurrent
/// mutation of already published nested objects.
/// </remarks>
public sealed class ErrorCatalogPublishedCombinedSnapshot
{
    internal ErrorCatalogPublishedCombinedSnapshot(
        Guid storeId,
        long generation,
        ErrorCatalogCombinedSnapshot snapshot)
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

    /// <summary>Gets the context store instance identifier.</summary>
    public Guid StoreId { get; }

    /// <summary>Gets the publication generation within this store.</summary>
    public long Generation { get; }

    /// <summary>Gets the detached combined data from this publication.</summary>
    public ErrorCatalogCombinedSnapshot Snapshot { get; }
}
