using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Loads error owner catalog documents.
/// </summary>
public interface IErrorOwnerCatalogLoader
{
   /// <summary>
   /// Loads an error owner catalog document from a JSON file.
   /// </summary>
   /// <param name="filePath">Path to the JSON owner catalog file.</param>
   /// <param name="cancellationToken">Cancellation token.</param>
   /// <returns>Response containing the loaded owner catalog document.</returns>
   Task<Response<ErrorOwnerCatalogDocument>> LoadFromFileAsync(
       string filePath,
       CancellationToken cancellationToken = default);
}