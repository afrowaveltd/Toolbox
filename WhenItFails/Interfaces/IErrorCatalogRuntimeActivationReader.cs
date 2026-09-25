using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Runtime;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Optional capability for reading a completed runtime status observation
/// associated with a context-store publication.
/// </summary>
/// <remarks>
/// Not part of the existing IErrorCatalogRuntime API. A completed status
/// observation is distinct from the store's context publication and cannot
/// be used as a transaction against external store writers or live mutations.
/// </remarks>
public interface IErrorCatalogRuntimeActivationReader
{
    /// <summary>
    /// Reads a status update and its associated store publication identity,
    /// or a structured non-success result when no matching completed
    /// observation can currently be selected.
    /// </summary>
    Response<ErrorCatalogActivationStatusSnapshot> GetCompletedActivation();
}
