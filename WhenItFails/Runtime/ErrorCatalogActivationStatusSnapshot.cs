namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>
/// Read-only observation of one completed runtime status update associated
/// with a selected context-store publication.
/// </summary>
/// <remarks>
/// The store publication ID and runtime-local activation sequence have
/// different lifetimes. The status is immutable via its public init-only
/// properties. This record does not contain the live catalog context or
/// claim that a separately acquired status or catalog snapshot is paired.
/// </remarks>
public sealed class ErrorCatalogActivationStatusSnapshot
{
    internal ErrorCatalogActivationStatusSnapshot(
        Guid storeId,
        long generation,
        long activationSequence,
        ErrorCatalogRuntimeStatus status)
    {
        if (storeId == Guid.Empty)
        {
            throw new ArgumentException(
                "An activation observation needs a store identifier.",
                nameof(storeId));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(generation);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(activationSequence);
        ArgumentNullException.ThrowIfNull(status);

        StoreId = storeId;
        Generation = generation;
        ActivationSequence = activationSequence;
        Status = status;
    }

    /// <summary>Gets the context-store instance identifier.</summary>
    public Guid StoreId { get; }

    /// <summary>Gets the selected context publication generation.</summary>
    public long Generation { get; }

    /// <summary>
    /// Gets the runtime-local completed status-update sequence number.
    /// It can advance even while the context generation remains unchanged.
    /// </summary>
    public long ActivationSequence { get; }

    /// <summary>Gets the status recorded by this completed update.</summary>
    public ErrorCatalogRuntimeStatus Status { get; }
}
