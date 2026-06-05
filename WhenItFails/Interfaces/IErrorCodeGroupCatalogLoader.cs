using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Loads error code group catalog documents.
/// </summary>
public interface IErrorCodeGroupCatalogLoader
{
   /// <summary>
   /// Loads an error code group catalog document from a JSON file.
   /// </summary>
   /// <param name="filePath">Path to the JSON code group catalog file.</param>
   /// <param name="cancellationToken">Cancellation token.</param>
   /// <returns>Response containing the loaded code group catalog document.</returns>
   Task<Response<ErrorCodeGroupCatalogDocument>> LoadFromFileAsync(
       string filePath,
       CancellationToken cancellationToken = default);
}