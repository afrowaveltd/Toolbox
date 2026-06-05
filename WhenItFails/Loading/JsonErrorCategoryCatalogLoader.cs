using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Loading;

/// <summary>
/// Loads error category catalog documents from JSON files.
/// </summary>
public sealed class JsonErrorCategoryCatalogLoader : IErrorCategoryCatalogLoader
{
   private readonly JsonCatalogDocumentLoader _documentLoader;

   /// <summary>
   /// Initializes a new instance of the <see cref="JsonErrorCategoryCatalogLoader"/> class.
   /// </summary>
   public JsonErrorCategoryCatalogLoader()
       : this(new JsonCatalogDocumentLoader())
   {
   }

   /// <summary>
   /// Initializes a new instance of the <see cref="JsonErrorCategoryCatalogLoader"/> class.
   /// </summary>
   /// <param name="documentLoader">Shared JSON document loader.</param>
   public JsonErrorCategoryCatalogLoader(JsonCatalogDocumentLoader documentLoader)
   {
      _documentLoader = documentLoader
          ?? throw new ArgumentNullException(nameof(documentLoader));
   }

   /// <inheritdoc />
   public Task<Response<ErrorCategoryCatalogDocument>> LoadFromFileAsync(
       string filePath,
       CancellationToken cancellationToken = default)
   {
      return _documentLoader.LoadFromFileAsync<ErrorCategoryCatalogDocument>(
          filePath,
          cancellationToken);
   }
}