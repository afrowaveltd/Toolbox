using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Loads error catalog documents from an external source.
/// </summary>
public interface IErrorCatalogLoader
{
   /// <summary>
   /// Loads an error catalog document from the specified file path.
   /// </summary>
   /// <param name="filePath">Path to the JSON error catalog file.</param>
   /// <param name="cancellationToken">Cancellation token.</param>
   /// <returns>Response containing the loaded error catalog document.</returns>
   Task<Response<ErrorCatalogDocument>> LoadFromFileAsync(
       string filePath,
       CancellationToken cancellationToken = default);
}