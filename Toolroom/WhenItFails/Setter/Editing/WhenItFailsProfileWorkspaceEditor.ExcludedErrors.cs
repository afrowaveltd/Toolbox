using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;

/// <summary>
/// Provides profile excluded-error editing for <see cref="WhenItFailsProfileWorkspaceEditor"/>.
/// </summary>
internal static class WhenItFailsProfileWorkspaceEditorExcludedErrorExtensions
{
    /// <summary>
    /// Adds an existing workspace error to one profile's explicitly excluded errors.
    /// </summary>
    public static async Task<Response<ErrorProfileDefinition>> ProfileAddExcludedErrorAsync(
        this WhenItFailsProfileWorkspaceEditor editor,
        string inputPath,
        string profileName,
        string errorLookup,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editor);

        Response<ProfileExcludedErrorEditContext> contextResponse = await LoadContextAsync(
            inputPath,
            profileName,
            errorLookup,
            cancellationToken);

        if (!contextResponse.IsSuccess || contextResponse.Data is null)
        {
            return CopyFailure<ErrorProfileDefinition, ProfileExcludedErrorEditContext>(contextResponse);
        }

        ProfileExcludedErrorEditContext context = contextResponse.Data;

        bool errorAlreadyExcluded = context.ProfileDefinition.ExcludeErrors.Any(excludedErrorId =>
            string.Equals(
                TextKeyNormalizer.NormalizeKey(excludedErrorId),
                context.CanonicalErrorId,
                StringComparison.OrdinalIgnoreCase));

        if (errorAlreadyExcluded)
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileErrorAlreadyExcluded",
                message: $"Profile '{context.ProfileDefinition.Name}' already excludes error '{context.CanonicalErrorId}'.");
        }

        context.ProfileDefinition.ExcludeErrors.Add(context.CanonicalErrorId);

        Response<ErrorProfileDefinition>? saveFailure = await ValidateAndSaveAsync(
            context,
            rollback: () => context.ProfileDefinition.ExcludeErrors.Remove(context.CanonicalErrorId),
            cancellationToken);

        if (saveFailure is not null)
        {
            return saveFailure;
        }

        return Response<ErrorProfileDefinition>.Ok(
            context.ProfileDefinition,
            $"Error '{context.CanonicalErrorId}' was added to excluded errors for profile '{context.ProfileDefinition.Name}'.");
    }

    /// <summary>
    /// Removes an existing workspace error from one profile's explicitly excluded errors.
    /// </summary>
    public static async Task<Response<ErrorProfileDefinition>> ProfileRemoveExcludedErrorAsync(
        this WhenItFailsProfileWorkspaceEditor editor,
        string inputPath,
        string profileName,
        string errorLookup,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editor);

        Response<ProfileExcludedErrorEditContext> contextResponse = await LoadContextAsync(
            inputPath,
            profileName,
            errorLookup,
            cancellationToken);

        if (!contextResponse.IsSuccess || contextResponse.Data is null)
        {
            return CopyFailure<ErrorProfileDefinition, ProfileExcludedErrorEditContext>(contextResponse);
        }

        ProfileExcludedErrorEditContext context = contextResponse.Data;
        int excludedErrorIndex = context.ProfileDefinition.ExcludeErrors.FindIndex(excludedErrorId =>
            string.Equals(
                TextKeyNormalizer.NormalizeKey(excludedErrorId),
                context.CanonicalErrorId,
                StringComparison.OrdinalIgnoreCase));

        if (excludedErrorIndex < 0)
        {
            return Response<ErrorProfileDefinition>.NotFound(
                code: "ProfileErrorNotExcluded",
                message: $"Profile '{context.ProfileDefinition.Name}' does not exclude error '{context.CanonicalErrorId}'.");
        }

        string removedErrorId = context.ProfileDefinition.ExcludeErrors[excludedErrorIndex];
        context.ProfileDefinition.ExcludeErrors.RemoveAt(excludedErrorIndex);

        Response<ErrorProfileDefinition>? saveFailure = await ValidateAndSaveAsync(
            context,
            rollback: () => context.ProfileDefinition.ExcludeErrors.Insert(excludedErrorIndex, removedErrorId),
            cancellationToken);

        if (saveFailure is not null)
        {
            return saveFailure;
        }

        return Response<ErrorProfileDefinition>.Ok(
            context.ProfileDefinition,
            $"Error '{context.CanonicalErrorId}' was removed from excluded errors for profile '{context.ProfileDefinition.Name}'.");
    }

    private static async Task<Response<ProfileExcludedErrorEditContext>> LoadContextAsync(
        string inputPath,
        string profileName,
        string errorLookup,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        if (string.IsNullOrWhiteSpace(profileName))
        {
            return Response<ProfileExcludedErrorEditContext>.Invalid(
                code: "ProfileNameIsEmpty",
                message: "Profile name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(errorLookup))
        {
            return Response<ProfileExcludedErrorEditContext>.Invalid(
                code: "ErrorLookupIsEmpty",
                message: "Error id, code, or name cannot be empty.");
        }

        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(inputPath);

        Response<ErrorProfileCatalogDocument> profileLoadResponse =
            await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(
                options.ProfilesFilePath,
                cancellationToken);

        if (!profileLoadResponse.IsSuccess || profileLoadResponse.Data is null)
        {
            return Response<ProfileExcludedErrorEditContext>.Fail(
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
            return Response<ProfileExcludedErrorEditContext>.Invalid(
                code: "ProfileCatalogIsInvalid",
                message: "The profile catalog is invalid and cannot be edited safely.");
        }

        string normalizedProfileName = TextKeyNormalizer.NormalizeKey(profileName);
        ErrorProfileDefinition? profileDefinition = profileCatalog.Profiles.FirstOrDefault(profile =>
            string.Equals(
                profile.Name,
                normalizedProfileName,
                StringComparison.OrdinalIgnoreCase));

        if (profileDefinition is null)
        {
            return Response<ProfileExcludedErrorEditContext>.NotFound(
                code: "ProfileNotFound",
                message: $"Profile '{normalizedProfileName}' was not found.");
        }

        Response<ErrorCatalogDocument> errorLoadResponse =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                options.ErrorCatalogFilePath,
                cancellationToken);

        if (!errorLoadResponse.IsSuccess || errorLoadResponse.Data is null)
        {
            return Response<ProfileExcludedErrorEditContext>.Fail(
                code: "ErrorCatalogLoadFailed",
                message: string.IsNullOrWhiteSpace(errorLoadResponse.Message)
                    ? $"Error catalog could not be loaded: {options.ErrorCatalogFilePath}"
                    : errorLoadResponse.Message);
        }

        ErrorCatalogDocument errorCatalog =
            new ErrorCatalogDocumentNormalizer().Normalize(errorLoadResponse.Data);

        ErrorDefinition? errorDefinition = FindErrorDefinition(errorCatalog, errorLookup);

        if (errorDefinition is null)
        {
            return Response<ProfileExcludedErrorEditContext>.NotFound(
                code: "ErrorDefinitionNotFound",
                message: $"No error definition was found for '{errorLookup}'. Search by id, code, or name.");
        }

        string canonicalErrorId = TextKeyNormalizer.NormalizeKey(errorDefinition.Id);

        return Response<ProfileExcludedErrorEditContext>.Ok(
            new ProfileExcludedErrorEditContext(
                options.ProfilesFilePath,
                profileCatalog,
                profileDefinition,
                canonicalErrorId));
    }

    private static ErrorDefinition? FindErrorDefinition(
        ErrorCatalogDocument errorCatalog,
        string lookupValue)
    {
        string trimmedLookupValue = lookupValue.Trim();

        if (int.TryParse(trimmedLookupValue, out int numericCode))
        {
            ErrorDefinition? byCode = errorCatalog.Errors.FirstOrDefault(error =>
                error.Code == numericCode);

            if (byCode is not null)
            {
                return byCode;
            }
        }

        return errorCatalog.Errors.FirstOrDefault(error =>
            string.Equals(
                error.Id,
                trimmedLookupValue,
                StringComparison.OrdinalIgnoreCase)
            || string.Equals(
                error.Name,
                trimmedLookupValue,
                StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<Response<ErrorProfileDefinition>?> ValidateAndSaveAsync(
        ProfileExcludedErrorEditContext context,
        Action rollback,
        CancellationToken cancellationToken)
    {
        ErrorCatalogValidationResult editedValidationResult =
            new ErrorProfileCatalogValidator().Validate(context.ProfileCatalog);

        if (!editedValidationResult.IsValid)
        {
            rollback();

            return Response<ErrorProfileDefinition>.Invalid(
                code: "EditedProfileCatalogIsInvalid",
                message: "The edited profile catalog is invalid and was not saved.");
        }

        Response saveResponse = await new JsonCatalogDocumentWriter().SaveToFileAsync(
            context.ProfileCatalog,
            context.ProfileCatalogFilePath,
            cancellationToken);

        if (saveResponse.IsSuccess)
        {
            return null;
        }

        rollback();

        string saveFailureCode = saveResponse.Issues.Count > 0
            ? saveResponse.Issues[0].Code
            : "ProfileCatalogSaveFailed";

        string saveFailureMessage = string.IsNullOrWhiteSpace(saveResponse.Message)
            ? "Profile catalog could not be saved."
            : saveResponse.Message;

        return Response<ErrorProfileDefinition>.Fail(
            code: saveFailureCode,
            message: saveFailureMessage);
    }

    private static Response<TTarget> CopyFailure<TTarget, TSource>(Response<TSource> response)
    {
        string code = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "ProfileExcludedErrorEditFailed";

        return Response<TTarget>.Fail(
            code: code,
            message: response.Message);
    }

    private sealed record ProfileExcludedErrorEditContext(
        string ProfileCatalogFilePath,
        ErrorProfileCatalogDocument ProfileCatalog,
        ErrorProfileDefinition ProfileDefinition,
        string CanonicalErrorId);
}
