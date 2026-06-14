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

        if (context.ProfileCatalog is null)
        {
            return Response<IReadOnlyList<ErrorDefinition>>.Invalid(
                code: "ErrorProfileCatalogIsNull",
                message:
                    "Error catalog context does not contain an error profile catalog.");
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
                        StringComparison.OrdinalIgnoreCase));

        if (profile is null)
        {
            return Response<IReadOnlyList<ErrorDefinition>>.NotFound(
                code: "ErrorProfileNotFoundByName",
                message:
                    $"Error profile with name '{profileName}' was not found.");
        }

        IReadOnlyList<ErrorDefinition> resolvedErrors =
            _profileResolver.Resolve(
                context.ErrorCatalogDocument,
                profile);

        return Response<IReadOnlyList<ErrorDefinition>>.Ok(
            resolvedErrors);
    }
}