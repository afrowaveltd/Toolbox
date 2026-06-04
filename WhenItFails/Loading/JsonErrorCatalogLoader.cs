using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using System.Text.Json;

namespace Afrowave.Toolbox.WhenItFails.Loading;

/// <summary>
/// Loads error catalog documents from JSON files.
/// </summary>
public sealed class JsonErrorCatalogLoader : IErrorCatalogLoader
{
   private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
   {
      PropertyNameCaseInsensitive = true,
      ReadCommentHandling = JsonCommentHandling.Skip,
      AllowTrailingCommas = true,
      WriteIndented = true
   };

   /// <inheritdoc />
   public async Task<ErrorCatalogLoadResult> LoadFromFileAsync(
       string filePath,
       CancellationToken cancellationToken = default)
   {
      cancellationToken.ThrowIfCancellationRequested();
      if(string.IsNullOrWhiteSpace(filePath))
      {
         return ErrorCatalogLoadResult.Fail(
             errorCode: "FilePathIsEmpty",
             errorMessage: "Error catalog file path is empty.");
      }

      string normalizedFilePath = filePath.Trim();

      if(!File.Exists(normalizedFilePath))
      {
         return ErrorCatalogLoadResult.Fail(
             errorCode: "FileNotFound",
             errorMessage: $"Error catalog file was not found: {normalizedFilePath}");
      }

      try
      {
         await using FileStream fileStream = File.OpenRead(normalizedFilePath);

         ErrorCatalogDocument? document =
             await JsonSerializer.DeserializeAsync<ErrorCatalogDocument>(
                 fileStream,
                 DefaultJsonSerializerOptions,
                 cancellationToken);

         if(document is null)
         {
            return ErrorCatalogLoadResult.Fail(
                errorCode: "EmptyCatalogDocument",
                errorMessage: "Error catalog file was loaded, but the catalog document is empty.");
         }

         return ErrorCatalogLoadResult.Ok(document);
      }
      catch(JsonException exception)
      {
         return ErrorCatalogLoadResult.Fail(
             errorCode: "InvalidJson",
             errorMessage: "Error catalog file contains invalid JSON.",
             exception: exception);
      }
      catch(OperationCanceledException)
      {
         throw;
      }
      catch(UnauthorizedAccessException exception)
      {
         return ErrorCatalogLoadResult.Fail(
             errorCode: "AccessDenied",
             errorMessage: $"Access to error catalog file was denied: {normalizedFilePath}",
             exception: exception);
      }
      catch(IOException exception)
      {
         return ErrorCatalogLoadResult.Fail(
             errorCode: "InputOutputError",
             errorMessage: $"An I/O error occurred while reading error catalog file: {normalizedFilePath}",
             exception: exception);
      }
   }
}