using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Creates runtime error catalogs from loaded error catalog documents.
/// </summary>
public interface IErrorCatalogFactory
{
   /// <summary>
   /// Creates an indexed runtime error catalog from the specified catalog document.
   /// </summary>
   /// <param name="document">Loaded error catalog document.</param>
   /// <returns>Runtime error catalog.</returns>
   IErrorCatalog Create(ErrorCatalogDocument document);
}