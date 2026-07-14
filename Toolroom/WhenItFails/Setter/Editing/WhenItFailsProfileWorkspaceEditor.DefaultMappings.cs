using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;

/// <summary>
/// Provides profile default-mapping editing for <see cref="WhenItFailsProfileWorkspaceEditor"/>.
/// </summary>
internal static class WhenItFailsProfileWorkspaceEditorDefaultMappingExtensions
{
    /// <summary>
    /// Sets one default mapping on an existing profile.
    /// </summary>
    public static async Task<Response<ErrorProfileDefinition>> ProfileSetDefaultMappingAsync(
        this WhenItFailsProfileWorkspaceEditor editor,
        string inputPath,
        string profileName,
        string mappingKey,
        string mappingValue,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editor);
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        Response<ProfileDefaultMappingEditContext> contextResponse = await LoadContextAsync(
            inputPath,
            profileName,
            mappingKey,
            cancellationToken);

        if (!contextResponse.IsSuccess || contextResponse.Data is null)
        {
            return CopyFailure<ErrorProfileDefinition, ProfileDefaultMappingEditContext>(contextResponse);
        }

        if (string.IsNullOrWhiteSpace(mappingValue))
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileMappingValueIsEmpty",
                message: "Profile mapping value cannot be empty.");
        }

        ProfileDefaultMappingEditContext context = contextResponse.Data;
        string normalizedMappingValue = TextKeyNormalizer.NormalizeDisplayName(mappingValue);
        string? existingKey = FindExistingKey(
            context.ProfileDefinition,
            context.NormalizedMappingKey);
        bool mappingExisted = existingKey is not null;
        string? oldValue = mappingExisted
            ? context.ProfileDefinition.DefaultMappings[existingKey!]
            : null;

        if (mappingExisted
            && string.Equals(
                oldValue,
                normalizedMappingValue,
                StringComparison.Ordinal))
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileMappingAlreadySet",
                message: $"Profile '{context.ProfileDefinition.Name}' already maps '{context.NormalizedMappingKey}' to '{normalizedMappingValue}'.");
        }

        if (mappingExisted)
        {
            context.ProfileDefinition.DefaultMappings.Remove(existingKey!);
        }

        context.ProfileDefinition.DefaultMappings[context.NormalizedMappingKey] = normalizedMappingValue;

        Response<ErrorProfileDefinition>? saveFailure = await ValidateAndSaveAsync(
            context,
            rollback: () => RollbackMapping(
                context.ProfileDefinition,
                context.NormalizedMappingKey,
                existingKey,
                oldValue,
                mappingExisted),
            cancellationToken);

        if (saveFailure is not null)
        {
            return saveFailure;
        }

        string action = mappingExisted ? "updated" : "added";

        return Response<ErrorProfileDefinition>.Ok(
            context.ProfileDefinition,
            $"Default mapping '{context.NormalizedMappingKey}' was {action} for profile '{context.ProfileDefinition.Name}'.");
    }

    /// <summary>
    /// Removes one default mapping from an existing profile.
    /// </summary>
    public static async Task<Response<ErrorProfileDefinition>> ProfileRemoveDefaultMappingAsync(
        this WhenItFailsProfileWorkspaceEditor editor,
        string inputPath,
        string profileName,
        string mappingKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editor);
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        Response<ProfileDefaultMappingEditContext> contextResponse = await LoadContextAsync(
            inputPath,
            profileName,
            mappingKey,
            cancellationToken);

        if (!contextResponse.IsSuccess || contextResponse.Data is null)
        {
            return CopyFailure<ErrorProfileDefinition, ProfileDefaultMappingEditContext>(contextResponse);
        }

        ProfileDefaultMappingEditContext context = contextResponse.Data;
        string? existingKey = FindExistingKey(
            context.ProfileDefinition,
            context.NormalizedMappingKey);

        if (existingKey is null)
        {
            return Response<ErrorProfileDefinition>.NotFound(
                code: "ProfileMappingNotFound",
                message: $"Profile '{context.ProfileDefinition.Name}' does not contain default mapping '{context.NormalizedMappingKey}'.");
        }

        string oldValue = context.ProfileDefinition.DefaultMappings[existingKey];
        context.ProfileDefinition.DefaultMappings.Remove(existingKey);

        Response<ErrorProfileDefinition>? saveFailure = await ValidateAndSaveAsync(
            context,
            rollback: () => context.ProfileDefinition.DefaultMappings[existingKey] = oldValue,
            cancellationToken);

        if (saveFailure is not null)
        {
            return saveFailure;
        }

        return Response<ErrorProfileDefinition>.Ok(
            context.ProfileDefinition,
            $"Default mapping '{context.NormalizedMappingKey}' was removed from profile '{context.ProfileDefinition.Name}'.");
    }

    private static async Task<Response<ProfileDefaultMappingEditContext>> LoadContextAsync(
        string inputPath,
        string profileName,
        string mappingKey,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(profileName))
        {
            return Response<ProfileDefaultMappingEditContext>.Invalid(
                code: "ProfileNameIsEmpty",
                message: "Profile name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(mappingKey))
        {
            return Response<ProfileDefaultMappingEditContext>.Invalid(
                code: "ProfileMappingKeyIsEmpty",
                message: "Profile mapping key cannot be empty.");
        }

        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(inputPath);

        Response<ErrorProfileCatalogDocument> loadResponse =
            await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(
                options.ProfilesFilePath,
                cancellationToken);

        if (!loadResponse.IsSuccess || loadResponse.Data is null)
        {
            return Response<ProfileDefaultMappingEditContext>.Fail(
                code: "ProfileCatalogLoadFailed",
                message: string.IsNullOrWhiteSpace(loadResponse.Message)
                    ? $"Profile catalog could not be loaded: {options.ProfilesFilePath}"
                    : loadResponse.Message);
        }

        ErrorProfileCatalogDocument profileCatalog =
            new ErrorProfileCatalogDocumentNormalizer().Normalize(loadResponse.Data);

        ErrorCatalogValidationResult validationResult =
            new ErrorProfileCatalogValidator().Validate(profileCatalog);

        if (!validationResult.IsValid)
        {
            return Response<ProfileDefaultMappingEditContext>.Invalid(
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
            return Response<ProfileDefaultMappingEditContext>.NotFound(
                code: "ProfileNotFound",
                message: $"Profile '{normalizedProfileName}' was not found.");
        }

        string normalizedMappingKey = TextKeyNormalizer.NormalizeKey(mappingKey);

        return Response<ProfileDefaultMappingEditContext>.Ok(
            new ProfileDefaultMappingEditContext(
                options.ProfilesFilePath,
                profileCatalog,
                profileDefinition,
                normalizedMappingKey));
    }

    private static string? FindExistingKey(
        ErrorProfileDefinition profileDefinition,
        string normalizedMappingKey)
    {
        return profileDefinition.DefaultMappings.Keys.FirstOrDefault(key =>
            string.Equals(
                key,
                normalizedMappingKey,
                StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<Response<ErrorProfileDefinition>?> ValidateAndSaveAsync(
        ProfileDefaultMappingEditContext context,
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

    private static void RollbackMapping(
        ErrorProfileDefinition profileDefinition,
        string normalizedMappingKey,
        string? existingKey,
        string? oldValue,
        bool mappingExisted)
    {
        profileDefinition.DefaultMappings.Remove(normalizedMappingKey);

        if (mappingExisted && existingKey is not null)
        {
            profileDefinition.DefaultMappings[existingKey] = oldValue ?? string.Empty;
        }
    }

    private static Response<TTarget> CopyFailure<TTarget, TSource>(Response<TSource> response)
    {
        string code = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "ProfileDefaultMappingEditFailed";

        return Response<TTarget>.Fail(
            code: code,
            message: response.Message);
    }

    private sealed record ProfileDefaultMappingEditContext(
        string ProfileCatalogFilePath,
        ErrorProfileCatalogDocument ProfileCatalog,
        ErrorProfileDefinition ProfileDefinition,
        string NormalizedMappingKey);
}
