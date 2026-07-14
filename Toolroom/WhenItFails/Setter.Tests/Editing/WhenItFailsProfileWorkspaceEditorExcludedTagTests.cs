using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Editing;

public sealed class WhenItFailsProfileWorkspaceEditorExcludedTagTests
{
    [Fact]
    public async Task ProfileAddExcludedTagAsync_WithExistingTag_AddsCanonicalValue()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        Assert.True((await editor.AddProfileAsync(
            workspace.ProjectRootPath,
            "DITA_TEST",
            "DiTa Test")).IsSuccess);

        string tag = await LoadFirstTagAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorProfileDefinition> response =
            await editor.ProfileAddExcludedTagAsync(
                workspace.ProjectRootPath,
                " dita test ",
                tag.ToLowerInvariant());

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Contains(TextKeyNormalizer.NormalizeKey(tag), response.Data.ExcludeTags);

        ErrorProfileDefinition savedProfile = await LoadProfileAsync(
            workspace.WhenItFailsJsonsPath,
            "DITA_TEST");
        Assert.Contains(TextKeyNormalizer.NormalizeKey(tag), savedProfile.ExcludeTags);
        Assert.Equal(backupsBefore + 1, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ProfileAddExcludedTagAsync_WithDuplicate_ReturnsInvalidWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        Assert.True((await editor.AddProfileAsync(
            workspace.ProjectRootPath,
            "DITA_TEST",
            "DiTa Test")).IsSuccess);

        string tag = await LoadFirstTagAsync(workspace.WhenItFailsJsonsPath);
        Assert.True((await editor.ProfileAddExcludedTagAsync(
            workspace.ProjectRootPath,
            "DITA_TEST",
            tag)).IsSuccess);

        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);
        Response<ErrorProfileDefinition> response =
            await editor.ProfileAddExcludedTagAsync(
                workspace.ProjectRootPath,
                "DITA_TEST",
                tag);

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "ProfileTagAlreadyExcluded");
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ProfileAddExcludedTagAsync_WithUnknownTag_ReturnsNotFoundWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        Assert.True((await editor.AddProfileAsync(
            workspace.ProjectRootPath,
            "DITA_TEST",
            "DiTa Test")).IsSuccess);

        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);
        Response<ErrorProfileDefinition> response =
            await editor.ProfileAddExcludedTagAsync(
                workspace.ProjectRootPath,
                "DITA_TEST",
                "DOES_NOT_EXIST");

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "TagNotFound");
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ProfileAddExcludedTagAsync_WithUnknownProfile_ReturnsNotFound()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        string tag = await LoadFirstTagAsync(workspace.WhenItFailsJsonsPath);
        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().ProfileAddExcludedTagAsync(
                workspace.ProjectRootPath,
                "DOES_NOT_EXIST",
                tag);

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "ProfileNotFound");
    }

    [Theory]
    [InlineData("", "NETWORK")]
    [InlineData("DITA_TEST", "")]
    public async Task ProfileAddExcludedTagAsync_WithEmptyValue_ReturnsInvalid(
        string profileName,
        string tagName)
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().ProfileAddExcludedTagAsync(
                workspace.ProjectRootPath,
                profileName,
                tagName);

        Assert.False(response.IsSuccess);
    }

    private static int CountBackups(string path) =>
        Directory.GetFiles(path, "profiles.*.bak.json").Length;

    private static async Task<string> LoadFirstTagAsync(string path)
    {
        Response<ErrorCatalogDocument> response =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                Path.Combine(path, "errors.en.json"));

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        string? tag = new ErrorCatalogDocumentNormalizer()
            .Normalize(response.Data)
            .Errors
            .SelectMany(error => error.Tags)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

        Assert.False(string.IsNullOrWhiteSpace(tag));
        return tag;
    }

    private static async Task<ErrorProfileDefinition> LoadProfileAsync(
        string path,
        string profileName)
    {
        Response<ErrorProfileCatalogDocument> response =
            await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(
                Path.Combine(path, "profiles.json"));

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
