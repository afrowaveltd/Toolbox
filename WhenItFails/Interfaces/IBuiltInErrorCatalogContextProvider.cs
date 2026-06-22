using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Creates a validated error catalog context from the bundled
/// Afrowave default catalog templates.
/// </summary>
public interface IBuiltInErrorCatalogContextProvider
{
    /// <summary>
    /// Loads and validates the bundled default catalog context.
    /// </summary>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// Response containing the validated built-in catalog context.
    /// </returns>
    Task<Response<ErrorCatalogContext>> LoadAsync(
        CancellationToken cancellationToken = default);
}