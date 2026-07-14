using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;

/// <summary>
/// Provides safe owner editing for error definitions.
/// </summary>
internal static class WhenItFailsWorkspaceEditorOwnerExtensions
{
    /// <summary>
    /// Changes the owner of one error definition and updates its structured id atomically.
    /// </summary>
    public static async Task<Response<ErrorDefinition>> SetOwnerAsync(
        this WhenItFailsWorkspaceEditor editor,
        string inputPath,
        string lookupValue,
        string ownerName,
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

        if (string.IsNullOrWhiteSpace(ownerName))
        {
            return Response<ErrorDefinition>.Invalid(
                code: "OwnerNameIsEmpty",
                message: "Owner name cannot be empty.");
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

        Response<ErrorOwnerCatalogDocument> ownerLoadResponse =
            await new JsonErrorOwnerCatalogLoader().LoadFromFileAsync(
                options.OwnerCatalogFilePath,
                cancellationToken);

        if (!ownerLoadResponse.IsSuccess || ownerLoadResponse.Data is null)
        {
            return Response<ErrorDefinition>.Fail(
                code: "OwnerCatalogLoadFailed",
                message: string.IsNullOrWhiteSpace(ownerLoadResponse.Message)
                    ? $"Owner catalog could not be loaded: {options.OwnerCatalogFilePath}"
                    : ownerLoadResponse.Message);
        }

        ErrorOwnerCatalogDocument ownerCatalog =
            new ErrorOwnerCatalogDocumentNormalizer().Normalize(ownerLoadResponse.Data);
        string normalizedOwnerName = TextKeyNormalizer.NormalizeKey(ownerName);
        ErrorOwnerDefinition? ownerDefinition = ownerCatalog.Owners.FirstOrDefault(owner =>
            string.Equals(owner.Name, normalizedOwnerName, StringComparison.OrdinalIgnoreCase)
            || owner.Aliases.Any(alias => string.Equals(
                alias,
                normalizedOwnerName,
                StringComparison.OrdinalIgnoreCase)));

        if (ownerDefinition is null)
        {
            return Response<ErrorDefinition>.NotFound(
                code: "OwnerNotFound",
                message: $"Owner '{normalizedOwnerName}' was not found.");
        }

        if (string.Equals(errorDefinition.Owner, ownerDefinition.Name, StringComparison.OrdinalIgnoreCase))
        {
            return Response<ErrorDefinition>.Invalid(
                code: "OwnerAlreadySet",
                message: $"Error '{errorDefinition.Id}' already belongs to owner '{ownerDefinition.Name}'.");
        }

        if (errorDefinition.Code < ownerDefinition.CodeFrom
            || errorDefinition.Code > ownerDefinition.CodeTo)
        {
            return Response<ErrorDefinition>.Invalid(
                code: "ErrorCodeOutsideOwnerRange",
                message: $"Error code '{errorDefinition.Code}' is outside owner '{ownerDefinition.Name}' range {ownerDefinition.CodeFrom}-{ownerDefinition.CodeTo}.");
        }

        string normalizedCurrentOwner = TextKeyNormalizer.NormalizeKey(errorDefinition.Owner);
        string normalizedCodePrefix = TextKeyNormalizer.NormalizeKey(errorDefinition.CodePrefix);
        string normalizedCurrentId = TextKeyNormalizer.NormalizeKey(errorDefinition.Id);
        string expectedCurrentPrefix = $"{normalizedCurrentOwner}_{normalizedCodePrefix}_";

        if (!normalizedCurrentId.StartsWith(expectedCurrentPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return Response<ErrorDefinition>.Invalid(
                code: "ErrorIdCannotBeRewrittenSafely",
                message: $"Error id '{errorDefinition.Id}' does not match owner '{errorDefinition.Owner}' and code prefix '{errorDefinition.CodePrefix}'.");
        }

        string idSuffix = normalizedCurrentId[expectedCurrentPrefix.Length..];
        string newId = $"{ownerDefinition.Name}_{normalizedCodePrefix}_{idSuffix}";

        if (errorCatalog.Errors.Any(error =>
                !ReferenceEquals(error, errorDefinition)
                && string.Equals(error.Id, newId, StringComparison.OrdinalIgnoreCase)))
        {
            return Response<ErrorDefinition>.Invalid(
                code: "DuplicateErrorIdAfterOwnerChange",
                message: $"Changing owner would create duplicate error id '{newId}'.");
        }

        string oldOwner = errorDefinition.Owner;
        string oldId = errorDefinition.Id;
        errorDefinition.Owner = ownerDefinition.Name;
        errorDefinition.Id = newId;

        ErrorCatalogValidationResult validationResult =
            new ErrorCatalogValidator().Validate(errorCatalog);

        if (!validationResult.IsValid)
        {
            errorDefinition.Owner = oldOwner;
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
            errorDefinition.Owner = oldOwner;
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
            $"Owner was changed from '{oldOwner}' to '{ownerDefinition.Name}' and error id was updated to '{newId}'.");
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
