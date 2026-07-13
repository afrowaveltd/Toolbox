using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;

/// <summary>
/// Provides safe write operations for project-local WhenItFails profiles.
/// </summary>
internal sealed class WhenItFailsProfileWorkspaceEditor
{
    /// <summary>
    /// Adds a new project profile to the workspace profile catalog.
    /// </summary>
    /// <param name="inputPath">Project root or Jsons/WhenItFails directory.</param>
    /// <param name="name">Stable profile name.</param>
    /// <param name="displayName">Human-readable profile name.</param>
    /// <param name="description">Optional profile description.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The added profile definition.</returns>
    public async Task<Response<ErrorProfileDefinition>> AddProfileAsync(
        string inputPath,
        string name,
        string displayName,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(inputPath);

        if (string.IsNullOrWhiteSpace(name))
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileNameIsEmpty",
                message: "Profile name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileDisplayNameIsEmpty",
                message: "Profile display name cannot be empty.");
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

        bool profileExists = normalizedDocument.Profiles.Any(profile =>
            string.Equals(
                profile.Name,
                normalizedName,
                StringComparison.OrdinalIgnoreCase));

        if (profileExists)
        {
            return Response<ErrorProfileDefinition>.Invalid(
                code: "ProfileAlreadyExists",
                message: $"Profile '{normalizedName}' already exists.");
        }

        ErrorProfileDefinition profileDefinition =
            new ErrorProfileDefinitionNormalizer().Normalize(
                new ErrorProfileDefinition
                {
                    Name = normalizedName,
                    DisplayName = displayName,
                    Description = description,
                    Source = "Project"
                });

        normalizedDocument.Profiles.Add(profileDefinition);

        ErrorCatalogValidationResult editedValidationResult =
            new ErrorProfileCatalogValidator().Validate(normalizedDocument);

        if (!editedValidationResult.IsValid)
        {
            normalizedDocument.Profiles.Remove(profileDefinition);

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
            normalizedDocument.Profiles.Remove(profileDefinition);

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
            $"Profile '{profileDefinition.Name}' was added.");
    }
}
