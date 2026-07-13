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
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        if (string.IsNullOrWhiteSpace(profileName))
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileNameIsEmpty",
                message: "Profile name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(categoryName))
        {
            return Response<ErrorProfileDefinition>.Invalid(
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

        Response<ErrorCategoryCatalogDocument> categoryLoadResponse =
            await new JsonErrorCategoryCatalogLoader().LoadFromFileAsync(
                options.CategoryCatalogFilePath,
                cancellationToken);

        if (!categoryLoadResponse.IsSuccess || categoryLoadResponse.Data is null)
        {
            return Response<ErrorProfileDefinition>.Fail(
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
            return Response<ErrorProfileDefinition>.NotFound(
                code: "CategoryNotFound",
                message: $"Category '{normalizedCategoryName}' was not found.");
        }

        bool categoryAlreadyIncluded =
            profileDefinition.IncludeCategories.Any(includedCategory =>
                string.Equals(
                    includedCategory,
                    categoryDefinition.Name,
                    StringComparison.OrdinalIgnoreCase));

        if (categoryAlreadyIncluded)
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileCategoryAlreadyIncluded",
                message: $"Profile '{profileDefinition.Name}' already includes category '{categoryDefinition.Name}'.");
        }

        profileDefinition.IncludeCategories.Add(categoryDefinition.Name);

        ErrorCatalogValidationResult editedValidationResult =
            new ErrorProfileCatalogValidator().Validate(profileCatalog);

        if (!editedValidationResult.IsValid)
        {
            profileDefinition.IncludeCategories.Remove(categoryDefinition.Name);

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
            profileDefinition.IncludeCategories.Remove(categoryDefinition.Name);

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
            $"Category '{categoryDefinition.Name}' was added to profile '{profileDefinition.Name}'.");
    }
}
