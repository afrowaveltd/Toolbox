using Afrowave.Toolbox.Essentials.Results;
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
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(lookupValue);

        if (string.IsNullOrWhiteSpace(newTitle))
        {
            return Response<ErrorDefinition>.Invalid(
               code: "TitleIsEmpty",
               message: "Error title cannot be empty.");
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

        string oldTitle = errorDefinition.Title;
        errorDefinition.Title = newTitle.Trim();

        ErrorCatalogValidationResult validationResult =
           new ErrorCatalogValidator().Validate(normalizedDocument);

        if (!validationResult.IsValid)
        {
            errorDefinition.Title = oldTitle;

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
            errorDefinition.Title = oldTitle;

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
           $"Error title changed from '{oldTitle}' to '{errorDefinition.Title}'.");
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