using Afrowave.Toolbox.Essentials.Results;
using System.Text.Json;

namespace Afrowave.Toolbox.WhenItFails.Loading;

/// <summary>
/// Writes JSON catalog documents to files using a conservative safe-write workflow.
/// </summary>
/// <remarks>
/// The writer first writes the new JSON document to a temporary file.
/// If the target file already exists, it creates a timestamped backup.
/// Then it replaces the target file with the temporary file.
/// </remarks>
public sealed class JsonCatalogDocumentWriter
{
    private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        WriteIndented = true
    };

    /// <summary>
    /// Saves a JSON catalog document to the specified file path.
    /// </summary>
    /// <typeparam name="TDocument">Catalog document type.</typeparam>
    /// <param name="document">Document to save.</param>
    /// <param name="filePath">Target JSON file path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Response describing the save result.</returns>
    public async Task<Response> SaveToFileAsync<TDocument>(
       TDocument document,
       string filePath,
       CancellationToken cancellationToken = default)
       where TDocument : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(document);

        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Response.Invalid(
               code: "FilePathIsEmpty",
               message: "JSON catalog file path is empty.");
        }

        string normalizedFilePath = filePath.Trim();
        string? directoryPath = Path.GetDirectoryName(normalizedFilePath);

        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            return Response.Invalid(
               code: "DirectoryPathIsEmpty",
               message: $"JSON catalog directory path could not be resolved from: {normalizedFilePath}");
        }

        try
        {
            Directory.CreateDirectory(directoryPath);

            string temporaryFilePath = CreateTemporaryFilePath(normalizedFilePath);
            string? backupFilePath = null;

            await WriteDocumentToTemporaryFileAsync(
               document,
               temporaryFilePath,
               cancellationToken);

            if (File.Exists(normalizedFilePath))
            {
                backupFilePath = CreateBackupFilePath(normalizedFilePath);

                File.Copy(
                   normalizedFilePath,
                   backupFilePath,
                   overwrite: false);
            }

            File.Move(
               temporaryFilePath,
               normalizedFilePath,
               overwrite: true);

            string message = backupFilePath is null
               ? $"JSON catalog file was saved: {normalizedFilePath}"
               : $"JSON catalog file was saved: {normalizedFilePath}. Backup: {backupFilePath}";

            return Response.Ok(message);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (UnauthorizedAccessException exception)
        {
            return Response.Fail(
               code: "AccessDenied",
               message: $"Access to JSON catalog file was denied: {normalizedFilePath}. {exception.Message}");
        }
        catch (IOException exception)
        {
            return Response.Fail(
               code: "InputOutputError",
               message: $"An I/O error occurred while writing JSON catalog file: {normalizedFilePath}. {exception.Message}");
        }
        catch (JsonException exception)
        {
            return Response.Invalid(
               code: "JsonSerializationFailed",
               message: $"JSON catalog document serialization failed. {exception.Message}");
        }
    }

    private static async Task WriteDocumentToTemporaryFileAsync<TDocument>(
       TDocument document,
       string temporaryFilePath,
       CancellationToken cancellationToken)
       where TDocument : class
    {
        await using FileStream fileStream = new(
           temporaryFilePath,
           FileMode.CreateNew,
           FileAccess.Write,
           FileShare.None);

        await JsonSerializer.SerializeAsync(
           fileStream,
           document,
           DefaultJsonSerializerOptions,
           cancellationToken);

        await fileStream.FlushAsync(cancellationToken);
    }

    private static string CreateTemporaryFilePath(string filePath)
    {
        string directoryPath = Path.GetDirectoryName(filePath) ?? string.Empty;
        string fileName = Path.GetFileName(filePath);
        string temporaryFileName = $".{fileName}.{Guid.NewGuid():N}.tmp";

        return Path.Combine(
           directoryPath,
           temporaryFileName);
    }

    private static string CreateBackupFilePath(string filePath)
    {
        string directoryPath = Path.GetDirectoryName(filePath) ?? string.Empty;
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        string extension = Path.GetExtension(filePath);
        string timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss-fff");
        string backupFileName = $"{fileNameWithoutExtension}.{timestamp}.bak{extension}";

        return Path.Combine(
           directoryPath,
           backupFileName);
    }
}