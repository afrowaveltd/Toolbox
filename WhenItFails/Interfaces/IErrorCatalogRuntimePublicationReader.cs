using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Runtime;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Optional runtime capability for reading a context publication and its
/// store-scoped identity as one selected record.
/// </summary>
/// <remarks>
/// Implementations must return a publication recorded by their context
/// store. Never create a generation identifier on each read.
/// The record exposes a live context reference to infrastructure callers.
/// </remarks>
public interface IErrorCatalogRuntimePublicationReader
{
    /// <summary>
    /// Reads the active context publication. The default runtime returns
    /// NotSupported when configured with a store lacking publication support.
    /// </summary>
    Response<ErrorCatalogContextPublication> GetCurrentPublication();
}
