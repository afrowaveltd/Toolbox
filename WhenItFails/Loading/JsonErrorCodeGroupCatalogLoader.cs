using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Loading;

/// <summary>
/// Loads error code group catalog documents from JSON files.
/// </summary>
public sealed class JsonErrorCodeGroupCatalogLoader : IErrorCodeGroupCatalogLoader
{
   private readonly JsonCatalogDocumentLoader _documentLoader;

   /// <summary>
   /// Initializes a new instance of the <see cref="JsonErrorCodeGroupCatalogLoader"/> class.
   /// </summary>
   public JsonErrorCodeGroupCatalogLoader()
       : this(new JsonCatalogDocumentLoader())
   {
   }

   /// <summary>
   /// Initializes a new instance of the <see cref="JsonErrorCodeGroupCatalogLoader"/> class.
   /// </summary>
   /// <param name="documentLoader">Shared JSON document loader.</param>
   public JsonErrorCodeGroupCatalogLoader(JsonCatalogDocumentLoader documentLoader)
   {
      _documentLoader = documentLoader
          ?? throw new ArgumentNullException(nameof(documentLoader));
   }

   /// <inheritdoc />
   public Task<Response<ErrorCodeGroupCatalogDocument>> LoadFromFileAsync(
       string filePath,
       CancellationToken cancellationToken = default)
   {
      return _documentLoader.LoadFromFileAsync<ErrorCodeGroupCatalogDocument>(
          filePath,
          cancellationToken);
   }
}