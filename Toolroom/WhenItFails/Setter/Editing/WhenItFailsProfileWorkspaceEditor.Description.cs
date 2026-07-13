using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;

/// <summary>
/// Provides profile description editing for <see cref="WhenItFailsProfileWorkspaceEditor"/>.
/// </summary>
internal static class WhenItFailsProfileWorkspaceEditorDescriptionExtensions
{
    /// <summary>
    /// Sets or clears the optional description of one profile.
    /// </summary>
    /// <param name="editor">Profile workspace editor.</param>
    /// <param name="inputPath">Project root or Jsons/WhenItFails directory.</param>
    /// <param name="name">Stable profile name.</param>
    /// <param name="newDescription">New description, or null/whitespace to clear it.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The edited profile definition.</returns>
    public static async Task<Response<ErrorProfileDefinition>> SetProfileDescriptionAsync(
        this WhenItFailsProfileWorkspaceEditor editor,
        string inputPath,
        string name,
        string? newDescription,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(editor);
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        if (string.IsNullOrWhiteSpace(name))
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileNameIsEmpty",
                message: "Profile name cannot be empty.");
        }

        JsonsOptions options =
            WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(inputPath);

        JsonErrorProfileCatalogLoader loader = new();

        Response<ErrorProfileCatalogDocument> loadResponse =
            await loader.LoadFromFileAsync(
                options.ProfilesFilePath,
                cancellationToken);

        if (!loadResponse.IsSuccess || loadResponse.Data is null)
        {
            return Response<ErrorProfileDefinition>.Fail(
                code: "ProfileCatalogLoadFailed",
                message: string.IsNullOrWhiteSpace(loadResponse.Message)
                    ? $"Profile catalog could not be loaded: {options.ProfilesFilePath}"
                    : loadResponse.Message);
        }

        ErrorProfileCatalogDocument normalizedDocument =
            new ErrorProfileCatalogDocumentNormalizer().Normalize(loadResponse.Data);

        ErrorCatalogValidationResult originalValidationResult =
            new ErrorProfileCatalogValidator().Validate(normalizedDocument);

        if (!originalValidationResult.IsValid)
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileCatalogIsInvalid",
                message: "The profile catalog is invalid and cannot be edited safely.");
        }

        string normalizedName = TextKeyNormalizer.NormalizeKey(name);

        ErrorProfileDefinition? profileDefinition =
            normalizedDocument.Profiles.FirstOrDefault(profile =>
                string.Equals(
                    profile.Name,
                    normalizedName,
                    StringComparison.OrdinalIgnoreCase));

        if (profileDefinition is null)
        {
            return Response<ErrorProfileDefinition>.NotFound(
                code: "ProfileNotFound",
                message: $"Profile '{normalizedName}' was not found.");
        }

        string? oldDescription = profileDefinition.Description;
        string? normalizedDescription = string.IsNullOrWhiteSpace(newDescription)
            ? null
            : TextKeyNormalizer.NormalizeDisplayName(newDescription);

        profileDefinition.Description = normalizedDescription;

        ErrorCatalogValidationResult editedValidationResult =
            new ErrorProfileCatalogValidator().Validate(normalizedDocument);

        if (!editedValidationResult.IsValid)
        {
            profileDefinition.Description = oldDescription;

            return Response<ErrorProfileDefinition>.Invalid(
                code: "EditedProfileCatalogIsInvalid",
                message: "The edited profile catalog is invalid and was not saved.");
        }

        JsonCatalogDocumentWriter writer = new();

        Response saveResponse =
            await writer.SaveToFileAsync(
                normalizedDocument,
                options.ProfilesFilePath,
                cancellationToken);

        if (!saveResponse.IsSuccess)
        {
            profileDefinition.Description = oldDescription;

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

        string oldDescriptionText = oldDescription ?? "<empty>";
        string newDescriptionText = normalizedDescription ?? "<empty>";

        return Response<ErrorProfileDefinition>.Ok(
            profileDefinition,
            $"Profile '{profileDefinition.Name}' description changed from '{oldDescriptionText}' to '{newDescriptionText}'.");
    }
}
