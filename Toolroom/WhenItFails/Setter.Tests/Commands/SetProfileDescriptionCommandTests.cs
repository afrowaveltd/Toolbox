using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class SetProfileDescriptionCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidArguments_UpdatesDescriptionAndReturnsSuccess()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        int exitCode = await SetProfileDescriptionCommand.ExecuteAsync(
            [
                "set-profile-description",
                temporaryWorkspace.ProjectRootPath,
                "  web  ",
                "  Web-facing applications and services.  "
            ]);

        Assert.Equal(0, exitCode);

        ErrorProfileDefinition savedProfile = await LoadProfileAsync(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "WEB");

        Assert.Equal(
            "Web-facing applications and services.",
            savedProfile.Description);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyDescription_ClearsDescriptionAndReturnsSuccess()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        int setExitCode = await SetProfileDescriptionCommand.ExecuteAsync(
            [
                "set-profile-description",
                temporaryWorkspace.ProjectRootPath,
                "WEB",
                "Temporary description"
            ]);

        Assert.Equal(0, setExitCode);

        int clearExitCode = await SetProfileDescriptionCommand.ExecuteAsync(
            [
                "set-profile-description",
                temporaryWorkspace.ProjectRootPath,
                "WEB",
                string.Empty
            ]);

        Assert.Equal(0, clearExitCode);

        ErrorProfileDefinition savedProfile = await LoadProfileAsync(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "WEB");

        Assert.Null(savedProfile.Description);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownProfile_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        int exitCode = await SetProfileDescriptionCommand.ExecuteAsync(
            [
                "set-profile-description",
                temporaryWorkspace.ProjectRootPath,
                "DOES_NOT_EXIST",
                "Description"
            ]);

        Assert.Equal(2, exitCode);
    }

    public static TheoryData<string[]> MissingRequiredArguments =>
        new()
        {
            new[] { "set-profile-description" },
            new[] { "set-profile-description", "." },
            new[] { "set-profile-description", ".", "WEB" }
        };

    [Theory]
    [MemberData(nameof(MissingRequiredArguments))]
    public async Task ExecuteAsync_WithMissingRequiredArgument_ReturnsCommandInputError(
        string[] args)
    {
        int exitCode = await SetProfileDescriptionCommand.ExecuteAsync(args);

        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithTooManyArguments_ReturnsCommandInputError()
    {
        int exitCode = await SetProfileDescriptionCommand.ExecuteAsync(
            [
                "set-profile-description",
                ".",
                "WEB",
                "Description",
                "Unexpected"
            ]);

        Assert.Equal(1, exitCode);
    }

    private static async Task<ErrorProfileDefinition> LoadProfileAsync(
        string whenItFailsJsonsPath,
        string profileName)
    {
        string profileCatalogFilePath = Path.Combine(
            whenItFailsJsonsPath,
            "profiles.json");

        Response<ErrorProfileCatalogDocument> loadResponse =
            await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(profileCatalogFilePath);

        Assert.True(loadResponse.IsSuccess);
        Assert.NotNull(loadResponse.Data);

        ErrorProfileCatalogDocument profileCatalog =
            new ErrorProfileCatalogDocumentNormalizer().Normalize(loadResponse.Data);

        ErrorProfileDefinition? profile = profileCatalog.Profiles.FirstOrDefault(candidate =>
            string.Equals(
                candidate.Name,
                profileName,
                StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(profile);
        return profile;
    }
}
