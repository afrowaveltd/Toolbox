using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;

/// <summary>
/// Provides profile category editing for <see cref="WhenItFailsProfileWorkspaceEditor"/>.
/// </summary>
internal static class WhenItFailsProfileWorkspaceEditorCategoryExtensions
{
    /// <summary>
    /// Adds an existing workspace category to one profile.
    /// </summary>
    public static async Task<Response<ErrorProfileDefinition>> ProfileAddCategoryAsync(
        this WhenItFailsProfileWorkspaceEditor editor,
        string inputPath,
        string profileName,
        string categoryName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editor);

        Response<ProfileCategoryEditContext> contextResponse =
            await LoadContextAsync(
                inputPath,
                profileName,
                categoryName,
                cancellationToken);

        if (!contextResponse.IsSuccess || contextResponse.Data is null)
        {
            return CopyFailure<ErrorProfileDefinition, ProfileCategoryEditContext>(contextResponse);
        }

        ProfileCategoryEditContext context = contextResponse.Data;

        bool categoryAlreadyIncluded =
            context.ProfileDefinition.IncludeCategories.Any(includedCategory =>
                string.Equals(
                    includedCategory,
                    context.CategoryDefinition.Name,
                    StringComparison.OrdinalIgnoreCase));

        if (categoryAlreadyIncluded)
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileCategoryAlreadyIncluded",
                message: $"Profile '{context.ProfileDefinition.Name}' already includes category '{context.CategoryDefinition.Name}'.");
        }

        context.ProfileDefinition.IncludeCategories.Add(context.CategoryDefinition.Name);

        Response<ErrorProfileDefinition>? saveFailure = await ValidateAndSaveAsync(
            context,
            rollback: () => context.ProfileDefinition.IncludeCategories.Remove(context.CategoryDefinition.Name),
            cancellationToken);

        if (saveFailure is not null)
        {
            return saveFailure;
        }

        return Response<ErrorProfileDefinition>.Ok(
            context.ProfileDefinition,
            $"Category '{context.CategoryDefinition.Name}' was added to profile '{context.ProfileDefinition.Name}'.");
    }

    /// <summary>
    /// Removes an included workspace category from one profile.
    /// </summary>
    public static async Task<Response<ErrorProfileDefinition>> ProfileRemoveCategoryAsync(
        this WhenItFailsProfileWorkspaceEditor editor,
        string inputPath,
        string profileName,
        string categoryName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editor);

        Response<ProfileCategoryEditContext> contextResponse =
            await LoadContextAsync(
                inputPath,
                profileName,
                categoryName,
                cancellationToken);

        if (!contextResponse.IsSuccess || contextResponse.Data is null)
        {
            return CopyFailure<ErrorProfileDefinition, ProfileCategoryEditContext>(contextResponse);
        }

        ProfileCategoryEditContext context = contextResponse.Data;

        int categoryIndex = context.ProfileDefinition.IncludeCategories.FindIndex(includedCategory =>
            string.Equals(
                includedCategory,
                context.CategoryDefinition.Name,
                StringComparison.OrdinalIgnoreCase));

        if (categoryIndex < 0)
        {
            return Response<ErrorProfileDefinition>.NotFound(
                code: "ProfileCategoryNotIncluded",
                message: $"Profile '{context.ProfileDefinition.Name}' does not include category '{context.CategoryDefinition.Name}'.");
        }

        string removedCategoryName = context.ProfileDefinition.IncludeCategories[categoryIndex];
        context.ProfileDefinition.IncludeCategories.RemoveAt(categoryIndex);

        Response<ErrorProfileDefinition>? saveFailure = await ValidateAndSaveAsync(
            context,
            rollback: () => context.ProfileDefinition.IncludeCategories.Insert(categoryIndex, removedCategoryName),
            cancellationToken);

        if (saveFailure is not null)
        {
            return saveFailure;
        }

        return Response<ErrorProfileDefinition>.Ok(
            context.ProfileDefinition,
            $"Category '{context.CategoryDefinition.Name}' was removed from profile '{context.ProfileDefinition.Name}'.");
    }

    private static async Task<Response<ProfileCategoryEditContext>> LoadContextAsync(
        string inputPath,
        string profileName,
        string categoryName,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        if (string.IsNullOrWhiteSpace(profileName))
        {
            return Response<ProfileCategoryEditContext>.Invalid(
                code: "ProfileNameIsEmpty",
                message: "Profile name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(categoryName))
        {
            return Response<ProfileCategoryEditContext>.Invalid(
                code: "CategoryNameIsEmpty",
                message: "Category name cannot be empty.");
        }

        JsonsOptions options =
            WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(inputPath);

        Response<ErrorProfileCatalogDocument> profileLoadResponse =
            await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(
                options.ProfilesFilePath,
                cancellationToken);

        if (!profileLoadResponse.IsSuccess || profileLoadResponse.Data is null)
        {
            return Response<ProfileCategoryEditContext>.Fail(
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
            return Response<ProfileCategoryEditContext>.Invalid(
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
            return Response<ProfileCategoryEditContext>.NotFound(
                code: "ProfileNotFound",
                message: $"Profile '{normalizedProfileName}' was not found.");
        }

        Response<ErrorCategoryCatalogDocument> categoryLoadResponse =
            await new JsonErrorCategoryCatalogLoader().LoadFromFileAsync(
                options.CategoryCatalogFilePath,
                cancellationToken);

        if (!categoryLoadResponse.IsSuccess || categoryLoadResponse.Data is null)
        {
            return Response<ProfileCategoryEditContext>.Fail(
                code: "CategoryCatalogLoadFailed",
                message: string.IsNullOrWhiteSpace(categoryLoadResponse.Message)
                    ? $"Category catalog could not be loaded: {options.CategoryCatalogFilePath}"
                    : categoryLoadResponse.Message);
        }

        ErrorCategoryCatalogDocument categoryCatalog =
            new ErrorCategoryCatalogDocumentNormalizer().Normalize(categoryLoadResponse.Data);

        string normalizedCategoryName = TextKeyNormalizer.NormalizeKey(categoryName);
        ErrorCategoryDefinition? categoryDefinition =
            categoryCatalog.Categories.FirstOrDefault(category =>
                string.Equals(
                    category.Name,
                    normalizedCategoryName,
                    StringComparison.OrdinalIgnoreCase)
                || category.Aliases.Any(alias =>
                    string.Equals(
                        alias,
                        normalizedCategoryName,
                        StringComparison.OrdinalIgnoreCase)));

        if (categoryDefinition is null)
        {
            return Response<ProfileCategoryEditContext>.NotFound(
                code: "CategoryNotFound",
                message: $"Category '{normalizedCategoryName}' was not found.");
        }

        return Response<ProfileCategoryEditContext>.Ok(
            new ProfileCategoryEditContext(
                options.ProfilesFilePath,
                profileCatalog,
                profileDefinition,
                categoryDefinition));
    }

    private static async Task<Response<ErrorProfileDefinition>?> ValidateAndSaveAsync(
        ProfileCategoryEditContext context,
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
            : "ProfileCategoryEditFailed";

        return Response<TTarget>.Fail(
            code: code,
            message: response.Message);
    }

    private sealed record ProfileCategoryEditContext(
        string ProfileCatalogFilePath,
        ErrorProfileCatalogDocument ProfileCatalog,
        ErrorProfileDefinition ProfileDefinition,
        ErrorCategoryDefinition CategoryDefinition);
}
