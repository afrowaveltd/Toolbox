using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;

/// <summary>
/// Provides profile excluded-tag editing for <see cref="WhenItFailsProfileWorkspaceEditor"/>.
/// </summary>
internal static class WhenItFailsProfileWorkspaceEditorExcludedTagExtensions
{
    /// <summary>
    /// Adds an existing workspace tag to one profile's excluded tags.
    /// </summary>
    public static async Task<Response<ErrorProfileDefinition>> ProfileAddExcludedTagAsync(
        this WhenItFailsProfileWorkspaceEditor editor,
        string inputPath,
        string profileName,
        string tagName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editor);

        Response<ProfileExcludedTagEditContext> contextResponse = await LoadContextAsync(
            inputPath,
            profileName,
            tagName,
            cancellationToken);

        if (!contextResponse.IsSuccess || contextResponse.Data is null)
        {
            return CopyFailure<ErrorProfileDefinition, ProfileExcludedTagEditContext>(contextResponse);
        }

        ProfileExcludedTagEditContext context = contextResponse.Data;

        bool tagAlreadyExcluded = context.ProfileDefinition.ExcludeTags.Any(excludedTag =>
            string.Equals(
                excludedTag,
                context.CanonicalTagName,
                StringComparison.OrdinalIgnoreCase));

        if (tagAlreadyExcluded)
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileTagAlreadyExcluded",
                message: $"Profile '{context.ProfileDefinition.Name}' already excludes tag '{context.CanonicalTagName}'.");
        }

        context.ProfileDefinition.ExcludeTags.Add(context.CanonicalTagName);

        Response<ErrorProfileDefinition>? saveFailure = await ValidateAndSaveAsync(
            context,
            rollback: () => context.ProfileDefinition.ExcludeTags.Remove(context.CanonicalTagName),
            cancellationToken);

        if (saveFailure is not null)
        {
            return saveFailure;
        }

        return Response<ErrorProfileDefinition>.Ok(
            context.ProfileDefinition,
            $"Tag '{context.CanonicalTagName}' was added to excluded tags for profile '{context.ProfileDefinition.Name}'.");
    }

    private static async Task<Response<ProfileExcludedTagEditContext>> LoadContextAsync(
        string inputPath,
        string profileName,
        string tagName,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        if (string.IsNullOrWhiteSpace(profileName))
        {
            return Response<ProfileExcludedTagEditContext>.Invalid(
                code: "ProfileNameIsEmpty",
                message: "Profile name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(tagName))
        {
            return Response<ProfileExcludedTagEditContext>.Invalid(
                code: "TagNameIsEmpty",
                message: "Tag name cannot be empty.");
        }

        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(inputPath);

        Response<ErrorProfileCatalogDocument> profileLoadResponse =
            await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(
                options.ProfilesFilePath,
                cancellationToken);

        if (!profileLoadResponse.IsSuccess || profileLoadResponse.Data is null)
        {
            return Response<ProfileExcludedTagEditContext>.Fail(
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
            return Response<ProfileExcludedTagEditContext>.Invalid(
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
            return Response<ProfileExcludedTagEditContext>.NotFound(
                code: "ProfileNotFound",
                message: $"Profile '{normalizedProfileName}' was not found.");
        }

        Response<ErrorCatalogDocument> errorLoadResponse =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                options.ErrorCatalogFilePath,
                cancellationToken);

        if (!errorLoadResponse.IsSuccess || errorLoadResponse.Data is null)
        {
            return Response<ProfileExcludedTagEditContext>.Fail(
                code: "ErrorCatalogLoadFailed",
                message: string.IsNullOrWhiteSpace(errorLoadResponse.Message)
                    ? $"Error catalog could not be loaded: {options.ErrorCatalogFilePath}"
                    : errorLoadResponse.Message);
        }

        ErrorCatalogDocument errorCatalog =
            new ErrorCatalogDocumentNormalizer().Normalize(errorLoadResponse.Data);

        string normalizedTagName = TextKeyNormalizer.NormalizeKey(tagName);
        string? canonicalTagName = errorCatalog.Errors
            .SelectMany(error => error.Tags)
            .Select(TextKeyNormalizer.NormalizeKey)
            .FirstOrDefault(tag => string.Equals(
                tag,
                normalizedTagName,
                StringComparison.OrdinalIgnoreCase));

        if (canonicalTagName is null)
        {
            return Response<ProfileExcludedTagEditContext>.NotFound(
                code: "TagNotFound",
                message: $"Tag '{normalizedTagName}' was not found in the error catalog.");
        }

        return Response<ProfileExcludedTagEditContext>.Ok(
            new ProfileExcludedTagEditContext(
                options.ProfilesFilePath,
                profileCatalog,
                profileDefinition,
                canonicalTagName));
    }

    private static async Task<Response<ErrorProfileDefinition>?> ValidateAndSaveAsync(
        ProfileExcludedTagEditContext context,
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
            : "ProfileExcludedTagEditFailed";

        return Response<TTarget>.Fail(
            code: code,
            message: response.Message);
    }

    private sealed record ProfileExcludedTagEditContext(
        string ProfileCatalogFilePath,
        ErrorProfileCatalogDocument ProfileCatalog,
        ErrorProfileDefinition ProfileDefinition,
        string CanonicalTagName);
}
