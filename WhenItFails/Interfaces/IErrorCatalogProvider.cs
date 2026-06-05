using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Provides ready-to-use runtime error catalogs from external catalog sources.
/// </summary>
public interface IErrorCatalogProvider
{
   /// <summary>
   /// Loads, normalizes, validates and creates a runtime error catalog from a JSON file.
   /// </summary>
   /// <param name="filePath">Path to the JSON error catalog file.</param>
   /// <param name="cancellationToken">Cancellation token.</param>
   /// <returns>Response containing the provider payload.</returns>
   Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
       string filePath,
       CancellationToken cancellationToken = default);
}