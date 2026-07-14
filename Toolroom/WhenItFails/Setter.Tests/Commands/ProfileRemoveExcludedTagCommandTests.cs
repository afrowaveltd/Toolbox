using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class ProfileRemoveExcludedTagCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidArguments_RemovesExcludedTagAndReturnsSuccess()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        WhenItFailsProfileWorkspaceEditor editor = new();
        Assert.True((await editor.AddProfileAsync(workspace.ProjectRootPath, "DITA_TEST", "DiTa Test")).IsSuccess);
        string tag = await LoadFirstTagAsync(workspace.WhenItFailsJsonsPath);
        Assert.True((await editor.ProfileAddExcludedTagAsync(workspace.ProjectRootPath, "DITA_TEST", tag)).IsSuccess);

        int exitCode = await ProfileRemoveExcludedTagCommand.ExecuteAsync(
            ["profile-remove-excluded-tag", workspace.ProjectRootPath, " dita test ", tag.ToLowerInvariant()]);

        Assert.Equal(0, exitCode);
        ErrorProfileDefinition profile = await LoadProfileAsync(workspace.WhenItFailsJsonsPath, "DITA_TEST");
        Assert.DoesNotContain(TextKeyNormalizer.NormalizeKey(tag), profile.ExcludeTags);
    }

    [Fact]
    public async Task ExecuteAsync_WithTagNotExcluded_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        WhenItFailsProfileWorkspaceEditor editor = new();
        Assert.True((await editor.AddProfileAsync(workspace.ProjectRootPath, "DITA_TEST", "DiTa Test")).IsSuccess);
        string tag = await LoadFirstTagAsync(workspace.WhenItFailsJsonsPath);

        int exitCode = await ProfileRemoveExcludedTagCommand.ExecuteAsync(
            ["profile-remove-excluded-tag", workspace.ProjectRootPath, "DITA_TEST", tag]);

        Assert.Equal(2, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownTag_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        Assert.True((await new WhenItFailsProfileWorkspaceEditor().AddProfileAsync(
            workspace.ProjectRootPath,
            "DITA_TEST",
            "DiTa Test")).IsSuccess);

        int exitCode = await ProfileRemoveExcludedTagCommand.ExecuteAsync(
            ["profile-remove-excluded-tag", workspace.ProjectRootPath, "DITA_TEST", "DOES_NOT_EXIST"]);

        Assert.Equal(2, exitCode);
    }

    public static TheoryData<string[]> MissingArgumentCases =>
        new()
        {
            new[] { "profile-remove-excluded-tag" },
            new[] { "profile-remove-excluded-tag", "." },
            new[] { "profile-remove-excluded-tag", ".", "DITA_TEST" }
        };

    [Theory]
    [MemberData(nameof(MissingArgumentCases))]
    public async Task ExecuteAsync_WithMissingRequiredArgument_ReturnsCommandInputError(string[] args)
    {
        Assert.Equal(1, await ProfileRemoveExcludedTagCommand.ExecuteAsync(args));
    }

    [Fact]
    public async Task ExecuteAsync_WithTooManyArguments_ReturnsCommandInputError()
    {
        Assert.Equal(
            1,
            await ProfileRemoveExcludedTagCommand.ExecuteAsync(
                ["profile-remove-excluded-tag", ".", "DITA_TEST", "RETRYABLE", "Unexpected"]));
    }

    private static async Task<string> LoadFirstTagAsync(string jsonsPath)
    {
        Response<ErrorCatalogDocument> response = await new JsonErrorCatalogLoader()
            .LoadFromFileAsync(Path.Combine(jsonsPath, "errors.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        ErrorCatalogDocument catalog = new ErrorCatalogDocumentNormalizer().Normalize(response.Data);
        string? tag = catalog.Errors.SelectMany(error => error.Tags)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        Assert.False(string.IsNullOrWhiteSpace(tag));
        return tag;
    }

    private static async Task<ErrorProfileDefinition> LoadProfileAsync(string jsonsPath, string profileName)
    {
        Response<ErrorProfileCatalogDocument> response = await new JsonErrorProfileCatalogLoader()
            .LoadFromFileAsync(Path.Combine(jsonsPath, "profiles.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        ErrorProfileCatalogDocument catalog = new ErrorProfileCatalogDocumentNormalizer().Normalize(response.Data);
        ErrorProfileDefinition? profile = catalog.Profiles.FirstOrDefault(candidate =>
            string.Equals(candidate.Name, profileName, StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(profile);
        return profile;
    }
}
