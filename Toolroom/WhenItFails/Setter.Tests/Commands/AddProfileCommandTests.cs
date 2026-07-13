using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class AddProfileCommandTests
{
    public static TheoryData<string[]> MissingRequiredArgumentCases =>
        new()
        {
            new[] { "add-profile" },
            new[] { "add-profile", "." },
            new[] { "add-profile", ".", "DITA" }
        };

    [Fact]
    public async Task ExecuteAsync_WithValidArguments_AddsProfileAndReturnsSuccess()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        int exitCode = await AddProfileCommand.ExecuteAsync(
            [
                "add-profile",
                temporaryWorkspace.ProjectRootPath,
                "dita research",
                "DiTa Research",
                "Profiles disk research tools."
            ]);

        Assert.Equal(0, exitCode);

        ErrorProfileDefinition savedProfile = await LoadProfileAsync(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "DITA_RESEARCH");

        Assert.Equal("DiTa Research", savedProfile.DisplayName);
        Assert.Equal("Profiles disk research tools.", savedProfile.Description);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingProfile_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        int exitCode = await AddProfileCommand.ExecuteAsync(
            [
                "add-profile",
                temporaryWorkspace.ProjectRootPath,
                "WEB",
                "Another Web"
            ]);

        Assert.Equal(2, exitCode);
    }

    [Theory]
    [MemberData(nameof(MissingRequiredArgumentCases))]
    public async Task ExecuteAsync_WithMissingRequiredArgument_ReturnsCommandInputError(
        string[] args)
    {
        int exitCode = await AddProfileCommand.ExecuteAsync(args);

        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithTooManyArguments_ReturnsCommandInputError()
    {
        int exitCode = await AddProfileCommand.ExecuteAsync(
            [
                "add-profile",
                ".",
                "DITA",
                "DiTa",
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
