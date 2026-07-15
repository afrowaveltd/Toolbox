using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;

/// <summary>
/// Provides safe code group editing for error definitions.
/// </summary>
internal static class WhenItFailsWorkspaceEditorCodeGroupExtensions
{
    /// <summary>
    /// Changes the code group of one error definition and updates its code prefix and structured id atomically.
    /// </summary>
    public static async Task<Response<ErrorDefinition>> SetCodeGroupAsync(
        this WhenItFailsWorkspaceEditor editor,
        string inputPath,
        string lookupValue,
        string codeGroupName,
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

        if (string.IsNullOrWhiteSpace(codeGroupName))
        {
            return Response<ErrorDefinition>.Invalid(
                code: "CodeGroupNameIsEmpty",
                message: "Code group name or prefix cannot be empty.");
        }

        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(inputPath);

        Response<ErrorCatalogDocument> errorLoadResponse =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                options.ErrorCatalogFilePath,
                cancellationToken);

        if (!errorLoadResponse.IsSuccess || errorLoadResponse.Data is null)
        {
            return Response<ErrorDefinition>.Fail(
                code: "ErrorCatalogLoadFailed",
                message: string.IsNullOrWhiteSpace(errorLoadResponse.Message)
                    ? $"Error catalog could not be loaded: {options.ErrorCatalogFilePath}"
                    : errorLoadResponse.Message);
        }

        ErrorCatalogDocument errorCatalog =
            new ErrorCatalogDocumentNormalizer().Normalize(errorLoadResponse.Data);

        ErrorDefinition? errorDefinition = FindErrorDefinition(errorCatalog, lookupValue.Trim());
        if (errorDefinition is null)
        {
            return Response<ErrorDefinition>.NotFound(
                code: "ErrorDefinitionNotFound",
                message: $"No error definition was found for '{lookupValue}'. Search by Id, Code or Name.");
        }

        Response<ErrorCodeGroupCatalogDocument> codeGroupLoadResponse =
            await new JsonErrorCodeGroupCatalogLoader().LoadFromFileAsync(
                options.CodeGroupCatalogFilePath,
                cancellationToken);

        if (!codeGroupLoadResponse.IsSuccess || codeGroupLoadResponse.Data is null)
        {
            return Response<ErrorDefinition>.Fail(
                code: "CodeGroupCatalogLoadFailed",
                message: string.IsNullOrWhiteSpace(codeGroupLoadResponse.Message)
                    ? $"Code group catalog could not be loaded: {options.CodeGroupCatalogFilePath}"
                    : codeGroupLoadResponse.Message);
        }

        ErrorCodeGroupCatalogDocument codeGroupCatalog =
            new ErrorCodeGroupCatalogDocumentNormalizer().Normalize(codeGroupLoadResponse.Data);

        ErrorCatalogValidationResult codeGroupValidationResult =
            new ErrorCodeGroupCatalogValidator().Validate(codeGroupCatalog);
        if (!codeGroupValidationResult.IsValid)
        {
            return Response<ErrorDefinition>.Invalid(
                code: "CodeGroupCatalogIsInvalid",
                message: "The code group catalog is invalid and cannot be used safely.");
        }

        string normalizedCodeGroupName = TextKeyNormalizer.NormalizeKey(codeGroupName);
        ErrorCodeGroupDefinition? codeGroupDefinition = codeGroupCatalog.CodeGroups.FirstOrDefault(group =>
            string.Equals(group.Name, normalizedCodeGroupName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(group.CodePrefix, normalizedCodeGroupName, StringComparison.OrdinalIgnoreCase));

        if (codeGroupDefinition is null)
        {
            return Response<ErrorDefinition>.NotFound(
                code: "CodeGroupNotFound",
                message: $"Code group '{normalizedCodeGroupName}' was not found.");
        }

        if (string.Equals(errorDefinition.CodeGroup, codeGroupDefinition.Name, StringComparison.OrdinalIgnoreCase)
            && string.Equals(errorDefinition.CodePrefix, codeGroupDefinition.CodePrefix, StringComparison.OrdinalIgnoreCase))
        {
            return Response<ErrorDefinition>.Invalid(
                code: "CodeGroupAlreadySet",
                message: $"Error '{errorDefinition.Id}' already belongs to code group '{codeGroupDefinition.Name}'.");
        }

        if (errorDefinition.Code < codeGroupDefinition.CodeFrom
            || errorDefinition.Code > codeGroupDefinition.CodeTo)
        {
            return Response<ErrorDefinition>.Invalid(
                code: "ErrorCodeOutsideCodeGroupRange",
                message: $"Error code '{errorDefinition.Code}' is outside code group '{codeGroupDefinition.Name}' range {codeGroupDefinition.CodeFrom}-{codeGroupDefinition.CodeTo}.");
        }

        string normalizedOwner = TextKeyNormalizer.NormalizeKey(errorDefinition.Owner);
        string normalizedCurrentPrefix = TextKeyNormalizer.NormalizeKey(errorDefinition.CodePrefix);
        string normalizedCurrentId = TextKeyNormalizer.NormalizeKey(errorDefinition.Id);
        string expectedCurrentPrefix = $"{normalizedOwner}_{normalizedCurrentPrefix}_";

        if (!normalizedCurrentId.StartsWith(expectedCurrentPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return Response<ErrorDefinition>.Invalid(
                code: "ErrorIdCannotBeRewrittenSafely",
                message: $"Error id '{errorDefinition.Id}' does not match owner '{errorDefinition.Owner}' and code prefix '{errorDefinition.CodePrefix}'.");
        }

        string idSuffix = normalizedCurrentId[expectedCurrentPrefix.Length..];
        string newCodePrefix = TextKeyNormalizer.NormalizeKey(codeGroupDefinition.CodePrefix);
        string newId = $"{normalizedOwner}_{newCodePrefix}_{idSuffix}";

        if (errorCatalog.Errors.Any(error =>
                !ReferenceEquals(error, errorDefinition)
                && string.Equals(error.Id, newId, StringComparison.OrdinalIgnoreCase)))
        {
            return Response<ErrorDefinition>.Invalid(
                code: "DuplicateErrorIdAfterCodeGroupChange",
                message: $"Changing code group would create duplicate error id '{newId}'.");
        }

        string oldCodeGroup = errorDefinition.CodeGroup;
        string oldCodePrefix = errorDefinition.CodePrefix;
        string oldId = errorDefinition.Id;

        errorDefinition.CodeGroup = codeGroupDefinition.Name;
        errorDefinition.CodePrefix = newCodePrefix;
        errorDefinition.Id = newId;

        ErrorCatalogValidationResult validationResult =
            new ErrorCatalogValidator().Validate(errorCatalog);

        if (!validationResult.IsValid)
        {
            errorDefinition.CodeGroup = oldCodeGroup;
            errorDefinition.CodePrefix = oldCodePrefix;
            errorDefinition.Id = oldId;
            return Response<ErrorDefinition>.Invalid(
                code: "EditedErrorCatalogIsInvalid",
                message: "The edited error catalog is invalid and was not saved.");
        }

        Response saveResponse = await new JsonCatalogDocumentWriter().SaveToFileAsync(
            errorCatalog,
            options.ErrorCatalogFilePath,
            cancellationToken);

        if (!saveResponse.IsSuccess)
        {
            errorDefinition.CodeGroup = oldCodeGroup;
            errorDefinition.CodePrefix = oldCodePrefix;
            errorDefinition.Id = oldId;
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
            $"Code group was changed from '{oldCodeGroup}' to '{codeGroupDefinition.Name}', code prefix was changed to '{newCodePrefix}', and error id was updated to '{newId}'.");
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
