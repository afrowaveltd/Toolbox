using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;

/// <summary>
/// Provides profile code group editing for <see cref="WhenItFailsProfileWorkspaceEditor"/>.
/// </summary>
internal static class WhenItFailsProfileWorkspaceEditorCodeGroupExtensions
{
    /// <summary>
    /// Adds an existing workspace code group to one profile.
    /// </summary>
    public static async Task<Response<ErrorProfileDefinition>> ProfileAddCodeGroupAsync(
        this WhenItFailsProfileWorkspaceEditor editor,
        string inputPath,
        string profileName,
        string codeGroupName,
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

        if (string.IsNullOrWhiteSpace(codeGroupName))
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "CodeGroupNameIsEmpty",
                message: "Code group name cannot be empty.");
        }

        JsonsOptions options =
            WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(inputPath);

        Response<ErrorProfileCatalogDocument> profileLoadResponse =
            await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(
                options.ProfilesFilePath,
                cancellationToken);

        if (!profileLoadResponse.IsSuccess || profileLoadResponse.Data is null)
        {
            return Response<ErrorProfileDefinition>.Fail(
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
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileCatalogIsInvalid",
                message: "The profile catalog is invalid and cannot be edited safely.");
        }

        string normalizedProfileName = TextKeyNormalizer.NormalizeKey(profileName);
        ErrorProfileDefinition? profileDefinition =
            profileCatalog.Profiles.FirstOrDefault(profile =>
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

        Response<ErrorCodeGroupCatalogDocument> codeGroupLoadResponse =
            await new JsonErrorCodeGroupCatalogLoader().LoadFromFileAsync(
                options.CodeGroupCatalogFilePath,
                cancellationToken);

        if (!codeGroupLoadResponse.IsSuccess || codeGroupLoadResponse.Data is null)
        {
            return Response<ErrorProfileDefinition>.Fail(
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
            return Response<ErrorProfileDefinition>.Invalid(
                code: "CodeGroupCatalogIsInvalid",
                message: "The code group catalog is invalid and cannot be used safely.");
        }

        string normalizedCodeGroupName = TextKeyNormalizer.NormalizeKey(codeGroupName);
        ErrorCodeGroupDefinition? codeGroupDefinition =
            codeGroupCatalog.CodeGroups.FirstOrDefault(codeGroup =>
                string.Equals(
                    codeGroup.Name,
                    normalizedCodeGroupName,
                    StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                    codeGroup.CodePrefix,
                    normalizedCodeGroupName,
                    StringComparison.OrdinalIgnoreCase));

        if (codeGroupDefinition is null)
        {
            return Response<ErrorProfileDefinition>.NotFound(
                code: "CodeGroupNotFound",
                message: $"Code group '{normalizedCodeGroupName}' was not found.");
        }

        bool codeGroupAlreadyIncluded =
            profileDefinition.IncludeCodeGroups.Any(includedCodeGroup =>
                string.Equals(
                    includedCodeGroup,
                    codeGroupDefinition.Name,
                    StringComparison.OrdinalIgnoreCase));

        if (codeGroupAlreadyIncluded)
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileCodeGroupAlreadyIncluded",
                message: $"Profile '{profileDefinition.Name}' already includes code group '{codeGroupDefinition.Name}'.");
        }

        profileDefinition.IncludeCodeGroups.Add(codeGroupDefinition.Name);

        ErrorCatalogValidationResult editedValidationResult =
            new ErrorProfileCatalogValidator().Validate(profileCatalog);

        if (!editedValidationResult.IsValid)
        {
            profileDefinition.IncludeCodeGroups.Remove(codeGroupDefinition.Name);

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
            profileDefinition.IncludeCodeGroups.Remove(codeGroupDefinition.Name);

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
            $"Code group '{codeGroupDefinition.Name}' was added to profile '{profileDefinition.Name}'.");
    }
}
