using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Loading;

/// <summary>
/// Loads error owner catalog documents from JSON files.
/// </summary>
public sealed class JsonErrorOwnerCatalogLoader : IErrorOwnerCatalogLoader
{
   private readonly JsonCatalogDocumentLoader _documentLoader;

   /// <summary>
   /// Initializes a new instance of the <see cref="JsonErrorOwnerCatalogLoader"/> class.
   /// </summary>
   public JsonErrorOwnerCatalogLoader()
       : this(new JsonCatalogDocumentLoader())
   {
   }

   /// <summary>
   /// Initializes a new instance of the <see cref="JsonErrorOwnerCatalogLoader"/> class.
   /// </summary>
   /// <param name="documentLoader">Shared JSON document loader.</param>
   public JsonErrorOwnerCatalogLoader(JsonCatalogDocumentLoader documentLoader)
   {
      _documentLoader = documentLoader
          ?? throw new ArgumentNullException(nameof(documentLoader));
   }

   /// <inheritdoc />
   public Task<Response<ErrorOwnerCatalogDocument>> LoadFromFileAsync(
       string filePath,
       CancellationToken cancellationToken = default)
   {
      return _documentLoader.LoadFromFileAsync<ErrorOwnerCatalogDocument>(
          filePath,
          cancellationToken);
   }
}