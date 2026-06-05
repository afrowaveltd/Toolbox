using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Provides normalized and validated error category catalog documents.
/// </summary>
public interface IErrorCategoryCatalogProvider
{
   /// <summary>
   /// Loads, normalizes and validates an error category catalog document from a JSON file.
   /// </summary>
   /// <param name="filePath">Path to the JSON category catalog file.</param>
   /// <param name="cancellationToken">Cancellation token.</param>
   /// <returns>Response containing the category catalog provider payload.</returns>
   Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
       string filePath,
       CancellationToken cancellationToken = default);
}