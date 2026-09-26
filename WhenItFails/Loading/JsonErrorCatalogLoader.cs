using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Loading;

/// <summary>
/// Loads error catalog documents from JSON files.
/// </summary>
public sealed class JsonErrorCatalogLoader : IErrorCatalogLoader
{
   private readonly JsonCatalogDocumentLoader _documentLoader;

   /// <summary>
   /// Initializes a new instance of the <see cref="JsonErrorCatalogLoader"/> class.
   /// </summary>
   public JsonErrorCatalogLoader()
       : this(new JsonCatalogDocumentLoader())
   {
   }

   /// <summary>
   /// Initializes a new instance of the <see cref="JsonErrorCatalogLoader"/> class.
   /// </summary>
   /// <param name="documentLoader">Shared JSON document loader.</param>
   internal JsonErrorCatalogLoader(JsonCatalogDocumentLoader documentLoader)
   {
      _documentLoader = documentLoader
          ?? throw new ArgumentNullException(nameof(documentLoader));
   }

   /// <inheritdoc />
   public Task<Response<ErrorCatalogDocument>> LoadFromFileAsync(
       string filePath,
       CancellationToken cancellationToken = default)
   {
      return _documentLoader.LoadFromFileAsync<ErrorCatalogDocument>(
          filePath,
          cancellationToken);
   }
}