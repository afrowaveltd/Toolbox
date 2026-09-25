using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Runtime;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Optional capability for selecting a completed runtime status and detached
/// catalog data from one associated context publication.
/// </summary>
public interface IErrorCatalogRuntimeCombinedObservationReader
{
    /// <summary>
    /// Attempts a combined capture; returns a non-success response if its
    /// selected status/publication is missing or changed during capture.
    /// </summary>
    Response<ErrorCatalogCompletedCombinedSnapshot> GetCompletedCombinedSnapshot();
}
