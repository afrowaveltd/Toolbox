using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Runtime;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Optional store capability that returns the exact context publication
/// created by a successful write, even if another writer immediately replaces it.
/// </summary>
/// <remarks>
/// This interface is additive; IErrorCatalogContextStore.Set retains its
/// original signature. The returned publication contains a LIVE mutable
/// context reference and is intended for infrastructure, not as a safe
/// application-facing context view.
/// </remarks>
public interface IErrorCatalogContextPublisher
{
    /// <summary>
    /// Publishes the supplied context and returns the exact successful
    /// publication record owned by this call.
    /// </summary>
    /// <param name="context">The context to publish.</param>
    /// <returns>The successful publication record for this write.</returns>
    /// <exception cref="ArgumentNullException">The context is null.</exception>
    ErrorCatalogContextPublication Publish(ErrorCatalogContext context);
}
