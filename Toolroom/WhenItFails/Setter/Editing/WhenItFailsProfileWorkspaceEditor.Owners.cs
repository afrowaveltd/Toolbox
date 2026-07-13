using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;

/// <summary>
/// Provides profile owner editing for <see cref="WhenItFailsProfileWorkspaceEditor"/>.
/// </summary>
internal static class WhenItFailsProfileWorkspaceEditorOwnerExtensions
{
    /// <summary>
    /// Adds an existing workspace owner to one profile.
    /// </summary>
    /// <param name="editor">Profile workspace editor.</param>
    /// <param name="inputPath">Project root or Jsons/WhenItFails directory.</param>
    /// <param name="profileName">Stable profile name.</param>
    /// <param name="ownerName">Stable owner name or alias.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The edited profile definition.</returns>
    public static async Task<Response<ErrorProfileDefinition>> ProfileAddOwnerAsync(
        this WhenItFailsProfileWorkspaceEditor editor,
        string inputPath,
        string profileName,
        string ownerName,
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

        if (string.IsNullOrWhiteSpace(ownerName))
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "OwnerNameIsEmpty",
                message: "Owner name cannot be empty.");
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

        Response<ErrorOwnerCatalogDocument> ownerLoadResponse =
            await new JsonErrorOwnerCatalogLoader().LoadFromFileAsync(
                options.OwnerCatalogFilePath,
                cancellationToken);

        if (!ownerLoadResponse.IsSuccess || ownerLoadResponse.Data is null)
        {
            return Response<ErrorProfileDefinition>.Fail(
                code: "OwnerCatalogLoadFailed",
                message: string.IsNullOrWhiteSpace(ownerLoadResponse.Message)
                    ? $"Owner catalog could not be loaded: {options.OwnerCatalogFilePath}"
                    : ownerLoadResponse.Message);
        }

        ErrorOwnerCatalogDocument ownerCatalog =
            new ErrorOwnerCatalogDocumentNormalizer().Normalize(ownerLoadResponse.Data);

        string normalizedOwnerName = TextKeyNormalizer.NormalizeKey(ownerName);
        ErrorOwnerDefinition? ownerDefinition = ownerCatalog.Owners.FirstOrDefault(owner =>
            string.Equals(
                owner.Name,
                normalizedOwnerName,
                StringComparison.OrdinalIgnoreCase)
            || owner.Aliases.Any(alias =>
                string.Equals(
                    alias,
                    normalizedOwnerName,
                    StringComparison.OrdinalIgnoreCase)));

        if (ownerDefinition is null)
        {
            return Response<ErrorProfileDefinition>.NotFound(
                code: "OwnerNotFound",
                message: $"Owner '{normalizedOwnerName}' was not found.");
        }

        bool ownerAlreadyIncluded = profileDefinition.IncludeOwners.Any(includedOwner =>
            string.Equals(
                includedOwner,
                ownerDefinition.Name,
                StringComparison.OrdinalIgnoreCase));

        if (ownerAlreadyIncluded)
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileOwnerAlreadyIncluded",
                message: $"Profile '{profileDefinition.Name}' already includes owner '{ownerDefinition.Name}'.");
        }

        profileDefinition.IncludeOwners.Add(ownerDefinition.Name);

        ErrorCatalogValidationResult editedValidationResult =
            new ErrorProfileCatalogValidator().Validate(profileCatalog);

        if (!editedValidationResult.IsValid)
        {
            profileDefinition.IncludeOwners.Remove(ownerDefinition.Name);

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
            profileDefinition.IncludeOwners.Remove(ownerDefinition.Name);

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
            $"Owner '{ownerDefinition.Name}' was added to profile '{profileDefinition.Name}'.");
    }
}
