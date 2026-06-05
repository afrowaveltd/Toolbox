using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Provides normalized and validated error owner catalog documents.
/// </summary>
public interface IErrorOwnerCatalogProvider
{
   /// <summary>
   /// Loads, normalizes and validates an error owner catalog document from a JSON file.
   /// </summary>
   /// <param name="filePath">Path to the JSON owner catalog file.</param>
   /// <param name="cancellationToken">Cancellation token.</param>
   /// <returns>Response containing the owner catalog provider payload.</returns>
   Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
       string filePath,
       CancellationToken cancellationToken = default);
}