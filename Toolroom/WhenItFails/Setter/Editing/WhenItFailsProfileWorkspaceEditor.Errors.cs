using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;

/// <summary>
/// Provides profile included-error editing for <see cref="WhenItFailsProfileWorkspaceEditor"/>.
/// </summary>
internal static class WhenItFailsProfileWorkspaceEditorErrorExtensions
{
    /// <summary>
    /// Adds an existing workspace error to one profile's explicitly included errors.
    /// </summary>
    public static async Task<Response<ErrorProfileDefinition>> ProfileAddErrorAsync(
        this WhenItFailsProfileWorkspaceEditor editor,
        string inputPath,
        string profileName,
        string errorLookup,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editor);

        Response<ProfileErrorEditContext> contextResponse = await LoadContextAsync(
            inputPath,
            profileName,
            errorLookup,
            cancellationToken);

        if (!contextResponse.IsSuccess || contextResponse.Data is null)
        {
            return CopyFailure<ErrorProfileDefinition, ProfileErrorEditContext>(contextResponse);
        }

        ProfileErrorEditContext context = contextResponse.Data;

        bool errorAlreadyIncluded = context.ProfileDefinition.IncludeErrors.Any(includedErrorId =>
            string.Equals(
                TextKeyNormalizer.NormalizeKey(includedErrorId),
                context.CanonicalErrorId,
                StringComparison.OrdinalIgnoreCase));

        if (errorAlreadyIncluded)
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileErrorAlreadyIncluded",
                message: $"Profile '{context.ProfileDefinition.Name}' already includes error '{context.CanonicalErrorId}'.");
        }

        context.ProfileDefinition.IncludeErrors.Add(context.CanonicalErrorId);

        Response<ErrorProfileDefinition>? saveFailure = await ValidateAndSaveAsync(
            context,
            rollback: () => context.ProfileDefinition.IncludeErrors.Remove(context.CanonicalErrorId),
            cancellationToken);

        if (saveFailure is not null)
        {
            return saveFailure;
        }

        return Response<ErrorProfileDefinition>.Ok(
            context.ProfileDefinition,
            $"Error '{context.CanonicalErrorId}' was added to profile '{context.ProfileDefinition.Name}'.");
    }

    /// <summary>
    /// Removes an existing workspace error from one profile's explicitly included errors.
    /// </summary>
    public static async Task<Response<ErrorProfileDefinition>> ProfileRemoveErrorAsync(
        this WhenItFailsProfileWorkspaceEditor editor,
        string inputPath,
        string profileName,
        string errorLookup,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editor);

        Response<ProfileErrorEditContext> contextResponse = await LoadContextAsync(
            inputPath,
            profileName,
            errorLookup,
            cancellationToken);

        if (!contextResponse.IsSuccess || contextResponse.Data is null)
        {
            return CopyFailure<ErrorProfileDefinition, ProfileErrorEditContext>(contextResponse);
        }

        ProfileErrorEditContext context = contextResponse.Data;

        int includedErrorIndex = context.ProfileDefinition.IncludeErrors.FindIndex(includedErrorId =>
            string.Equals(
                TextKeyNormalizer.NormalizeKey(includedErrorId),
                context.CanonicalErrorId,
                StringComparison.OrdinalIgnoreCase));

        if (includedErrorIndex < 0)
        {
            return Response<ErrorProfileDefinition>.NotFound(
                code: "ProfileErrorNotIncluded",
                message: $"Profile '{context.ProfileDefinition.Name}' does not include error '{context.CanonicalErrorId}'.");
        }

        string removedErrorId = context.ProfileDefinition.IncludeErrors[includedErrorIndex];
        context.ProfileDefinition.IncludeErrors.RemoveAt(includedErrorIndex);

        Response<ErrorProfileDefinition>? saveFailure = await ValidateAndSaveAsync(
            context,
            rollback: () => context.ProfileDefinition.IncludeErrors.Insert(
                includedErrorIndex,
                removedErrorId),
            cancellationToken);

        if (saveFailure is not null)
        {
            return saveFailure;
        }

        return Response<ErrorProfileDefinition>.Ok(
            context.ProfileDefinition,
            $"Error '{context.CanonicalErrorId}' was removed from profile '{context.ProfileDefinition.Name}'.");
    }

    private static async Task<Response<ProfileErrorEditContext>> LoadContextAsync(
        string inputPath,
        string profileName,
        string errorLookup,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        if (string.IsNullOrWhiteSpace(profileName))
        {
            return Response<ProfileErrorEditContext>.Invalid(
                code: "ProfileNameIsEmpty",
                message: "Profile name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(errorLookup))
        {
            return Response<ProfileErrorEditContext>.Invalid(
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
            return Response<ProfileErrorEditContext>.Fail(
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
            return Response<ProfileErrorEditContext>.Invalid(
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
            return Response<ProfileErrorEditContext>.NotFound(
                code: "ProfileNotFound",
                message: $"Profile '{normalizedProfileName}' was not found.");
        }

        Response<ErrorCatalogDocument> errorLoadResponse =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                options.ErrorCatalogFilePath,
                cancellationToken);

        if (!errorLoadResponse.IsSuccess || errorLoadResponse.Data is null)
        {
            return Response<ProfileErrorEditContext>.Fail(
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
            return Response<ProfileErrorEditContext>.NotFound(
                code: "ErrorDefinitionNotFound",
                message: $"No error definition was found for '{errorLookup}'. Search by id, code, or name.");
        }

        string canonicalErrorId = TextKeyNormalizer.NormalizeKey(errorDefinition.Id);

        return Response<ProfileErrorEditContext>.Ok(
            new ProfileErrorEditContext(
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
        ProfileErrorEditContext context,
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
            : "ProfileErrorEditFailed";

        return Response<TTarget>.Fail(
            code: code,
            message: response.Message);
    }

    private sealed record ProfileErrorEditContext(
        string ProfileCatalogFilePath,
        ErrorProfileCatalogDocument ProfileCatalog,
        ErrorProfileDefinition ProfileDefinition,
        string CanonicalErrorId);
}
