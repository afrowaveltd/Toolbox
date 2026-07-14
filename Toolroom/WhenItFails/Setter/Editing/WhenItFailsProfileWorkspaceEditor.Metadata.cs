using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;

/// <summary>
/// Provides profile metadata editing for <see cref="WhenItFailsProfileWorkspaceEditor"/>.
/// </summary>
internal static class WhenItFailsProfileWorkspaceEditorMetadataExtensions
{
    /// <summary>
    /// Adds or updates one metadata value on an existing profile.
    /// </summary>
    public static async Task<Response<ErrorProfileDefinition>> ProfileSetMetadataAsync(
        this WhenItFailsProfileWorkspaceEditor editor,
        string inputPath,
        string profileName,
        string metadataKey,
        string metadataValue,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editor);
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        if (string.IsNullOrWhiteSpace(profileName))
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileNameIsEmpty",
                message: "Profile name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(metadataKey))
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileMetadataKeyIsEmpty",
                message: "Profile metadata key cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(metadataValue))
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileMetadataValueIsEmpty",
                message: "Profile metadata value cannot be empty.");
        }

        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(inputPath);

        Response<ErrorProfileCatalogDocument> loadResponse =
            await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(
                options.ProfilesFilePath,
                cancellationToken);

        if (!loadResponse.IsSuccess || loadResponse.Data is null)
        {
            return Response<ErrorProfileDefinition>.Fail(
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
            return Response<ErrorProfileDefinition>.Invalid(
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
            return Response<ErrorProfileDefinition>.NotFound(
                code: "ProfileNotFound",
                message: $"Profile '{normalizedProfileName}' was not found.");
        }

        string normalizedMetadataKey = TextKeyNormalizer.NormalizeKey(metadataKey);
        string normalizedMetadataValue = TextKeyNormalizer.NormalizeDisplayName(metadataValue);

        bool metadataExisted = profileDefinition.Metadata.TryGet(
            normalizedMetadataKey,
            out string? oldValue);

        if (metadataExisted
            && string.Equals(
                oldValue,
                normalizedMetadataValue,
                StringComparison.Ordinal))
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileMetadataAlreadySet",
                message: $"Profile '{profileDefinition.Name}' already has metadata '{normalizedMetadataKey}' set to '{normalizedMetadataValue}'.");
        }

        profileDefinition.Metadata.Set(
            normalizedMetadataKey,
            normalizedMetadataValue);

        ErrorCatalogValidationResult editedValidationResult =
            new ErrorProfileCatalogValidator().Validate(profileCatalog);

        if (!editedValidationResult.IsValid)
        {
            RollbackMetadata(
                profileDefinition,
                normalizedMetadataKey,
                oldValue,
                metadataExisted);

            return Response<ErrorProfileDefinition>.Invalid(
                code: "EditedProfileCatalogIsInvalid",
                message: "The edited profile catalog is invalid and was not saved.");
        }

        Response saveResponse = await new JsonCatalogDocumentWriter().SaveToFileAsync(
            profileCatalog,
            options.ProfilesFilePath,
            cancellationToken);

        if (!saveResponse.IsSuccess)
        {
            RollbackMetadata(
                profileDefinition,
                normalizedMetadataKey,
                oldValue,
                metadataExisted);

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

        string action = metadataExisted ? "updated" : "added";

        return Response<ErrorProfileDefinition>.Ok(
            profileDefinition,
            $"Metadata '{normalizedMetadataKey}' was {action} for profile '{profileDefinition.Name}'.");
    }

    /// <summary>
    /// Removes one metadata value from an existing profile.
    /// </summary>
    public static async Task<Response<ErrorProfileDefinition>> ProfileRemoveMetadataAsync(
        this WhenItFailsProfileWorkspaceEditor editor,
        string inputPath,
        string profileName,
        string metadataKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editor);
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        if (string.IsNullOrWhiteSpace(profileName))
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileNameIsEmpty",
                message: "Profile name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(metadataKey))
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileMetadataKeyIsEmpty",
                message: "Profile metadata key cannot be empty.");
        }

        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(inputPath);

        Response<ErrorProfileCatalogDocument> loadResponse =
            await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(
                options.ProfilesFilePath,
                cancellationToken);

        if (!loadResponse.IsSuccess || loadResponse.Data is null)
        {
            return Response<ErrorProfileDefinition>.Fail(
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
            return Response<ErrorProfileDefinition>.Invalid(
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
            return Response<ErrorProfileDefinition>.NotFound(
                code: "ProfileNotFound",
                message: $"Profile '{normalizedProfileName}' was not found.");
        }

        string normalizedMetadataKey = TextKeyNormalizer.NormalizeKey(metadataKey);

        if (!profileDefinition.Metadata.TryGet(
                normalizedMetadataKey,
                out string? removedValue))
        {
            return Response<ErrorProfileDefinition>.NotFound(
                code: "ProfileMetadataNotFound",
                message: $"Profile '{profileDefinition.Name}' does not contain metadata '{normalizedMetadataKey}'.");
        }

        profileDefinition.Metadata.Remove(normalizedMetadataKey);

        ErrorCatalogValidationResult editedValidationResult =
            new ErrorProfileCatalogValidator().Validate(profileCatalog);

        if (!editedValidationResult.IsValid)
        {
            profileDefinition.Metadata.Set(
                normalizedMetadataKey,
                removedValue ?? string.Empty);

            return Response<ErrorProfileDefinition>.Invalid(
                code: "EditedProfileCatalogIsInvalid",
                message: "The edited profile catalog is invalid and was not saved.");
        }

        Response saveResponse = await new JsonCatalogDocumentWriter().SaveToFileAsync(
            profileCatalog,
            options.ProfilesFilePath,
            cancellationToken);

        if (!saveResponse.IsSuccess)
        {
            profileDefinition.Metadata.Set(
                normalizedMetadataKey,
                removedValue ?? string.Empty);

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

        return Response<ErrorProfileDefinition>.Ok(
            profileDefinition,
            $"Metadata '{normalizedMetadataKey}' was removed from profile '{profileDefinition.Name}'.");
    }

    private static void RollbackMetadata(
        ErrorProfileDefinition profileDefinition,
        string normalizedMetadataKey,
        string? oldValue,
        bool metadataExisted)
    {
        if (metadataExisted)
        {
            profileDefinition.Metadata.Set(
                normalizedMetadataKey,
                oldValue ?? string.Empty);
            return;
        }

        profileDefinition.Metadata.Remove(normalizedMetadataKey);
    }
}
