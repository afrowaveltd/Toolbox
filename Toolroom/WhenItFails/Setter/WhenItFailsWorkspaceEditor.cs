using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter;

/// <summary>
/// Provides safe write operations for a project-local WhenItFails JSON workspace.
/// </summary>
internal sealed class WhenItFailsWorkspaceEditor
{
   /// <summary>
   /// Sets the title of one error definition.
   /// </summary>
   /// <param name="inputPath">Project root or Jsons/WhenItFails directory.</param>
   /// <param name="lookupValue">Error id, numeric code, or name.</param>
   /// <param name="newTitle">New error title.</param>
   /// <param name="cancellationToken">Cancellation token.</param>
   /// <returns>The edited error definition.</returns>
   public async Task<Response<ErrorDefinition>> SetErrorTitleAsync(
      string inputPath,
      string lookupValue,
      string newTitle,
      CancellationToken cancellationToken = default)
   {
      return await SetErrorTextValueAsync(
          inputPath,
          lookupValue,
          newTitle,
          fieldName: "title",
          emptyValueCode: "TitleIsEmpty",
          emptyValueMessage: "Error title cannot be empty.",
          getCurrentValue: errorDefinition => errorDefinition.Title,
          setNewValue: (errorDefinition, value) => errorDefinition.Title = value,
          cancellationToken);
   }

   /// <summary>
   /// Sets the message of one error definition.
   /// </summary>
   /// <param name="inputPath">Project root or Jsons/WhenItFails directory.</param>
   /// <param name="lookupValue">Error id, numeric code, or name.</param>
   /// <param name="newMessage">New error message.</param>
   /// <param name="cancellationToken">Cancellation token.</param>
   /// <returns>The edited error definition.</returns>
   public async Task<Response<ErrorDefinition>> SetErrorMessageAsync(
       string inputPath,
       string lookupValue,
       string newMessage,
       CancellationToken cancellationToken = default)
   {
      return await SetErrorTextValueAsync(
          inputPath,
          lookupValue,
          newMessage,
          fieldName: "message",
          emptyValueCode: "MessageIsEmpty",
          emptyValueMessage: "Error message cannot be empty.",
          getCurrentValue: errorDefinition => errorDefinition.Message,
          setNewValue: (errorDefinition, value) => errorDefinition.Message = value,
          cancellationToken);
   }

   /// <summary>
   /// Sets the developer hint of one error definition.
   /// </summary>
   /// <param name="inputPath">Project root or Jsons/WhenItFails directory.</param>
   /// <param name="lookupValue">Error id, numeric code, or name.</param>
   /// <param name="newDeveloperHint">New developer hint.</param>
   /// <param name="cancellationToken">Cancellation token.</param>
   /// <returns>The edited error definition.</returns>
   public async Task<Response<ErrorDefinition>> SetErrorDeveloperHintAsync(
       string inputPath,
       string lookupValue,
       string newDeveloperHint,
       CancellationToken cancellationToken = default)
   {
      return await SetErrorTextValueAsync(
          inputPath,
          lookupValue,
          newDeveloperHint,
          fieldName: "developer hint",
          emptyValueCode: "DeveloperHintIsEmpty",
          emptyValueMessage: "Developer hint cannot be empty.",
          getCurrentValue: errorDefinition => errorDefinition.DeveloperHint,
          setNewValue: (errorDefinition, value) => errorDefinition.DeveloperHint = value,
          cancellationToken);
   }

   /// <summary>
   /// Sets the documentation key of one error definition.
   /// </summary>
   /// <param name="inputPath">Project root or Jsons/WhenItFails directory.</param>
   /// <param name="lookupValue">Error id, numeric code, or name.</param>
   /// <param name="newDocumentationKey">New documentation key.</param>
   /// <param name="cancellationToken">Cancellation token.</param>
   /// <returns>The edited error definition.</returns>
   public async Task<Response<ErrorDefinition>> SetErrorDocumentationKeyAsync(
       string inputPath,
       string lookupValue,
       string newDocumentationKey,
       CancellationToken cancellationToken = default)
   {
      if (!string.IsNullOrWhiteSpace(newDocumentationKey) &&
          !WhenItFailsDocumentationKeyFormatChecker.IsCanonical(newDocumentationKey))
      {
         return Response<ErrorDefinition>.Invalid(
             code: "InvalidDocumentationKeyFormat",
             message: "Documentation key must use lowercase slash-separated kebab-case, for example 'when-it-fails/errors/network/network-unavailable'.");
      }

      return await SetErrorTextValueAsync(
          inputPath,
          lookupValue,
          newDocumentationKey,
          fieldName: "documentation key",
          emptyValueCode: "DocumentationKeyIsEmpty",
          emptyValueMessage: "Documentation key cannot be empty.",
          getCurrentValue: errorDefinition => errorDefinition.DocumentationKey,
          setNewValue: (errorDefinition, value) => errorDefinition.DocumentationKey = value,
          cancellationToken);
   }

   /// <summary>
   /// Sets the severity of one error definition.
   /// </summary>
   /// <param name="inputPath">Project root or Jsons/WhenItFails directory.</param>
   /// <param name="lookupValue">Error id, numeric code, or name.</param>
   /// <param name="newSeverity">New severity value.</param>
   /// <param name="cancellationToken">Cancellation token.</param>
   /// <returns>The edited error definition.</returns>
   public async Task<Response<ErrorDefinition>> SetErrorSeverityAsync(
       string inputPath,
       string lookupValue,
       string newSeverity,
       CancellationToken cancellationToken = default)
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);
      ArgumentException.ThrowIfNullOrWhiteSpace(lookupValue);

      if (string.IsNullOrWhiteSpace(newSeverity))
      {
         return Response<ErrorDefinition>.Invalid(
             code: "SeverityIsEmpty",
             message: "Error severity cannot be empty.");
      }

      if (!TryNormalizeSeverity(
          newSeverity,
          out string normalizedSeverity))
      {
         return Response<ErrorDefinition>.Invalid(
             code: "UnsupportedSeverity",
             message: "Unsupported severity. Allowed values are: Trace, Debug, Information, Warning, Error, Critical.");
      }

      JsonsOptions options =
          WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(inputPath);

      JsonErrorCatalogLoader loader = new();

      Response<ErrorCatalogDocument> loadResponse =
          await loader.LoadFromFileAsync(
              options.ErrorCatalogFilePath,
              cancellationToken);

      if (!loadResponse.IsSuccess || loadResponse.Data is null)
      {
         return Response<ErrorDefinition>.Fail(
             code: "ErrorCatalogLoadFailed",
             message: string.IsNullOrWhiteSpace(loadResponse.Message)
                 ? $"Error catalog could not be loaded: {options.ErrorCatalogFilePath}"
                 : loadResponse.Message);
      }

      ErrorCatalogDocument normalizedDocument =
          new ErrorCatalogDocumentNormalizer().Normalize(loadResponse.Data);

      ErrorDefinition? errorDefinition = FindErrorDefinition(
          normalizedDocument,
          lookupValue);

      if (errorDefinition is null)
      {
         return Response<ErrorDefinition>.NotFound(
             code: "ErrorDefinitionNotFound",
             message: $"No error definition was found for '{lookupValue}'. Search by Id, Code or Name.");
      }

      string oldSeverity = errorDefinition.DefaultSeverity ?? string.Empty;

      errorDefinition.DefaultSeverity = normalizedSeverity;

      ErrorCatalogValidationResult validationResult =
          new ErrorCatalogValidator().Validate(normalizedDocument);

      if (!validationResult.IsValid)
      {
         errorDefinition.DefaultSeverity = oldSeverity;

         return Response<ErrorDefinition>.Invalid(
             code: "EditedErrorCatalogIsInvalid",
             message: "The edited error catalog is invalid and was not saved.");
      }

      JsonCatalogDocumentWriter writer = new();

      Response saveResponse =
          await writer.SaveToFileAsync(
              normalizedDocument,
              options.ErrorCatalogFilePath,
              cancellationToken);

      if (!saveResponse.IsSuccess)
      {
         errorDefinition.DefaultSeverity = oldSeverity;

         string saveFailureCode = saveResponse.Issues.Count > 0
             ? saveResponse.Issues[0].Code
             : "ErrorCatalogSaveFailed";

         string saveFailureMessage = string.IsNullOrWhiteSpace(saveResponse.Message)
             ? "Error catalog could not be saved."
             : saveResponse.Message;

         return Response<ErrorDefinition>.Fail(
             code: saveFailureCode,
             message: saveFailureMessage);
      }

      return Response<ErrorDefinition>.Ok(
          errorDefinition,
          $"Error severity changed from '{oldSeverity}' to '{normalizedSeverity}'.");
   }



   private static bool TryNormalizeSeverity(
       string inputSeverity,
       out string normalizedSeverity)
   {
      normalizedSeverity = string.Empty;

      string trimmedSeverity = inputSeverity.Trim();

      string[] allowedSeverities =
      [
          "Trace",
        "Debug",
        "Information",
        "Warning",
        "Error",
        "Critical"
      ];

      string? matchedSeverity = allowedSeverities.FirstOrDefault(allowedSeverity =>
          string.Equals(
              allowedSeverity,
              trimmedSeverity,
              StringComparison.OrdinalIgnoreCase));

      if (matchedSeverity is null)
      {
         return false;
      }

      normalizedSeverity = matchedSeverity;

      return true;
   }

   private static async Task<Response<ErrorDefinition>> SetErrorTextValueAsync(
       string inputPath,
       string lookupValue,
       string newValue,
       string fieldName,
       string emptyValueCode,
       string emptyValueMessage,
       Func<ErrorDefinition, string?> getCurrentValue,
       Action<ErrorDefinition, string> setNewValue,
       CancellationToken cancellationToken)
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);
      ArgumentException.ThrowIfNullOrWhiteSpace(lookupValue);

      if (string.IsNullOrWhiteSpace(newValue))
      {
         return Response<ErrorDefinition>.Invalid(
             code: emptyValueCode,
             message: emptyValueMessage);
      }

      JsonsOptions options =
          WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(inputPath);

      JsonErrorCatalogLoader loader = new();

      Response<ErrorCatalogDocument> loadResponse =
          await loader.LoadFromFileAsync(
              options.ErrorCatalogFilePath,
              cancellationToken);

      if (!loadResponse.IsSuccess || loadResponse.Data is null)
      {
         return Response<ErrorDefinition>.Fail(
             code: "ErrorCatalogLoadFailed",
             message: string.IsNullOrWhiteSpace(loadResponse.Message)
                 ? $"Error catalog could not be loaded: {options.ErrorCatalogFilePath}"
                 : loadResponse.Message);
      }

      ErrorCatalogDocument normalizedDocument =
          new ErrorCatalogDocumentNormalizer().Normalize(loadResponse.Data);

      ErrorDefinition? errorDefinition = FindErrorDefinition(
          normalizedDocument,
          lookupValue);

      if (errorDefinition is null)
      {
         return Response<ErrorDefinition>.NotFound(
             code: "ErrorDefinitionNotFound",
             message: $"No error definition was found for '{lookupValue}'. Search by Id, Code or Name.");
      }

      string oldValue = getCurrentValue(errorDefinition) ?? string.Empty;
      string trimmedNewValue = newValue.Trim();

      setNewValue(
          errorDefinition,
          trimmedNewValue);

      ErrorCatalogValidationResult validationResult =
          new ErrorCatalogValidator().Validate(normalizedDocument);

      if (!validationResult.IsValid)
      {
         setNewValue(
             errorDefinition,
             oldValue);

         return Response<ErrorDefinition>.Invalid(
             code: "EditedErrorCatalogIsInvalid",
             message: "The edited error catalog is invalid and was not saved.");
      }

      JsonCatalogDocumentWriter writer = new();

      Response saveResponse =
          await writer.SaveToFileAsync(
              normalizedDocument,
              options.ErrorCatalogFilePath,
              cancellationToken);

      if (!saveResponse.IsSuccess)
      {
         setNewValue(
             errorDefinition,
             oldValue);

         string saveFailureCode = saveResponse.Issues.Count > 0
             ? saveResponse.Issues[0].Code
             : "ErrorCatalogSaveFailed";

         string saveFailureMessage = string.IsNullOrWhiteSpace(saveResponse.Message)
             ? "Error catalog could not be saved."
             : saveResponse.Message;

         return Response<ErrorDefinition>.Fail(
             code: saveFailureCode,
             message: saveFailureMessage);
      }

      return Response<ErrorDefinition>.Ok(
          errorDefinition,
          $"Error {fieldName} changed from '{oldValue}' to '{trimmedNewValue}'.");
   }

   private static ErrorDefinition? FindErrorDefinition(
      ErrorCatalogDocument document,
      string lookupValue)
   {
      if (int.TryParse(lookupValue, out int numericCode))
      {
         ErrorDefinition? byCode = document.Errors.FirstOrDefault(errorDefinition =>
            errorDefinition.Code == numericCode);

         if (byCode is not null)
         {
            return byCode;
         }
      }

      return document.Errors.FirstOrDefault(errorDefinition =>
         string.Equals(
            errorDefinition.Id,
            lookupValue,
            StringComparison.OrdinalIgnoreCase)
         || string.Equals(
            errorDefinition.Name,
            lookupValue,
            StringComparison.OrdinalIgnoreCase));
   }
}