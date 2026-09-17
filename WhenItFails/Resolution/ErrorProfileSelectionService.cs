using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.WhenItFails.Resolution;

/// <summary>
/// Default implementation that resolves errors using a named profile
/// from a loaded error catalog context.
/// </summary>
public sealed class ErrorProfileSelectionService
    : IErrorProfileSelectionService
{
    private readonly IErrorProfileResolver _profileResolver;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ErrorProfileSelectionService"/> class.
    /// </summary>
    /// <param name="profileResolver">
    /// Resolver used to apply the selected profile.
    /// </param>
    public ErrorProfileSelectionService(
        IErrorProfileResolver profileResolver)
    {
        _profileResolver = profileResolver
            ?? throw new ArgumentNullException(nameof(profileResolver));
    }

    /// <inheritdoc />
    public Response<IReadOnlyList<ErrorDefinition>> ResolveByProfileName(
        ErrorCatalogContext? context,
        string profileName)
    {
        if (context is null)
        {
            return Response<IReadOnlyList<ErrorDefinition>>.Invalid(
                code: "ErrorCatalogContextIsNull",
                message: "Error catalog context is null.");
        }

        if (context.ErrorCatalogDocument is null)
        {
            return Response<IReadOnlyList<ErrorDefinition>>.Invalid(
                code: "ErrorCatalogDocumentIsNull",
                message:
                    "Error catalog context does not contain an error catalog document.");
        }

        if (context.ErrorCatalogDocument.Errors is null)
        {
            return Response<IReadOnlyList<ErrorDefinition>>.Invalid(
                code: "CatalogErrorsCollectionIsNull",
                message: "Error catalog errors collection is null.");
        }

        if (context.ErrorCatalogDocument.Errors.Any(error => error is null))
        {
            return Response<IReadOnlyList<ErrorDefinition>>.Invalid(
                code: "ErrorDefinitionIsNull",
                message: "Error catalog contains a null error definition.");
        }

        if (context.ProfileCatalog is null)
        {
            return Response<IReadOnlyList<ErrorDefinition>>.Invalid(
                code: "ErrorProfileCatalogIsNull",
                message:
                    "Error catalog context does not contain an error profile catalog.");
        }

        if (context.ProfileCatalog.Profiles is null)
        {
            return Response<IReadOnlyList<ErrorDefinition>>.Invalid(
                code: "ErrorProfileCatalogProfilesCollectionIsNull",
                message:
                    "Error profile catalog profiles collection is null.");
        }

        if (context.ProfileCatalog.Profiles.Any(candidate => candidate is null))
        {
            return Response<IReadOnlyList<ErrorDefinition>>.Invalid(
                code: "ErrorProfileDefinitionIsNull",
                message:
                    "Error profile catalog contains a null profile definition.");
        }

        if (string.IsNullOrWhiteSpace(profileName))
        {
            return Response<IReadOnlyList<ErrorDefinition>>.Invalid(
                code: "ProfileNameIsEmpty",
                message: "Profile name is empty.");
        }

        string normalizedProfileName =
            TextKeyNormalizer.NormalizeKey(profileName);

        ErrorProfileDefinition? profile =
            context.ProfileCatalog.Profiles.FirstOrDefault(
                candidate =>
                    string.Equals(
                        TextKeyNormalizer.NormalizeKey(candidate.Name),
                        normalizedProfileName,
                        StringComparison.OrdinalIgnoreCase)
                    || string.Equals(
                        TextKeyNormalizer.NormalizeKey(candidate.DisplayName),
                        normalizedProfileName,
                        StringComparison.OrdinalIgnoreCase));

        if (profile is null)
        {
            return Response<IReadOnlyList<ErrorDefinition>>.NotFound(
                code: "ErrorProfileNotFoundByName",
                message:
                    $"Error profile with name or display name '{profileName}' was not found.");
        }

        if (profile.IncludeOwners is null)
        {
            return Response<IReadOnlyList<ErrorDefinition>>.Invalid(
                code: "ProfileIncludeOwnersCollectionIsNull",
                message: "Profile include owners collection is null.");
        }

        if (profile.IncludeCodeGroups is null)
        {
            return Response<IReadOnlyList<ErrorDefinition>>.Invalid(
                code: "ProfileIncludeCodeGroupsCollectionIsNull",
                message: "Profile include code groups collection is null.");
        }

        if (profile.IncludeCategories is null)
        {
            return Response<IReadOnlyList<ErrorDefinition>>.Invalid(
                code: "ProfileIncludeCategoriesCollectionIsNull",
                message: "Profile include categories collection is null.");
        }

        if (profile.IncludeSubcategories is null)
        {
            return Response<IReadOnlyList<ErrorDefinition>>.Invalid(
                code: "ProfileIncludeSubcategoriesCollectionIsNull",
                message: "Profile include subcategories collection is null.");
        }

        if (profile.IncludeTags is null)
        {
            return Response<IReadOnlyList<ErrorDefinition>>.Invalid(
                code: "ProfileIncludeTagsCollectionIsNull",
                message: "Profile include tags collection is null.");
        }

        if (profile.ExcludeTags is null)
        {
            return Response<IReadOnlyList<ErrorDefinition>>.Invalid(
                code: "ProfileExcludeTagsCollectionIsNull",
                message: "Profile exclude tags collection is null.");
        }

        if (profile.IncludeErrors is null)
        {
            return Response<IReadOnlyList<ErrorDefinition>>.Invalid(
                code: "ProfileIncludeErrorsCollectionIsNull",
                message: "Profile include errors collection is null.");
        }

        if (profile.ExcludeErrors is null)
        {
            return Response<IReadOnlyList<ErrorDefinition>>.Invalid(
                code: "ProfileExcludeErrorsCollectionIsNull",
                message: "Profile exclude errors collection is null.");
        }

        IReadOnlyList<ErrorDefinition>? resolvedErrors;

        try
        {
            resolvedErrors =
                _profileResolver.Resolve(
                    context.ErrorCatalogDocument,
                    profile);
        }
        catch (Exception exception)
            when (exception is not OperationCanceledException)
        {
            return Response<IReadOnlyList<ErrorDefinition>>.Fail(
                code: "WIF_PROFILE_RESOLVER_FAILED",
                message: "The error profile resolver failed.");
        }

        if (resolvedErrors is null)
        {
            return Response<IReadOnlyList<ErrorDefinition>>.Invalid(
                code: "WIF_PROFILE_RESOLVER_RESULT_NULL",
                message:
                    "The error profile resolver returned a null result.");
        }

        return Response<IReadOnlyList<ErrorDefinition>>.Ok(
            resolvedErrors);
    }
}
