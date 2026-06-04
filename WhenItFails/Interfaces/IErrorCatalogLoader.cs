using Afrowave.Toolbox.WhenItFails.Loading;

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
   /// <returns>Catalog loading result.</returns>
   Task<ErrorCatalogLoadResult> LoadFromFileAsync(
       string filePath,
       CancellationToken cancellationToken = default);
}