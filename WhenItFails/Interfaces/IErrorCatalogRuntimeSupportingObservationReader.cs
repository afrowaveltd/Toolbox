using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Runtime;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Optional runtime capability for reading a completed activation status
/// together with detached data from all four supporting catalogs.
/// </summary>
public interface IErrorCatalogRuntimeSupportingObservationReader
{
    /// <summary>
    /// Captures one selected completed activation and its supporting catalogs.
    /// Returns a non-success response if status or publication changes during
    /// the capture, or if the capability is unavailable.
    /// </summary>
    Response<ErrorCatalogCompletedSupportingCatalogsSnapshot>
        GetCompletedSupportingCatalogsSnapshot();
}
