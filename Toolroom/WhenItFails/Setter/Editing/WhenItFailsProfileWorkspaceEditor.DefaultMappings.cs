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

        if (string.IsNullOrWhiteSpace(profileName))
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileNameIsEmpty",
                message: "Profile name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(mappingKey))
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileMappingKeyIsEmpty",
                message: "Profile mapping key cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(mappingValue))
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileMappingValueIsEmpty",
                message: "Profile mapping value cannot be empty.");
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

        string normalizedMappingKey = TextKeyNormalizer.NormalizeKey(mappingKey);
        string normalizedMappingValue = TextKeyNormalizer.NormalizeDisplayName(mappingValue);

        string? existingKey = profileDefinition.DefaultMappings.Keys.FirstOrDefault(key =>
            string.Equals(
                key,
                normalizedMappingKey,
                StringComparison.OrdinalIgnoreCase));

        bool mappingExisted = existingKey is not null;
        string? oldValue = mappingExisted
            ? profileDefinition.DefaultMappings[existingKey!]
            : null;

        if (mappingExisted
            && string.Equals(
                oldValue,
                normalizedMappingValue,
                StringComparison.Ordinal))
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileMappingAlreadySet",
                message: $"Profile '{profileDefinition.Name}' already maps '{normalizedMappingKey}' to '{normalizedMappingValue}'.");
        }

        if (mappingExisted)
        {
            profileDefinition.DefaultMappings.Remove(existingKey!);
        }

        profileDefinition.DefaultMappings[normalizedMappingKey] = normalizedMappingValue;

        ErrorCatalogValidationResult editedValidationResult =
            new ErrorProfileCatalogValidator().Validate(profileCatalog);

        if (!editedValidationResult.IsValid)
        {
            RollbackMapping(
                profileDefinition,
                normalizedMappingKey,
                existingKey,
                oldValue,
                mappingExisted);

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
            RollbackMapping(
                profileDefinition,
                normalizedMappingKey,
                existingKey,
                oldValue,
                mappingExisted);

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

        string action = mappingExisted ? "updated" : "added";

        return Response<ErrorProfileDefinition>.Ok(
            profileDefinition,
            $"Default mapping '{normalizedMappingKey}' was {action} for profile '{profileDefinition.Name}'.");
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
}
