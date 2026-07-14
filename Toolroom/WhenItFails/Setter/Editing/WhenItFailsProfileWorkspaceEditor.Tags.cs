using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;

/// <summary>
/// Provides profile include-tag editing for <see cref="WhenItFailsProfileWorkspaceEditor"/>.
/// </summary>
internal static class WhenItFailsProfileWorkspaceEditorTagExtensions
{
    /// <summary>
    /// Adds an existing workspace tag to one profile.
    /// </summary>
    public static async Task<Response<ErrorProfileDefinition>> ProfileAddTagAsync(
        this WhenItFailsProfileWorkspaceEditor editor,
        string inputPath,
        string profileName,
        string tagName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editor);

        Response<ProfileTagEditContext> contextResponse = await LoadContextAsync(
            inputPath,
            profileName,
            tagName,
            cancellationToken);

        if (!contextResponse.IsSuccess || contextResponse.Data is null)
        {
            return CopyFailure<ErrorProfileDefinition, ProfileTagEditContext>(contextResponse);
        }

        ProfileTagEditContext context = contextResponse.Data;

        bool tagAlreadyIncluded = context.ProfileDefinition.IncludeTags.Any(includedTag =>
            string.Equals(
                includedTag,
                context.CanonicalTagName,
                StringComparison.OrdinalIgnoreCase));

        if (tagAlreadyIncluded)
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileTagAlreadyIncluded",
                message: $"Profile '{context.ProfileDefinition.Name}' already includes tag '{context.CanonicalTagName}'.");
        }

        context.ProfileDefinition.IncludeTags.Add(context.CanonicalTagName);

        Response<ErrorProfileDefinition>? saveFailure = await ValidateAndSaveAsync(
            context,
            rollback: () => context.ProfileDefinition.IncludeTags.Remove(context.CanonicalTagName),
            cancellationToken);

        if (saveFailure is not null)
        {
            return saveFailure;
        }

        return Response<ErrorProfileDefinition>.Ok(
            context.ProfileDefinition,
            $"Tag '{context.CanonicalTagName}' was added to profile '{context.ProfileDefinition.Name}'.");
    }

    private static async Task<Response<ProfileTagEditContext>> LoadContextAsync(
        string inputPath,
        string profileName,
        string tagName,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        if (string.IsNullOrWhiteSpace(profileName))
        {
            return Response<ProfileTagEditContext>.Invalid(
                code: "ProfileNameIsEmpty",
                message: "Profile name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(tagName))
        {
            return Response<ProfileTagEditContext>.Invalid(
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
            return Response<ProfileTagEditContext>.Fail(
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
            return Response<ProfileTagEditContext>.Invalid(
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
            return Response<ProfileTagEditContext>.NotFound(
                code: "ProfileNotFound",
                message: $"Profile '{normalizedProfileName}' was not found.");
        }

        Response<ErrorCatalogDocument> errorLoadResponse =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                options.ErrorCatalogFilePath,
                cancellationToken);

        if (!errorLoadResponse.IsSuccess || errorLoadResponse.Data is null)
        {
            return Response<ProfileTagEditContext>.Fail(
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
            return Response<ProfileTagEditContext>.NotFound(
                code: "TagNotFound",
                message: $"Tag '{normalizedTagName}' was not found in the error catalog.");
        }

        return Response<ProfileTagEditContext>.Ok(
            new ProfileTagEditContext(
                options.ProfilesFilePath,
                profileCatalog,
                profileDefinition,
                canonicalTagName));
    }

    private static async Task<Response<ErrorProfileDefinition>?> ValidateAndSaveAsync(
        ProfileTagEditContext context,
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
            : "ProfileTagEditFailed";

        return Response<TTarget>.Fail(
            code: code,
            message: response.Message);
    }

    private sealed record ProfileTagEditContext(
        string ProfileCatalogFilePath,
        ErrorProfileCatalogDocument ProfileCatalog,
        ErrorProfileDefinition ProfileDefinition,
        string CanonicalTagName);
}
