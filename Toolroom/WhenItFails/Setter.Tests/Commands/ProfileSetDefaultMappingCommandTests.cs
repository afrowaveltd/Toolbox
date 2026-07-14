using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class ProfileSetDefaultMappingCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidArguments_SetsNormalizedMapping()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        Assert.True((await new WhenItFailsProfileWorkspaceEditor().AddProfileAsync(
            workspace.ProjectRootPath,
            "DITA_TEST",
            "DiTa Test")).IsSuccess);

        int exitCode = await ProfileSetDefaultMappingCommand.ExecuteAsync(
        [
            "profile-set-default-mapping",
            workspace.ProjectRootPath,
            " dita test ",
            "web.problemDetails",
            " true "
        ]);

        Assert.Equal(0, exitCode);

        ErrorProfileDefinition profile = await LoadProfileAsync(
            workspace.WhenItFailsJsonsPath,
            "DITA_TEST");
        string key = TextKeyNormalizer.NormalizeKey("web.problemDetails");

        Assert.Equal("true", profile.DefaultMappings[key]);
    }

    [Fact]
    public async Task ExecuteAsync_WithSameMapping_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        WhenItFailsProfileWorkspaceEditor editor = new();

        Assert.True((await editor.AddProfileAsync(
            workspace.ProjectRootPath,
            "DITA_TEST",
            "DiTa Test")).IsSuccess);
        Assert.True((await editor.ProfileSetDefaultMappingAsync(
            workspace.ProjectRootPath,
            "DITA_TEST",
            "web.problemDetails",
            "true")).IsSuccess);

        int exitCode = await ProfileSetDefaultMappingCommand.ExecuteAsync(
        [
            "profile-set-default-mapping",
            workspace.ProjectRootPath,
            "DITA_TEST",
            "WEB_PROBLEMDETAILS",
            "true"
        ]);

        Assert.Equal(2, exitCode);
    }

    public static TheoryData<string[]> InvalidArgumentCases =>
        new()
        {
            new[] { "profile-set-default-mapping" },
            new[] { "profile-set-default-mapping", "." },
            new[] { "profile-set-default-mapping", ".", "DITA_TEST" },
            new[] { "profile-set-default-mapping", ".", "DITA_TEST", "KEY" },
            new[] { "profile-set-default-mapping", ".", "DITA_TEST", "KEY", "VALUE", "EXTRA" }
        };

    [Theory]
    [MemberData(nameof(InvalidArgumentCases))]
    public async Task ExecuteAsync_WithInvalidArguments_ReturnsCommandInputError(string[] args)
    {
        Assert.Equal(1, await ProfileSetDefaultMappingCommand.ExecuteAsync(args));
    }

    private static async Task<ErrorProfileDefinition> LoadProfileAsync(
        string jsonsPath,
        string profileName)
    {
        Response<ErrorProfileCatalogDocument> response =
            await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "profiles.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        ErrorProfileDefinition? profile = new ErrorProfileCatalogDocumentNormalizer()
            .Normalize(response.Data)
            .Profiles
            .FirstOrDefault(candidate => string.Equals(
                candidate.Name,
                profileName,
                StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(profile);
        return profile;
    }
}
