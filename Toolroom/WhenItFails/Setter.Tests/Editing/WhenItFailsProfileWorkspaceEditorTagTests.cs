using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Editing;

public sealed class WhenItFailsProfileWorkspaceEditorTagTests
{
    [Fact]
    public async Task ProfileAddTagAsync_WithExistingTag_AddsCanonicalValue()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        Response<ErrorProfileDefinition> addProfile = await editor.AddProfileAsync(
            workspace.ProjectRootPath,
            "DITA_TEST",
            "DiTa Test");

        Assert.True(addProfile.IsSuccess);

        int backupsBefore = GetBackupCount(workspace.WhenItFailsJsonsPath);
        string tag = await LoadFirstTagAsync(workspace.WhenItFailsJsonsPath);

        Response<ErrorProfileDefinition> response = await editor.ProfileAddTagAsync(
            workspace.ProjectRootPath,
            " dita test ",
            tag.ToLowerInvariant());

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        string canonicalTag = TextKeyNormalizer.NormalizeKey(tag);
        Assert.Contains(canonicalTag, response.Data.IncludeTags);

        ErrorProfileDefinition savedProfile = await LoadProfileAsync(
            workspace.WhenItFailsJsonsPath,
            "DITA_TEST");

        Assert.Contains(canonicalTag, savedProfile.IncludeTags);
        Assert.Equal(backupsBefore + 1, GetBackupCount(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ProfileAddTagAsync_WithDuplicate_ReturnsInvalidWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        Assert.True((await editor.AddProfileAsync(
            workspace.ProjectRootPath,
            "DITA_TEST",
            "DiTa Test")).IsSuccess);

        string tag = await LoadFirstTagAsync(workspace.WhenItFailsJsonsPath);
        Assert.True((await editor.ProfileAddTagAsync(
            workspace.ProjectRootPath,
            "DITA_TEST",
            tag)).IsSuccess);

        int backupsBefore = GetBackupCount(workspace.WhenItFailsJsonsPath);
        Response<ErrorProfileDefinition> response = await editor.ProfileAddTagAsync(
            workspace.ProjectRootPath,
            "DITA_TEST",
            tag);

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "ProfileTagAlreadyIncluded");
        Assert.Equal(backupsBefore, GetBackupCount(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ProfileAddTagAsync_WithUnknownTag_ReturnsNotFoundWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        Assert.True((await editor.AddProfileAsync(
            workspace.ProjectRootPath,
            "DITA_TEST",
            "DiTa Test")).IsSuccess);

        int backupsBefore = GetBackupCount(workspace.WhenItFailsJsonsPath);
        Response<ErrorProfileDefinition> response = await editor.ProfileAddTagAsync(
            workspace.ProjectRootPath,
            "DITA_TEST",
            "DOES_NOT_EXIST");

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "TagNotFound");
        Assert.Equal(backupsBefore, GetBackupCount(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task ProfileAddTagAsync_WithUnknownProfile_ReturnsNotFound()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        string tag = await LoadFirstTagAsync(workspace.WhenItFailsJsonsPath);
        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().ProfileAddTagAsync(
                workspace.ProjectRootPath,
                "DOES_NOT_EXIST",
                tag);

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "ProfileNotFound");
    }

    [Theory]
    [InlineData("", "NETWORK")]
    [InlineData("DITA_TEST", "")]
    public async Task ProfileAddTagAsync_WithEmptyValue_ReturnsInvalid(
        string profileName,
        string tagName)
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().ProfileAddTagAsync(
                workspace.ProjectRootPath,
                profileName,
                tagName);

        Assert.False(response.IsSuccess);
    }

    private static int GetBackupCount(string jsonsPath) =>
        Directory.GetFiles(jsonsPath, "profiles.*.bak.json").Length;

    private static async Task<string> LoadFirstTagAsync(string jsonsPath)
    {
        string filePath = Path.Combine(jsonsPath, "errors.en.json");
        Response<ErrorCatalogDocument> response =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(filePath);

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        ErrorCatalogDocument catalog =
            new ErrorCatalogDocumentNormalizer().Normalize(response.Data);

        string? tag = catalog.Errors
            .SelectMany(error => error.Tags)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

        Assert.False(string.IsNullOrWhiteSpace(tag));
        return tag;
    }

    private static async Task<ErrorProfileDefinition> LoadProfileAsync(
        string jsonsPath,
        string profileName)
    {
        string filePath = Path.Combine(jsonsPath, "profiles.json");
        Response<ErrorProfileCatalogDocument> response =
            await new JsonErrorProfileCatalogLoader().LoadFromFileAsync(filePath);

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        ErrorProfileCatalogDocument catalog =
            new ErrorProfileCatalogDocumentNormalizer().Normalize(response.Data);

        ErrorProfileDefinition? profile = catalog.Profiles.FirstOrDefault(candidate =>
            string.Equals(candidate.Name, profileName, StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(profile);
        return profile;
    }
}
