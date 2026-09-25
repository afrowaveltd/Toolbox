using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Runtime;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Optional publication-aware read contract for a context store.
/// </summary>
/// <remarks>
/// This contract is additive and does not change IErrorCatalogContextStore.
/// It returns a publication record containing a live mutable context reference.
/// Intended for runtime infrastructure, not as a safe application data view.
/// </remarks>
public interface IErrorCatalogContextPublicationReader
{
    /// <summary>
    /// Reads the active context and its store-scoped generation as one
    /// publication record; returns Invalid before the first publication.
    /// </summary>
    Response<ErrorCatalogContextPublication> GetCurrentPublication();
}
