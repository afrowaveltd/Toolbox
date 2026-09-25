namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>
/// Four detached supporting catalogs with the status recorded for the same
/// selected completed runtime activation and context publication.
/// </summary>
/// <remarks>
/// StoreId/Generation identify a store publication. ActivationSequence is
/// runtime-local and may advance without a new publication during recovery.
/// This observation does not prevent external in-place source mutation.
/// </remarks>
public sealed class ErrorCatalogCompletedSupportingCatalogsSnapshot
{
    internal ErrorCatalogCompletedSupportingCatalogsSnapshot(
        Guid storeId,
        long generation,
        long activationSequence,
        ErrorCatalogRuntimeStatus status,
        ErrorSupportingCatalogsSnapshot snapshot)
    {
        if (storeId == Guid.Empty)
        {
            throw new ArgumentException(
                "The selected publication requires a nonempty store ID.",
                nameof(storeId));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(generation);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(activationSequence);
        ArgumentNullException.ThrowIfNull(status);
        ArgumentNullException.ThrowIfNull(snapshot);

        StoreId = storeId;
        Generation = generation;
        ActivationSequence = activationSequence;
        Status = status;
        Snapshot = snapshot;
    }

    /// <summary>Gets the selected store identifier.</summary>
    public Guid StoreId { get; }

    /// <summary>Gets the selected publication generation within the store.</summary>
    public long Generation { get; }

    /// <summary>Gets the sequence of the selected completed runtime activation.</summary>
    public long ActivationSequence { get; }

    /// <summary>Gets the status recorded for that activation.</summary>
    public ErrorCatalogRuntimeStatus Status { get; }

    /// <summary>Gets the detached supporting category, owner, code group and profile catalogs.</summary>
    public ErrorSupportingCatalogsSnapshot Snapshot { get; }
}
