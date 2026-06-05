using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Provides normalized and validated error code group catalog documents.
/// </summary>
public interface IErrorCodeGroupCatalogProvider
{
   /// <summary>
   /// Loads, normalizes and validates an error code group catalog document from a JSON file.
   /// </summary>
   /// <param name="filePath">Path to the JSON code group catalog file.</param>
   /// <param name="cancellationToken">Cancellation token.</param>
   /// <returns>Response containing the code group catalog provider payload.</returns>
   Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
       string filePath,
       CancellationToken cancellationToken = default);
}