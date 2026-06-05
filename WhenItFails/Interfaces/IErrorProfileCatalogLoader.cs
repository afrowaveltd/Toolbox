using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Loads error profile catalog documents.
/// </summary>
public interface IErrorProfileCatalogLoader
{
    /// <summary>
    /// Loads an error profile catalog document from a JSON file.
    /// </summary>
    /// <param name="filePath">Path to the JSON profile catalog file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response containing the loaded profile catalog document.</returns>
    Task<Response<ErrorProfileCatalogDocument>> LoadFromFileAsync(
        string filePath,
        CancellationToken cancellationToken = default);
}