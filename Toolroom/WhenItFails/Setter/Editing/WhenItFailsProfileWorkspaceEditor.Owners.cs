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
    public static async Task<Response<ErrorProfileDefinition>> ProfileAddOwnerAsync(
        this WhenItFailsProfileWorkspaceEditor editor,
        string inputPath,
        string profileName,
        string ownerName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editor);

        Response<ProfileOwnerEditContext> contextResponse =
            await LoadContextAsync(
                inputPath,
                profileName,
                ownerName,
                cancellationToken);

        if (!contextResponse.IsSuccess || contextResponse.Data is null)
        {
            return CopyFailure<ErrorProfileDefinition>(contextResponse);
        }

        ProfileOwnerEditContext context = contextResponse.Data;

        bool ownerAlreadyIncluded = context.ProfileDefinition.IncludeOwners.Any(includedOwner =>
            string.Equals(
                includedOwner,
                context.OwnerDefinition.Name,
                StringComparison.OrdinalIgnoreCase));

        if (ownerAlreadyIncluded)
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileOwnerAlreadyIncluded",
                message: $"Profile '{context.ProfileDefinition.Name}' already includes owner '{context.OwnerDefinition.Name}'.");
        }

        context.ProfileDefinition.IncludeOwners.Add(context.OwnerDefinition.Name);

        Response<ErrorProfileDefinition>? saveFailure = await ValidateAndSaveAsync(
            context,
            rollback: () => context.ProfileDefinition.IncludeOwners.Remove(context.OwnerDefinition.Name),
            cancellationToken);

        if (saveFailure is not null)
        {
            return saveFailure;
        }

        return Response<ErrorProfileDefinition>.Ok(
            context.ProfileDefinition,
            $"Owner '{context.OwnerDefinition.Name}' was added to profile '{context.ProfileDefinition.Name}'.");
    }

    /// <summary>
    /// Removes an included workspace owner from one profile.
    /// </summary>
    public static async Task<Response<ErrorProfileDefinition>> ProfileRemoveOwnerAsync(
        this WhenItFailsProfileWorkspaceEditor editor,
        string inputPath,
        string profileName,
        string ownerName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editor);

        Response<ProfileOwnerEditContext> contextResponse =
            await LoadContextAsync(
                inputPath,
                profileName,
                ownerName,
                cancellationToken);

        if (!contextResponse.IsSuccess || contextResponse.Data is null)
        {
            return CopyFailure<ErrorProfileDefinition>(contextResponse);
        }

        ProfileOwnerEditContext context = contextResponse.Data;

        int ownerIndex = context.ProfileDefinition.IncludeOwners.FindIndex(includedOwner =>
            string.Equals(
                includedOwner,
                context.OwnerDefinition.Name,
                StringComparison.OrdinalIgnoreCase));

        if (ownerIndex < 0)
        {
            return Response<ErrorProfileDefinition>.NotFound(
                code: "ProfileOwnerNotIncluded",
                message: $"Profile '{context.ProfileDefinition.Name}' does not include owner '{context.OwnerDefinition.Name}'.");
        }

        string removedOwnerName = context.ProfileDefinition.IncludeOwners[ownerIndex];
        context.ProfileDefinition.IncludeOwners.RemoveAt(ownerIndex);

        Response<ErrorProfileDefinition>? saveFailure = await ValidateAndSaveAsync(
            context,
            rollback: () => context.ProfileDefinition.IncludeOwners.Insert(ownerIndex, removedOwnerName),
            cancellationToken);

        if (saveFailure is not null)
        {
            return saveFailure;
        }

        return Response<ErrorProfileDefinition>.Ok(
            context.ProfileDefinition,
            $"Owner '{context.OwnerDefinition.Name}' was removed from profile '{context.ProfileDefinition.Name}'.");
    }

    private static async Task<Response<ProfileOwnerEditContext>> LoadContextAsync(
        string inputPath,
        string profileName,
        string ownerName,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        if (string.IsNullOrWhiteSpace(profileName))
        {
            return Response<ProfileOwnerEditContext>.Invalid(
                code: "ProfileNameIsEmpty",
                message: "Profile name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(ownerName))
        {
            return Response<ProfileOwnerEditContext>.Invalid(
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
            return Response<ProfileOwnerEditContext>.Fail(
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
            return Response<ProfileOwnerEditContext>.Invalid(
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
            return Response<ProfileOwnerEditContext>.NotFound(
                code: "ProfileNotFound",
                message: $"Profile '{normalizedProfileName}' was not found.");
        }

        Response<ErrorOwnerCatalogDocument> ownerLoadResponse =
            await new JsonErrorOwnerCatalogLoader().LoadFromFileAsync(
                options.OwnerCatalogFilePath,
                cancellationToken);

        if (!ownerLoadResponse.IsSuccess || ownerLoadResponse.Data is null)
        {
            return Response<ProfileOwnerEditContext>.Fail(
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
            return Response<ProfileOwnerEditContext>.NotFound(
                code: "OwnerNotFound",
                message: $"Owner '{normalizedOwnerName}' was not found.");
        }

        return Response<ProfileOwnerEditContext>.Ok(
            new ProfileOwnerEditContext(
                options.ProfilesFilePath,
                profileCatalog,
                profileDefinition,
                ownerDefinition));
    }

    private static async Task<Response<ErrorProfileDefinition>?> ValidateAndSaveAsync(
        ProfileOwnerEditContext context,
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
            : "ProfileOwnerEditFailed";

        return Response<TTarget>.Fail(
            code: code,
            message: response.Message);
    }

    private sealed record ProfileOwnerEditContext(
        string ProfileCatalogFilePath,
        ErrorProfileCatalogDocument ProfileCatalog,
        ErrorProfileDefinition ProfileDefinition,
        ErrorOwnerDefinition OwnerDefinition);
}