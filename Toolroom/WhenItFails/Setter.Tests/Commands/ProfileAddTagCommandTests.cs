using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class ProfileAddTagCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidArguments_AddsTagAndReturnsSuccess()
    {
        using TemporaryWhenItFailsWorkspace workspace = await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        WhenItFailsProfileWorkspaceEditor editor = new();
        Assert.True((await editor.AddProfileAsync(workspace.ProjectRootPath, "DITA_TEST", "DiTa Test")).IsSuccess);

        string tag = await LoadFirstTagAsync(workspace.WhenItFailsJsonsPath);
        int exitCode = await ProfileAddTagCommand.ExecuteAsync([
            "profile-add-tag", workspace.ProjectRootPath, " dita test ", tag.ToLowerInvariant()]);

        Assert.Equal(0, exitCode);
        ErrorProfileDefinition profile = await LoadProfileAsync(workspace.WhenItFailsJsonsPath);
        Assert.Contains(TextKeyNormalizer.NormalizeKey(tag), profile.IncludeTags);
    }

    [Fact]
    public async Task ExecuteAsync_WithTagAlreadyIncluded_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace = await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        WhenItFailsProfileWorkspaceEditor editor = new();
        Assert.True((await editor.AddProfileAsync(workspace.ProjectRootPath, "DITA_TEST", "DiTa Test")).IsSuccess);
        string tag = await LoadFirstTagAsync(workspace.WhenItFailsJsonsPath);
        Assert.True((await editor.ProfileAddTagAsync(workspace.ProjectRootPath, "DITA_TEST", tag)).IsSuccess);

        Assert.Equal(2, await ProfileAddTagCommand.ExecuteAsync([
            "profile-add-tag", workspace.ProjectRootPath, "DITA_TEST", tag]));
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownTag_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace = await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        WhenItFailsProfileWorkspaceEditor editor = new();
        Assert.True((await editor.AddProfileAsync(workspace.ProjectRootPath, "DITA_TEST", "DiTa Test")).IsSuccess);

        Assert.Equal(2, await ProfileAddTagCommand.ExecuteAsync([
            "profile-add-tag", workspace.ProjectRootPath, "DITA_TEST", "DOES_NOT_EXIST"]));
    }

    public static TheoryData<string[]> MissingArgumentCases => new()
    {
        new[] { "profile-add-tag" },
        new[] { "profile-add-tag", "." },
        new[] { "profile-add-tag", ".", "DITA_TEST" }
    };

    [Theory]
    [MemberData(nameof(MissingArgumentCases))]
    public async Task ExecuteAsync_WithMissingRequiredArgument_ReturnsCommandInputError(string[] args) =>
        Assert.Equal(1, await ProfileAddTagCommand.ExecuteAsync(args));

    [Fact]
    public async Task ExecuteAsync_WithTooManyArguments_ReturnsCommandInputError() =>
        Assert.Equal(1, await ProfileAddTagCommand.ExecuteAsync([
            "profile-add-tag", ".", "DITA_TEST", "NETWORK", "Unexpected"]));

    private static async Task<string> LoadFirstTagAsync(string path)
    {
        Response<ErrorCatalogDocument> response = await new JsonErrorCatalogLoader().LoadFromFileAsync(Path.Combine(path, "errors.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        string? tag = new ErrorCatalogDocumentNormalizer().Normalize(response.Data).Errors
            .SelectMany(error => error.Tags)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        Assert.False(string.IsNullOrWhiteSpace(tag));
        return tag;
    }

    private static async Task<ErrorProfileDefinition> LoadProfileAsync(string path)
    {
        Response<ErrorProfileCatalogDocument> response = await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(Path.Combine(path, "profiles.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        ErrorProfileDefinition? profile = new ErrorProfileCatalogDocumentNormalizer().Normalize(response.Data).Profiles
            .FirstOrDefault(candidate => candidate.Name == "DITA_TEST");
        Assert.NotNull(profile);
        return profile;
    }
}
