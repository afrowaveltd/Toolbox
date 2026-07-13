using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class SetProfileDisplayNameCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidArguments_UpdatesDisplayNameAndReturnsSuccess()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        int exitCode = await SetProfileDisplayNameCommand.ExecuteAsync(
            [
                "set-profile-display-name",
                temporaryWorkspace.ProjectRootPath,
                "  web  ",
                "  Web Applications  "
            ]);

        Assert.Equal(0, exitCode);

        ErrorProfileDefinition savedProfile = await LoadProfileAsync(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "WEB");

        Assert.Equal("Web Applications", savedProfile.DisplayName);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownProfile_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        int exitCode = await SetProfileDisplayNameCommand.ExecuteAsync(
            [
                "set-profile-display-name",
                temporaryWorkspace.ProjectRootPath,
                "DOES_NOT_EXIST",
                "Unknown Profile"
            ]);

        Assert.Equal(2, exitCode);
    }

    public static TheoryData<string[]> MissingRequiredArguments =>
        new()
        {
            new[] { "set-profile-display-name" },
            new[] { "set-profile-display-name", "." },
            new[] { "set-profile-display-name", ".", "WEB" }
        };

    [Theory]
    [MemberData(nameof(MissingRequiredArguments))]
    public async Task ExecuteAsync_WithMissingRequiredArgument_ReturnsCommandInputError(
        string[] args)
    {
        int exitCode = await SetProfileDisplayNameCommand.ExecuteAsync(args);

        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithTooManyArguments_ReturnsCommandInputError()
    {
        int exitCode = await SetProfileDisplayNameCommand.ExecuteAsync(
            [
                "set-profile-display-name",
                ".",
                "WEB",
                "Web Applications",
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
