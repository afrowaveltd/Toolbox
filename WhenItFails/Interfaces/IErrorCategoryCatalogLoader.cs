using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Loads error category catalog documents.
/// </summary>
public interface IErrorCategoryCatalogLoader
{
   /// <summary>
   /// Loads an error category catalog document from a JSON file.
   /// </summary>
   /// <param name="filePath">Path to the JSON category catalog file.</param>
   /// <param name="cancellationToken">Cancellation token.</param>
   /// <returns>Response containing the loaded category catalog document.</returns>
   Task<Response<ErrorCategoryCatalogDocument>> LoadFromFileAsync(
       string filePath,
       CancellationToken cancellationToken = default);
}