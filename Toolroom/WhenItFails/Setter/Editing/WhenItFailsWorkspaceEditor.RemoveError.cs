using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;

/// <summary>
/// Provides safe removal of error definitions.
/// </summary>
internal static class WhenItFailsWorkspaceEditorRemoveErrorExtensions
{
    /// <summary>
    /// Removes one error definition when no profile explicitly references it.
    /// </summary>
    public static async Task<Response<ErrorDefinition>> RemoveErrorAsync(
        this WhenItFailsWorkspaceEditor editor,
        string inputPath,
        string lookupValue,
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
        ErrorCatalogValidationResult errorValidationResult =
            new ErrorCatalogValidator().Validate(errorCatalog);
        if (!errorValidationResult.IsValid)
        {
            return Response<ErrorDefinition>.Invalid(
                code: "ErrorCatalogIsInvalid",
                message: "The error catalog is invalid and cannot be edited safely.");
        }

        ErrorDefinition? errorDefinition = FindErrorDefinition(errorCatalog, lookupValue.Trim());
        if (errorDefinition is null)
        {
            return Response<ErrorDefinition>.NotFound(
                code: "ErrorDefinitionNotFound",
                message: $"No error definition was found for '{lookupValue}'. Search by id, code, or name.");
        }

        Response<ErrorProfileCatalogDocument> profileLoadResponse =
            await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(
                options.ProfilesFilePath,
                cancellationToken);
        if (!profileLoadResponse.IsSuccess || profileLoadResponse.Data is null)
        {
            return Response<ErrorDefinition>.Fail(
                code: "ProfileCatalogLoadFailed",
                message: string.IsNullOrWhiteSpace(profileLoadResponse.Message)
                    ? $"Profile catalog could not be loaded: {options.ProfilesFilePath}"
                    : profileLoadResponse.Message);
        }

        ErrorProfileCatalogDocument profileCatalog =
            new ErrorProfileCatalogDocumentNormalizer().Normalize(profileLoadResponse.Data);
        ErrorCatalogValidationResult profileValidationResult =
            new ErrorProfileCatalogValidator().Validate(profileCatalog);
        if (!profileValidationResult.IsValid)
        {
            return Response<ErrorDefinition>.Invalid(
                code: "ProfileCatalogIsInvalid",
                message: "The profile catalog is invalid and the error cannot be removed safely.");
        }

        string normalizedErrorId = TextKeyNormalizer.NormalizeKey(errorDefinition.Id);
        IReadOnlyList<string> referencingProfiles = profileCatalog.Profiles
            .Where(profile => ReferencesError(profile, normalizedErrorId))
            .Select(profile => profile.Name)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (referencingProfiles.Count > 0)
        {
            return Response<ErrorDefinition>.Invalid(
                code: "ErrorIsReferencedByProfiles",
                message: $"Error '{errorDefinition.Id}' is explicitly referenced by profile(s): {string.Join(", ", referencingProfiles)}.");
        }

        int errorIndex = errorCatalog.Errors.IndexOf(errorDefinition);
        errorCatalog.Errors.RemoveAt(errorIndex);

        ErrorCatalogValidationResult editedValidationResult =
            new ErrorCatalogValidator().Validate(errorCatalog);
        if (!editedValidationResult.IsValid)
        {
            errorCatalog.Errors.Insert(errorIndex, errorDefinition);
            return Response<ErrorDefinition>.Invalid(
                code: "EditedErrorCatalogIsInvalid",
                message: "The error catalog without the selected definition is invalid and was not saved.");
        }

        Response saveResponse = await new JsonCatalogDocumentWriter().SaveToFileAsync(
            errorCatalog,
            options.ErrorCatalogFilePath,
            cancellationToken);
        if (!saveResponse.IsSuccess)
        {
            errorCatalog.Errors.Insert(errorIndex, errorDefinition);
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
            $"Removed error '{errorDefinition.Id}' ({errorDefinition.Code}).");
    }

    private static bool ReferencesError(
        ErrorProfileDefinition profile,
        string normalizedErrorId) =>
        profile.IncludeErrors.Any(value => string.Equals(
            TextKeyNormalizer.NormalizeKey(value),
            normalizedErrorId,
            StringComparison.OrdinalIgnoreCase))
        || profile.ExcludeErrors.Any(value => string.Equals(
            TextKeyNormalizer.NormalizeKey(value),
            normalizedErrorId,
            StringComparison.OrdinalIgnoreCase));

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
