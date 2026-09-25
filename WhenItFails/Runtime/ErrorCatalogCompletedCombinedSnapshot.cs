namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>
/// Detached catalog data and the recorded runtime status associated with one
/// selected context publication.
/// </summary>
/// <remarks>
/// The identity is store-scoped and the activation sequence is runtime-local.
/// This projection does not hold a live context or lock out external writers.
/// </remarks>
public sealed class ErrorCatalogCompletedCombinedSnapshot
{
    internal ErrorCatalogCompletedCombinedSnapshot(
        Guid storeId,
        long generation,
        long activationSequence,
        ErrorCatalogRuntimeStatus status,
        ErrorCatalogCombinedSnapshot snapshot)
    {
        if (storeId == Guid.Empty)
        {
            throw new ArgumentException(
                "The selected publication must have a store identifier.",
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

    /// <summary>Gets the selected context store identifier.</summary>
    public Guid StoreId { get; }

    /// <summary>Gets the selected context publication generation.</summary>
    public long Generation { get; }

    /// <summary>Gets the runtime-local completed status sequence.</summary>
    public long ActivationSequence { get; }

    /// <summary>Gets the status recorded for the selected publication.</summary>
    public ErrorCatalogRuntimeStatus Status { get; }

    /// <summary>Gets the detached main definitions, categories and recorded validation.</summary>
    public ErrorCatalogCombinedSnapshot Snapshot { get; }
}
