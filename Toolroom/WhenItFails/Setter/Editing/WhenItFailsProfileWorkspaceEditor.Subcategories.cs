using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;

/// <summary>
/// Provides profile subcategory editing for <see cref="WhenItFailsProfileWorkspaceEditor"/>.
/// </summary>
internal static class WhenItFailsProfileWorkspaceEditorSubcategoryExtensions
{
    /// <summary>
    /// Adds an existing workspace subcategory to one profile.
    /// </summary>
    public static async Task<Response<ErrorProfileDefinition>> ProfileAddSubcategoryAsync(
        this WhenItFailsProfileWorkspaceEditor editor,
        string inputPath,
        string profileName,
        string subcategoryName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editor);

        Response<ProfileSubcategoryEditContext> contextResponse =
            await LoadContextAsync(
                inputPath,
                profileName,
                subcategoryName,
                cancellationToken);

        if (!contextResponse.IsSuccess || contextResponse.Data is null)
        {
            return CopyFailure<ErrorProfileDefinition, ProfileSubcategoryEditContext>(contextResponse);
        }

        ProfileSubcategoryEditContext context = contextResponse.Data;

        bool subcategoryAlreadyIncluded =
            context.ProfileDefinition.IncludeSubcategories.Any(includedSubcategory =>
                string.Equals(
                    includedSubcategory,
                    context.SubcategoryName,
                    StringComparison.OrdinalIgnoreCase));

        if (subcategoryAlreadyIncluded)
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileSubcategoryAlreadyIncluded",
                message: $"Profile '{context.ProfileDefinition.Name}' already includes subcategory '{context.SubcategoryName}'.");
        }

        context.ProfileDefinition.IncludeSubcategories.Add(context.SubcategoryName);

        Response<ErrorProfileDefinition>? saveFailure = await ValidateAndSaveAsync(
            context,
            rollback: () => context.ProfileDefinition.IncludeSubcategories.Remove(context.SubcategoryName),
            cancellationToken);

        if (saveFailure is not null)
        {
            return saveFailure;
        }

        return Response<ErrorProfileDefinition>.Ok(
            context.ProfileDefinition,
            $"Subcategory '{context.SubcategoryName}' was added to profile '{context.ProfileDefinition.Name}'.");
    }

    /// <summary>
    /// Removes an included workspace subcategory from one profile.
    /// </summary>
    public static async Task<Response<ErrorProfileDefinition>> ProfileRemoveSubcategoryAsync(
        this WhenItFailsProfileWorkspaceEditor editor,
        string inputPath,
        string profileName,
        string subcategoryName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editor);

        Response<ProfileSubcategoryEditContext> contextResponse =
            await LoadContextAsync(
                inputPath,
                profileName,
                subcategoryName,
                cancellationToken);

        if (!contextResponse.IsSuccess || contextResponse.Data is null)
        {
            return CopyFailure<ErrorProfileDefinition, ProfileSubcategoryEditContext>(contextResponse);
        }

        ProfileSubcategoryEditContext context = contextResponse.Data;

        int subcategoryIndex = context.ProfileDefinition.IncludeSubcategories.FindIndex(
            includedSubcategory => string.Equals(
                includedSubcategory,
                context.SubcategoryName,
                StringComparison.OrdinalIgnoreCase));

        if (subcategoryIndex < 0)
        {
            return Response<ErrorProfileDefinition>.NotFound(
                code: "ProfileSubcategoryNotIncluded",
                message: $"Profile '{context.ProfileDefinition.Name}' does not include subcategory '{context.SubcategoryName}'.");
        }

        string removedSubcategoryName =
            context.ProfileDefinition.IncludeSubcategories[subcategoryIndex];

        context.ProfileDefinition.IncludeSubcategories.RemoveAt(subcategoryIndex);

        Response<ErrorProfileDefinition>? saveFailure = await ValidateAndSaveAsync(
            context,
            rollback: () => context.ProfileDefinition.IncludeSubcategories.Insert(
                subcategoryIndex,
                removedSubcategoryName),
            cancellationToken);

        if (saveFailure is not null)
        {
            return saveFailure;
        }

        return Response<ErrorProfileDefinition>.Ok(
            context.ProfileDefinition,
            $"Subcategory '{context.SubcategoryName}' was removed from profile '{context.ProfileDefinition.Name}'.");
    }

    private static async Task<Response<ProfileSubcategoryEditContext>> LoadContextAsync(
        string inputPath,
        string profileName,
        string subcategoryName,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        if (string.IsNullOrWhiteSpace(profileName))
        {
            return Response<ProfileSubcategoryEditContext>.Invalid(
                code: "ProfileNameIsEmpty",
                message: "Profile name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(subcategoryName))
        {
            return Response<ProfileSubcategoryEditContext>.Invalid(
                code: "SubcategoryNameIsEmpty",
                message: "Subcategory name cannot be empty.");
        }

        JsonsOptions options =
            WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(inputPath);

        Response<ErrorProfileCatalogDocument> profileLoadResponse =
            await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(
                options.ProfilesFilePath,
                cancellationToken);

        if (!profileLoadResponse.IsSuccess || profileLoadResponse.Data is null)
        {
            return Response<ProfileSubcategoryEditContext>.Fail(
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
            return Response<ProfileSubcategoryEditContext>.Invalid(
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
            return Response<ProfileSubcategoryEditContext>.NotFound(
                code: "ProfileNotFound",
                message: $"Profile '{normalizedProfileName}' was not found.");
        }

        Response<ErrorCatalogDocument> errorLoadResponse =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                options.ErrorCatalogFilePath,
                cancellationToken);

        if (!errorLoadResponse.IsSuccess || errorLoadResponse.Data is null)
        {
            return Response<ProfileSubcategoryEditContext>.Fail(
                code: "ErrorCatalogLoadFailed",
                message: string.IsNullOrWhiteSpace(errorLoadResponse.Message)
                    ? $"Error catalog could not be loaded: {options.ErrorCatalogFilePath}"
                    : errorLoadResponse.Message);
        }

        ErrorCatalogDocument errorCatalog =
            new ErrorCatalogDocumentNormalizer().Normalize(errorLoadResponse.Data);

        string normalizedSubcategoryName =
            TextKeyNormalizer.NormalizeKey(subcategoryName);

        string? canonicalSubcategoryName = errorCatalog.Errors
            .SelectMany(error => error.Subcategories)
            .Select(TextKeyNormalizer.NormalizeKey)
            .FirstOrDefault(subcategory =>
                string.Equals(
                    subcategory,
                    normalizedSubcategoryName,
                    StringComparison.OrdinalIgnoreCase));

        if (canonicalSubcategoryName is null)
        {
            return Response<ProfileSubcategoryEditContext>.NotFound(
                code: "SubcategoryNotFound",
                message: $"Subcategory '{normalizedSubcategoryName}' was not found in the error catalog.");
        }

        return Response<ProfileSubcategoryEditContext>.Ok(
            new ProfileSubcategoryEditContext(
                options.ProfilesFilePath,
                profileCatalog,
                profileDefinition,
                canonicalSubcategoryName));
    }

    private static async Task<Response<ErrorProfileDefinition>?> ValidateAndSaveAsync(
        ProfileSubcategoryEditContext context,
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
            : "ProfileSubcategoryEditFailed";

        return Response<TTarget>.Fail(
            code: code,
            message: response.Message);
    }

    private sealed record ProfileSubcategoryEditContext(
        string ProfileCatalogFilePath,
        ErrorProfileCatalogDocument ProfileCatalog,
        ErrorProfileDefinition ProfileDefinition,
        string SubcategoryName);
}
