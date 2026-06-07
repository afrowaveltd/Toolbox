using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Provides a complete loaded error catalog context.
/// </summary>
public interface IErrorCatalogContextProvider
{
    /// <summary>
    /// Loads all configured error catalogs and creates one combined catalog context.
    /// </summary>
    /// <param name="options">JSON workspace options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response containing the loaded error catalog context.</returns>
    Task<Response<ErrorCatalogContext>> LoadFromJsonsAsync(
        JsonsOptions options,
        CancellationToken cancellationToken = default);
}