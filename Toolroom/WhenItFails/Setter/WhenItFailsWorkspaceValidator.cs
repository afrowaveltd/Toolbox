using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Issues;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter;

/// <summary>
/// Validates a project-local WhenItFails JSON workspace.
/// </summary>
internal sealed class WhenItFailsWorkspaceValidator
{
   /// <summary>
   /// Validates the workspace at the specified path.
   /// </summary>
   /// <param name="inputPath">Project root or Jsons/WhenItFails directory.</param>
   /// <param name="cancellationToken">Cancellation token.</param>
   /// <returns>Validation outcome.</returns>
   public async Task<WhenItFailsWorkspaceValidationOutcome> ValidateAsync(
      string inputPath,
      CancellationToken cancellationToken = default)
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

      JsonsOptions options = ResolveJsonsOptions(inputPath);
      ErrorCatalogValidationResult combinedResult = new();

      ErrorCatalogDocument? errorCatalogDocument = await LoadNormalizeValidateErrorCatalogAsync(
         options.ErrorCatalogFilePath,
         combinedResult,
         cancellationToken);

      ErrorCategoryCatalogDocument? categoryCatalogDocument = await LoadNormalizeValidateCategoryCatalogAsync(
         options.CategoryCatalogFilePath,
         combinedResult,
         cancellationToken);

      ErrorCodeGroupCatalogDocument? codeGroupCatalogDocument = await LoadNormalizeValidateCodeGroupCatalogAsync(
         options.CodeGroupCatalogFilePath,
         combinedResult,
         cancellationToken);

      ErrorOwnerCatalogDocument? ownerCatalogDocument = await LoadNormalizeValidateOwnerCatalogAsync(
         options.OwnerCatalogFilePath,
         combinedResult,
         cancellationToken);

      ErrorProfileCatalogDocument? profileCatalogDocument = await LoadNormalizeValidateProfileCatalogAsync(
         options.ProfilesFilePath,
         combinedResult,
         cancellationToken);

      if (errorCatalogDocument is not null
          && categoryCatalogDocument is not null
          && codeGroupCatalogDocument is not null
          && ownerCatalogDocument is not null
          && profileCatalogDocument is not null)
      {
         ErrorCatalogCrossValidator crossValidator = new();

         ErrorCatalogValidationResult crossValidationResult = crossValidator.Validate(
            errorCatalogDocument,
            ownerCatalogDocument,
            codeGroupCatalogDocument,
            categoryCatalogDocument,
            profileCatalogDocument);

         CopyIssues(
            source: crossValidationResult,
            target: combinedResult);
      }

      return new WhenItFailsWorkspaceValidationOutcome
      {
         PackageDirectoryPath = options.PackageDirectoryPath,
         ValidationResult = combinedResult
      };
   }

   private static JsonsOptions ResolveJsonsOptions(string inputPath)
   {
      string fullInputPath = Path.GetFullPath(inputPath);

      if (LooksLikePackageDirectory(fullInputPath))
      {
         DirectoryInfo packageDirectory = new(fullInputPath);

         return new JsonsOptions
         {
            RootDirectory = packageDirectory.Parent?.FullName ?? fullInputPath,
            PackageDirectoryName = packageDirectory.Name
         };
      }

      string defaultPackageDirectory = Path.Combine(
         fullInputPath,
         "Jsons",
         "WhenItFails");

      if (Directory.Exists(defaultPackageDirectory)
          || !Directory.Exists(fullInputPath))
      {
         return new JsonsOptions
         {
            RootDirectory = Path.Combine(fullInputPath, "Jsons"),
            PackageDirectoryName = "WhenItFails"
         };
      }

      return new JsonsOptions
      {
         RootDirectory = Path.Combine(fullInputPath, "Jsons"),
         PackageDirectoryName = "WhenItFails"
      };
   }

   private static bool LooksLikePackageDirectory(string directoryPath)
   {
      if (!Directory.Exists(directoryPath))
      {
         return false;
      }

      return File.Exists(Path.Combine(directoryPath, "errors.en.json"))
         || File.Exists(Path.Combine(directoryPath, "categories.en.json"))
         || File.Exists(Path.Combine(directoryPath, "code-groups.en.json"))
         || File.Exists(Path.Combine(directoryPath, "owners.en.json"))
         || File.Exists(Path.Combine(directoryPath, "profiles.json"));
   }

   private static async Task<ErrorCatalogDocument?> LoadNormalizeValidateErrorCatalogAsync(
      string filePath,
      ErrorCatalogValidationResult combinedResult,
      CancellationToken cancellationToken)
   {
      JsonErrorCatalogLoader loader = new();

      Response<ErrorCatalogDocument> loadResponse =
         await loader.LoadFromFileAsync(filePath, cancellationToken);

      if (!loadResponse.IsSuccess || loadResponse.Data is null)
      {
         AddResponseIssues(
            combinedResult,
            loadResponse,
            filePath,
            fallbackCode: "ErrorCatalogLoadFailed",
            fallbackMessage: "Error catalog loading failed.");

         return null;
      }

      ErrorCatalogDocument normalizedDocument =
         new ErrorCatalogDocumentNormalizer().Normalize(loadResponse.Data);

      ErrorCatalogValidationResult validationResult =
         new ErrorCatalogValidator().Validate(normalizedDocument);

      CopyIssues(validationResult, combinedResult);

      return validationResult.IsValid
         ? normalizedDocument
         : null;
   }

   private static async Task<ErrorCategoryCatalogDocument?> LoadNormalizeValidateCategoryCatalogAsync(
      string filePath,
      ErrorCatalogValidationResult combinedResult,
      CancellationToken cancellationToken)
   {
      JsonErrorCategoryCatalogLoader loader = new();

      Response<ErrorCategoryCatalogDocument> loadResponse =
         await loader.LoadFromFileAsync(filePath, cancellationToken);

      if (!loadResponse.IsSuccess || loadResponse.Data is null)
      {
         AddResponseIssues(
            combinedResult,
            loadResponse,
            filePath,
            fallbackCode: "CategoryCatalogLoadFailed",
            fallbackMessage: "Error category catalog loading failed.");

         return null;
      }

      ErrorCategoryCatalogDocument normalizedDocument =
         new ErrorCategoryCatalogDocumentNormalizer().Normalize(loadResponse.Data);

      ErrorCatalogValidationResult validationResult =
         new ErrorCategoryCatalogValidator().Validate(normalizedDocument);

      CopyIssues(validationResult, combinedResult);

      return validationResult.IsValid
         ? normalizedDocument
         : null;
   }

   private static async Task<ErrorCodeGroupCatalogDocument?> LoadNormalizeValidateCodeGroupCatalogAsync(
      string filePath,
      ErrorCatalogValidationResult combinedResult,
      CancellationToken cancellationToken)
   {
      JsonErrorCodeGroupCatalogLoader loader = new();

      Response<ErrorCodeGroupCatalogDocument> loadResponse =
         await loader.LoadFromFileAsync(filePath, cancellationToken);

      if (!loadResponse.IsSuccess || loadResponse.Data is null)
      {
         AddResponseIssues(
            combinedResult,
            loadResponse,
            filePath,
            fallbackCode: "CodeGroupCatalogLoadFailed",
            fallbackMessage: "Error code group catalog loading failed.");

         return null;
      }

      ErrorCodeGroupCatalogDocument normalizedDocument =
         new ErrorCodeGroupCatalogDocumentNormalizer().Normalize(loadResponse.Data);

      ErrorCatalogValidationResult validationResult =
         new ErrorCodeGroupCatalogValidator().Validate(normalizedDocument);

      CopyIssues(validationResult, combinedResult);

      return validationResult.IsValid
         ? normalizedDocument
         : null;
   }

   private static async Task<ErrorOwnerCatalogDocument?> LoadNormalizeValidateOwnerCatalogAsync(
      string filePath,
      ErrorCatalogValidationResult combinedResult,
      CancellationToken cancellationToken)
   {
      JsonErrorOwnerCatalogLoader loader = new();

      Response<ErrorOwnerCatalogDocument> loadResponse =
         await loader.LoadFromFileAsync(filePath, cancellationToken);

      if (!loadResponse.IsSuccess || loadResponse.Data is null)
      {
         AddResponseIssues(
            combinedResult,
            loadResponse,
            filePath,
            fallbackCode: "OwnerCatalogLoadFailed",
            fallbackMessage: "Error owner catalog loading failed.");

         return null;
      }

      ErrorOwnerCatalogDocument normalizedDocument =
         new ErrorOwnerCatalogDocumentNormalizer().Normalize(loadResponse.Data);

      ErrorCatalogValidationResult validationResult =
         new ErrorOwnerCatalogValidator().Validate(normalizedDocument);

      CopyIssues(validationResult, combinedResult);

      return validationResult.IsValid
         ? normalizedDocument
         : null;
   }

   private static async Task<ErrorProfileCatalogDocument?> LoadNormalizeValidateProfileCatalogAsync(
      string filePath,
      ErrorCatalogValidationResult combinedResult,
      CancellationToken cancellationToken)
   {
      JsonErrorProfileCatalogLoader loader = new();

      Response<ErrorProfileCatalogDocument> loadResponse =
         await loader.LoadFromFileAsync(filePath, cancellationToken);

      if (!loadResponse.IsSuccess || loadResponse.Data is null)
      {
         AddResponseIssues(
            combinedResult,
            loadResponse,
            filePath,
            fallbackCode: "ProfileCatalogLoadFailed",
            fallbackMessage: "Error profile catalog loading failed.");

         return null;
      }

      ErrorProfileCatalogDocument normalizedDocument =
         new ErrorProfileCatalogDocumentNormalizer().Normalize(loadResponse.Data);

      ErrorCatalogValidationResult validationResult =
         new ErrorProfileCatalogValidator().Validate(normalizedDocument);

      CopyIssues(validationResult, combinedResult);

      return validationResult.IsValid
         ? normalizedDocument
         : null;
   }

   private static void AddResponseIssues<TData>(
      ErrorCatalogValidationResult validationResult,
      Response<TData> response,
      string filePath,
      string fallbackCode,
      string fallbackMessage)
   {
      if (response.Issues.Count == 0)
      {
         validationResult.AddError(
            code: fallbackCode,
            message: string.IsNullOrWhiteSpace(response.Message)
               ? fallbackMessage
               : response.Message,
            path: filePath);

         return;
      }

      foreach (IssueInfo issue in response.Issues)
      {
         AddIssueInfo(
            validationResult,
            issue,
            filePath);
      }
   }

   private static void AddIssueInfo(
      ErrorCatalogValidationResult validationResult,
      IssueInfo issue,
      string filePath)
   {
      string issueMessage = string.IsNullOrWhiteSpace(issue.Details)
         ? issue.Message
         : issue.Message + " " + issue.Details;

      if (issue.Severity >= IssueSeverity.Error)
      {
         validationResult.AddError(
            code: issue.Code,
            message: issueMessage,
            path: filePath);

         return;
      }

      if (issue.Severity == IssueSeverity.Warning)
      {
         validationResult.AddWarning(
            code: issue.Code,
            message: issueMessage,
            path: filePath);

         return;
      }

      validationResult.AddInformation(
         code: issue.Code,
         message: issueMessage,
         path: filePath);
   }

   private static void CopyIssues(
      ErrorCatalogValidationResult source,
      ErrorCatalogValidationResult target)
   {
      foreach (ErrorCatalogValidationIssue issue in source.Issues)
      {
         target.AddIssue(issue);
      }
   }
}
