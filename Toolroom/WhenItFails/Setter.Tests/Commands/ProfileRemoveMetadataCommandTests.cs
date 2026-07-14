using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class ProfileRemoveMetadataCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidArguments_RemovesMetadata()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        WhenItFailsProfileWorkspaceEditor editor = new();

        Assert.True((await editor.AddProfileAsync(
            workspace.ProjectRootPath,
            "DITA_TEST",
            "DiTa Test")).IsSuccess);
        Assert.True((await editor.ProfileSetMetadataAsync(
            workspace.ProjectRootPath,
            "DITA_TEST",
            "documentation.owner",
            "DiTa Team")).IsSuccess);

        int exitCode = await ProfileRemoveMetadataCommand.ExecuteAsync(
        [
            "profile-remove-metadata",
            workspace.ProjectRootPath,
            " dita test ",
            "DOCUMENTATION-OWNER"
        ]);

        Assert.Equal(0, exitCode);

        ErrorProfileDefinition profile = await LoadProfileAsync(
            workspace.WhenItFailsJsonsPath,
            "DITA_TEST");
        string key = TextKeyNormalizer.NormalizeKey("documentation.owner");

        Assert.False(profile.Metadata.TryGet(key, out _));
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownMetadata_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        Assert.True((await new WhenItFailsProfileWorkspaceEditor().AddProfileAsync(
            workspace.ProjectRootPath,
            "DITA_TEST",
            "DiTa Test")).IsSuccess);

        int exitCode = await ProfileRemoveMetadataCommand.ExecuteAsync(
        [
            "profile-remove-metadata",
            workspace.ProjectRootPath,
            "DITA_TEST",
            "DOES_NOT_EXIST"
        ]);

        Assert.Equal(2, exitCode);
    }

    public static TheoryData<string[]> InvalidArgumentCases =>
        new()
        {
            new[] { "profile-remove-metadata" },
            new[] { "profile-remove-metadata", "." },
            new[] { "profile-remove-metadata", ".", "DITA_TEST" },
            new[] { "profile-remove-metadata", ".", "DITA_TEST", "KEY", "EXTRA" }
        };

    [Theory]
    [MemberData(nameof(InvalidArgumentCases))]
    public async Task ExecuteAsync_WithInvalidArguments_ReturnsCommandInputError(string[] args)
    {
        Assert.Equal(1, await ProfileRemoveMetadataCommand.ExecuteAsync(args));
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
