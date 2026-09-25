using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Runtime;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Optional capability to observe one completed activation with all detached
/// indexed definitions, supporting catalogs and recorded validation findings.
/// </summary>
public interface IErrorCatalogRuntimeFullObservationReader
{
    /// <summary>
    /// Returns a checked, completed activation observation with complete
    /// operational catalog projections, or a structured non-success response.
    /// </summary>
    Response<ErrorCatalogCompletedFullSnapshot> GetCompletedFullSnapshot();
}
