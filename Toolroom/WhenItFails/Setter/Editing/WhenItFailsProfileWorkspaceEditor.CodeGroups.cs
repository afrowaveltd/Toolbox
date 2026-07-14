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

        Response<ProfileCodeGroupEditContext> contextResponse =
            await LoadContextAsync(
                inputPath,
                profileName,
                codeGroupName,
                cancellationToken);

        if (!contextResponse.IsSuccess || contextResponse.Data is null)
        {
            return CopyFailure<ErrorProfileDefinition, ProfileCodeGroupEditContext>(contextResponse);
        }

        ProfileCodeGroupEditContext context = contextResponse.Data;

        bool codeGroupAlreadyIncluded =
            context.ProfileDefinition.IncludeCodeGroups.Any(includedCodeGroup =>
                string.Equals(
                    includedCodeGroup,
                    context.CodeGroupDefinition.Name,
                    StringComparison.OrdinalIgnoreCase));

        if (codeGroupAlreadyIncluded)
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileCodeGroupAlreadyIncluded",
                message: $"Profile '{context.ProfileDefinition.Name}' already includes code group '{context.CodeGroupDefinition.Name}'.");
        }

        context.ProfileDefinition.IncludeCodeGroups.Add(context.CodeGroupDefinition.Name);

        Response<ErrorProfileDefinition>? saveFailure = await ValidateAndSaveAsync(
            context,
            rollback: () => context.ProfileDefinition.IncludeCodeGroups.Remove(context.CodeGroupDefinition.Name),
            cancellationToken);

        if (saveFailure is not null)
        {
            return saveFailure;
        }

        return Response<ErrorProfileDefinition>.Ok(
            context.ProfileDefinition,
            $"Code group '{context.CodeGroupDefinition.Name}' was added to profile '{context.ProfileDefinition.Name}'.");
    }

    /// <summary>
    /// Removes an included workspace code group from one profile.
    /// </summary>
    public static async Task<Response<ErrorProfileDefinition>> ProfileRemoveCodeGroupAsync(
        this WhenItFailsProfileWorkspaceEditor editor,
        string inputPath,
        string profileName,
        string codeGroupName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editor);

        Response<ProfileCodeGroupEditContext> contextResponse =
            await LoadContextAsync(
                inputPath,
                profileName,
                codeGroupName,
                cancellationToken);

        if (!contextResponse.IsSuccess || contextResponse.Data is null)
        {
            return CopyFailure<ErrorProfileDefinition, ProfileCodeGroupEditContext>(contextResponse);
        }

        ProfileCodeGroupEditContext context = contextResponse.Data;

        int codeGroupIndex = context.ProfileDefinition.IncludeCodeGroups.FindIndex(includedCodeGroup =>
            string.Equals(
                includedCodeGroup,
                context.CodeGroupDefinition.Name,
                StringComparison.OrdinalIgnoreCase));

        if (codeGroupIndex < 0)
        {
            return Response<ErrorProfileDefinition>.NotFound(
                code: "ProfileCodeGroupNotIncluded",
                message: $"Profile '{context.ProfileDefinition.Name}' does not include code group '{context.CodeGroupDefinition.Name}'.");
        }

        string removedCodeGroupName = context.ProfileDefinition.IncludeCodeGroups[codeGroupIndex];
        context.ProfileDefinition.IncludeCodeGroups.RemoveAt(codeGroupIndex);

        Response<ErrorProfileDefinition>? saveFailure = await ValidateAndSaveAsync(
            context,
            rollback: () => context.ProfileDefinition.IncludeCodeGroups.Insert(codeGroupIndex, removedCodeGroupName),
            cancellationToken);

        if (saveFailure is not null)
        {
            return saveFailure;
        }

        return Response<ErrorProfileDefinition>.Ok(
            context.ProfileDefinition,
            $"Code group '{context.CodeGroupDefinition.Name}' was removed from profile '{context.ProfileDefinition.Name}'.");
    }

    private static async Task<Response<ProfileCodeGroupEditContext>> LoadContextAsync(
        string inputPath,
        string profileName,
        string codeGroupName,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        if (string.IsNullOrWhiteSpace(profileName))
        {
            return Response<ProfileCodeGroupEditContext>.Invalid(
                code: "ProfileNameIsEmpty",
                message: "Profile name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(codeGroupName))
        {
            return Response<ProfileCodeGroupEditContext>.Invalid(
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
            return Response<ProfileCodeGroupEditContext>.Fail(
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
            return Response<ProfileCodeGroupEditContext>.Invalid(
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
            return Response<ProfileCodeGroupEditContext>.NotFound(
                code: "ProfileNotFound",
                message: $"Profile '{normalizedProfileName}' was not found.");
        }

        Response<ErrorCodeGroupCatalogDocument> codeGroupLoadResponse =
            await new JsonErrorCodeGroupCatalogLoader().LoadFromFileAsync(
                options.CodeGroupCatalogFilePath,
                cancellationToken);

        if (!codeGroupLoadResponse.IsSuccess || codeGroupLoadResponse.Data is null)
        {
            return Response<ProfileCodeGroupEditContext>.Fail(
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
            return Response<ProfileCodeGroupEditContext>.Invalid(
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
            return Response<ProfileCodeGroupEditContext>.NotFound(
                code: "CodeGroupNotFound",
                message: $"Code group '{normalizedCodeGroupName}' was not found.");
        }

        return Response<ProfileCodeGroupEditContext>.Ok(
            new ProfileCodeGroupEditContext(
                options.ProfilesFilePath,
                profileCatalog,
                profileDefinition,
                codeGroupDefinition));
    }

    private static async Task<Response<ErrorProfileDefinition>?> ValidateAndSaveAsync(
        ProfileCodeGroupEditContext context,
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
            : "ProfileCodeGroupEditFailed";

        return Response<TTarget>.Fail(
            code: code,
            message: response.Message);
    }

    private sealed record ProfileCodeGroupEditContext(
        string ProfileCatalogFilePath,
        ErrorProfileCatalogDocument ProfileCatalog,
        ErrorProfileDefinition ProfileDefinition,
        ErrorCodeGroupDefinition CodeGroupDefinition);
}
