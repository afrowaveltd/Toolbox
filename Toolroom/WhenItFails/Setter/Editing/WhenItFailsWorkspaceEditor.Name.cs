using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;

/// <summary>
/// Provides machine-friendly name editing for error definitions.
/// </summary>
internal static class WhenItFailsWorkspaceEditorNameExtensions
{
    /// <summary>
    /// Changes the normalized machine-friendly name of one existing error definition.
    /// </summary>
    public static async Task<Response<ErrorDefinition>> SetNameAsync(
        this WhenItFailsWorkspaceEditor editor,
        string inputPath,
        string lookupValue,
        string newName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editor);
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        if (string.IsNullOrWhiteSpace(lookupValue))
        {
            return Response<ErrorDefinition>.Invalid(
                code: "ErrorLookupIsEmpty",
                message: "Error id, code, or name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(newName))
        {
            return Response<ErrorDefinition>.Invalid(
                code: "ErrorNameIsEmpty",
                message: "Error name cannot be empty.");
        }

        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(inputPath);
        Response<ErrorCatalogDocument> loadResponse =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
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

        ErrorCatalogDocument catalog =
            new ErrorCatalogDocumentNormalizer().Normalize(loadResponse.Data);
        ErrorCatalogValidationResult currentValidation =
            new ErrorCatalogValidator().Validate(catalog);

        if (!currentValidation.IsValid)
        {
            return Response<ErrorDefinition>.Invalid(
                code: "ErrorCatalogIsInvalid",
                message: "The error catalog is invalid and cannot be edited safely.");
        }

        ErrorDefinition? errorDefinition = FindErrorDefinition(catalog, lookupValue.Trim());
        if (errorDefinition is null)
        {
            return Response<ErrorDefinition>.NotFound(
                code: "ErrorDefinitionNotFound",
                message: $"No error definition was found for '{lookupValue}'. Search by id, code, or name.");
        }

        string normalizedName = TextKeyNormalizer.NormalizeKey(newName);
        if (string.Equals(errorDefinition.Name, normalizedName, StringComparison.OrdinalIgnoreCase))
        {
            return Response<ErrorDefinition>.Invalid(
                code: "ErrorNameAlreadySet",
                message: $"Error '{errorDefinition.Id}' already has name '{normalizedName}'.");
        }

        bool nameExists = catalog.Errors.Any(error =>
            !ReferenceEquals(error, errorDefinition)
            && string.Equals(error.Name, normalizedName, StringComparison.OrdinalIgnoreCase));

        if (nameExists)
        {
            return Response<ErrorDefinition>.Invalid(
                code: "ErrorNameAlreadyExists",
                message: $"An error named '{normalizedName}' already exists.");
        }

        string oldName = errorDefinition.Name;
        errorDefinition.Name = normalizedName;

        ErrorCatalogValidationResult editedValidation =
            new ErrorCatalogValidator().Validate(catalog);
        if (!editedValidation.IsValid)
        {
            errorDefinition.Name = oldName;
            return Response<ErrorDefinition>.Invalid(
                code: "EditedErrorCatalogIsInvalid",
                message: "The edited error catalog is invalid and was not saved.");
        }

        Response saveResponse = await new JsonCatalogDocumentWriter().SaveToFileAsync(
            catalog,
            options.ErrorCatalogFilePath,
            cancellationToken);

        if (!saveResponse.IsSuccess)
        {
            errorDefinition.Name = oldName;
            return Response<ErrorDefinition>.Fail(
                code: saveResponse.Issues.Count > 0
                    ? saveResponse.Issues[0].Code
                    : "ErrorCatalogSaveFailed",
                message: string.IsNullOrWhiteSpace(saveResponse.Message)
                    ? "Error catalog could not be saved."
                    : saveResponse.Message);
        }

        return Response<ErrorDefinition>.Ok(
            errorDefinition,
            $"Error '{errorDefinition.Id}' name changed from '{oldName}' to '{normalizedName}'.");
    }

    private static ErrorDefinition? FindErrorDefinition(
        ErrorCatalogDocument catalog,
        string lookupValue)
    {
        if (int.TryParse(lookupValue, out int numericCode))
        {
            ErrorDefinition? byCode = catalog.Errors.FirstOrDefault(error => error.Code == numericCode);
            if (byCode is not null)
            {
                return byCode;
            }
        }

        return catalog.Errors.FirstOrDefault(error =>
            string.Equals(error.Id, lookupValue, StringComparison.OrdinalIgnoreCase)
            || string.Equals(error.Name, lookupValue, StringComparison.OrdinalIgnoreCase));
    }
}
