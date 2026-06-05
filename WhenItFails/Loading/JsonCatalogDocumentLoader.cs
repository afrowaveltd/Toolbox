using Afrowave.Toolbox.Essentials.Results;
using System.Text.Json;

namespace Afrowave.Toolbox.WhenItFails.Loading;

/// <summary>
/// Loads JSON catalog documents from files.
/// </summary>
/// <remarks>
/// This is a shared helper used by concrete catalog loaders.
/// It returns the standard Essentials <see cref="Response{T}"/> type
/// instead of creating package-specific result wrappers.
/// </remarks>
public sealed class JsonCatalogDocumentLoader
{
   private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
   {
      PropertyNameCaseInsensitive = true,
      ReadCommentHandling = JsonCommentHandling.Skip,
      AllowTrailingCommas = true,
      WriteIndented = true
   };

   /// <summary>
   /// Loads a JSON catalog document from the specified file path.
   /// </summary>
   /// <typeparam name="TDocument">Catalog document type.</typeparam>
   /// <param name="filePath">Path to the JSON catalog document.</param>
   /// <param name="cancellationToken">Cancellation token.</param>
   /// <returns>Response containing the loaded document or loading issues.</returns>
   public async Task<Response<TDocument>> LoadFromFileAsync<TDocument>(
       string filePath,
       CancellationToken cancellationToken = default)
       where TDocument : class
   {
      cancellationToken.ThrowIfCancellationRequested();

      if(string.IsNullOrWhiteSpace(filePath))
      {
         return Response<TDocument>.Invalid(
             code: "FilePathIsEmpty",
             message: "JSON catalog file path is empty.");
      }

      string normalizedFilePath = filePath.Trim();

      if(!File.Exists(normalizedFilePath))
      {
         return Response<TDocument>.NotFound(
             code: "FileNotFound",
             message: $"JSON catalog file was not found: {normalizedFilePath}");
      }

      try
      {
         await using FileStream fileStream = File.OpenRead(normalizedFilePath);

         TDocument? document =
             await JsonSerializer.DeserializeAsync<TDocument>(
                 fileStream,
                 DefaultJsonSerializerOptions,
                 cancellationToken);

         if(document is null)
         {
            return Response<TDocument>.Invalid(
                code: "EmptyCatalogDocument",
                message: "JSON catalog file was loaded, but the catalog document is empty.");
         }

         return Response<TDocument>.Ok(document);
      }
      catch(JsonException exception)
      {
         return Response<TDocument>.Invalid(
             code: "InvalidJson",
             message: $"JSON catalog file contains invalid JSON. {exception.Message}");
      }
      catch(OperationCanceledException)
      {
         throw;
      }
      catch(UnauthorizedAccessException exception)
      {
         return Response<TDocument>.Fail(
             code: "AccessDenied",
             message: $"Access to JSON catalog file was denied: {normalizedFilePath}. {exception.Message}");
      }
      catch(IOException exception)
      {
         return Response<TDocument>.Fail(
             code: "InputOutputError",
             message: $"An I/O error occurred while reading JSON catalog file: {normalizedFilePath}. {exception.Message}");
      }
   }
}