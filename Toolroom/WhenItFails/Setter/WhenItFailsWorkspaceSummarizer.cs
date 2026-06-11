using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter;

/// <summary>
/// Loads a project-local WhenItFails JSON workspace and creates a read-only summary.
/// </summary>
internal sealed class WhenItFailsWorkspaceSummarizer
{
    /// <summary>
    /// Loads workspace documents and returns a summary.
    /// </summary>
    /// <param name="inputPath">Project root or Jsons/WhenItFails directory.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Workspace summary.</returns>
    public async Task<WhenItFailsWorkspaceSummary> LoadAsync(
       string inputPath,
       CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(inputPath);

        ErrorCatalogDocument errorCatalogDocument =
           await LoadRequiredDocumentAsync(
              options.ErrorCatalogFilePath,
              new JsonErrorCatalogLoader().LoadFromFileAsync,
              cancellationToken);

        ErrorCategoryCatalogDocument categoryCatalogDocument =
           await LoadRequiredDocumentAsync(
              options.CategoryCatalogFilePath,
              new JsonErrorCategoryCatalogLoader().LoadFromFileAsync,
              cancellationToken);

        ErrorCodeGroupCatalogDocument codeGroupCatalogDocument =
           await LoadRequiredDocumentAsync(
              options.CodeGroupCatalogFilePath,
              new JsonErrorCodeGroupCatalogLoader().LoadFromFileAsync,
              cancellationToken);

        ErrorOwnerCatalogDocument ownerCatalogDocument =
           await LoadRequiredDocumentAsync(
              options.OwnerCatalogFilePath,
              new JsonErrorOwnerCatalogLoader().LoadFromFileAsync,
              cancellationToken);

        ErrorProfileCatalogDocument profileCatalogDocument =
           await LoadRequiredDocumentAsync(
              options.ProfilesFilePath,
              new JsonErrorProfileCatalogLoader().LoadFromFileAsync,
              cancellationToken);

        ErrorCatalogDocument normalizedErrorCatalogDocument =
           new ErrorCatalogDocumentNormalizer().Normalize(errorCatalogDocument);

        ErrorCategoryCatalogDocument normalizedCategoryCatalogDocument =
           new ErrorCategoryCatalogDocumentNormalizer().Normalize(categoryCatalogDocument);

        ErrorCodeGroupCatalogDocument normalizedCodeGroupCatalogDocument =
           new ErrorCodeGroupCatalogDocumentNormalizer().Normalize(codeGroupCatalogDocument);

        ErrorOwnerCatalogDocument normalizedOwnerCatalogDocument =
           new ErrorOwnerCatalogDocumentNormalizer().Normalize(ownerCatalogDocument);

        ErrorProfileCatalogDocument normalizedProfileCatalogDocument =
           new ErrorProfileCatalogDocumentNormalizer().Normalize(profileCatalogDocument);

        return new WhenItFailsWorkspaceSummary
        {
            PackageDirectoryPath = options.PackageDirectoryPath,
            DisplayPath = WhenItFailsWorkspacePathResolver.CreateDisplayPath(
   inputPath,
   options.PackageDirectoryPath),
            ErrorCatalog = normalizedErrorCatalogDocument,
            CategoryCatalog = normalizedCategoryCatalogDocument,
            CodeGroupCatalog = normalizedCodeGroupCatalogDocument,
            OwnerCatalog = normalizedOwnerCatalogDocument,
            ProfileCatalog = normalizedProfileCatalogDocument
        };
    }

    private static async Task<TDocument> LoadRequiredDocumentAsync<TDocument>(
       string filePath,
       Func<string, CancellationToken, Task<Response<TDocument>>> loadAsync,
       CancellationToken cancellationToken)
       where TDocument : class
    {
        Response<TDocument> response =
           await loadAsync(filePath, cancellationToken);

        if (response.IsSuccess && response.Data is not null)
        {
            return response.Data;
        }

        string message = string.IsNullOrWhiteSpace(response.Message)
           ? "Catalog document loading failed."
           : response.Message;

        throw new InvalidOperationException(
           $"{message} File: {filePath}");
    }


}