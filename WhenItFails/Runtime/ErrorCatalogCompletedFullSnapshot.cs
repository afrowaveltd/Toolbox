namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>
/// Detached operational catalog data and the recorded status of one checked
/// completed runtime activation and its selected context publication.
/// </summary>
public sealed class ErrorCatalogCompletedFullSnapshot
{
    internal ErrorCatalogCompletedFullSnapshot(
        Guid storeId,
        long generation,
        long activationSequence,
        ErrorCatalogRuntimeStatus status,
        ErrorCatalogFullSnapshot snapshot)
    {
        if (storeId == Guid.Empty)
        {
            throw new ArgumentException(
                "A completed observation requires a store identifier.",
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

    /// <summary>Gets the identifier of the selected context store.</summary>
    public Guid StoreId { get; }

    /// <summary>Gets the selected publication's generation within its store.</summary>
    public long Generation { get; }

    /// <summary>Gets the runtime-local sequence of the selected completed activation.</summary>
    public long ActivationSequence { get; }

    /// <summary>Gets the status recorded for that completed activation.</summary>
    public ErrorCatalogRuntimeStatus Status { get; }

    /// <summary>Gets all detached operational catalog projections.</summary>
    public ErrorCatalogFullSnapshot Snapshot { get; }
}
