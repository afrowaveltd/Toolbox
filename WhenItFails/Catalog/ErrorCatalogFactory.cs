using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Catalog;

/// <summary>
/// Default factory for creating runtime error catalogs.
/// </summary>
public sealed class ErrorCatalogFactory : IErrorCatalogFactory
{
   /// <inheritdoc />
   public IErrorCatalog Create(ErrorCatalogDocument document)
   {
      ArgumentNullException.ThrowIfNull(document);

      return new ErrorCatalog(document.Errors);
   }
}