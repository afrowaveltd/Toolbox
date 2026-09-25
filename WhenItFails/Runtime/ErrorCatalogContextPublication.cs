using Afrowave.Toolbox.WhenItFails.Catalog;

namespace Afrowave.Toolbox.WhenItFails.Runtime;

/// <summary>
/// Records one atomic publication of a context in a particular context store.
/// </summary>
/// <remarks>
/// This low-level publication record retains the live mutable context reference.
/// It is not a detached consumer snapshot. The store identifier is valid for
/// the store instance lifetime; a generation advances on every successful Set.
/// </remarks>
public sealed class ErrorCatalogContextPublication
{
    internal ErrorCatalogContextPublication(
        Guid storeId,
        long generation,
        ErrorCatalogContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (storeId == Guid.Empty)
        {
            throw new ArgumentException(
                "A publication requires a non-empty store identifier.",
                nameof(storeId));
        }
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(generation);

        StoreId = storeId;
        Generation = generation;
        Context = context;
    }

    /// <summary>Gets the identity of the context store instance.</summary>
    public Guid StoreId { get; }

    /// <summary>Gets the monotonically increasing publication number within this store.</summary>
    public long Generation { get; }

    /// <summary>
    /// Gets the live context reference held by this publication.
    /// Do not mutate a published context.
    /// </summary>
    public ErrorCatalogContext Context { get; }
}
