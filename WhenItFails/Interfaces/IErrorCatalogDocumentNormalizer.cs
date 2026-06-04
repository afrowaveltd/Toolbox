using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Normalizes complete error catalog documents before validation and runtime catalog creation.
/// </summary>
public interface IErrorCatalogDocumentNormalizer
{
   /// <summary>
   /// Creates a normalized copy of the specified catalog document.
   /// </summary>
   /// <param name="document">Source catalog document.</param>
   /// <returns>Normalized catalog document copy.</returns>
   ErrorCatalogDocument Normalize(ErrorCatalogDocument document);
}