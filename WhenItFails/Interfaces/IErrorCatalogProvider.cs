using Afrowave.Toolbox.WhenItFails.Catalog;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Provides ready-to-use runtime error catalogs from external catalog sources.
/// </summary>
public interface IErrorCatalogProvider
{
   /// <summary>
   /// Loads, validates and creates a runtime error catalog from a JSON file.
   /// </summary>
   /// <param name="filePath">Path to the JSON error catalog file.</param>
   /// <param name="cancellationToken">Cancellation token.</param>
   /// <returns>Provider result containing either a runtime catalog or failure details.</returns>
   Task<ErrorCatalogProviderResult> LoadFromFileAsync(
       string filePath,
       CancellationToken cancellationToken = default);
}