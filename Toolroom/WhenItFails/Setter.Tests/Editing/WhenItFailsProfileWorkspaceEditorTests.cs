using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Editing;

public sealed class WhenItFailsProfileWorkspaceEditorTests
{
    [Fact]
    public async Task AddProfileAsync_ShouldAddNormalizedProfileAndCreateBackup()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();

        Response<ErrorProfileDefinition> response =
            await editor.AddProfileAsync(
                temporaryWorkspace.ProjectRootPath,
                "  dita research  ",
                "  DiTa Research  ",
                "  Profiles disk research tools.  ");

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal("DITA_RESEARCH", response.Data.Name);
        Assert.Equal("DiTa Research", response.Data.DisplayName);
        Assert.Equal("Profiles disk research tools.", response.Data.Description);
        Assert.Equal("Project", response.Data.Source);

        ErrorProfileDefinition savedProfile =
            await LoadProfileAsync(
                temporaryWorkspace.WhenItFailsJsonsPath,
                "DITA_RESEARCH");

        Assert.Equal("DiTa Research", savedProfile.DisplayName);
        Assert.Equal("Profiles disk research tools.", savedProfile.Description);
        AssertBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task AddProfileAsync_ShouldReturnInvalid_WhenProfileAlreadyExists()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();

        Response<ErrorProfileDefinition> response =
            await editor.AddProfileAsync(
                temporaryWorkspace.ProjectRootPath,
                "web",
                "Another Web Profile");

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "ProfileAlreadyExists");
        AssertNoBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task AddProfileAsync_ShouldReturnInvalid_WhenNameIsEmpty()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();

        Response<ErrorProfileDefinition> response =
            await editor.AddProfileAsync(
                temporaryWorkspace.ProjectRootPath,
                "   ",
                "Empty Name");

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "ProfileNameIsEmpty");
        AssertNoBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task AddProfileAsync_ShouldReturnInvalid_WhenDisplayNameIsEmpty()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();

        Response<ErrorProfileDefinition> response =
            await editor.AddProfileAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA",
                "   ");

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "ProfileDisplayNameIsEmpty");
        AssertNoBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    private static async Task<ErrorProfileDefinition> LoadProfileAsync(
        string whenItFailsJsonsPath,
        string profileName)
    {
        string profileCatalogFilePath = Path.Combine(
            whenItFailsJsonsPath,
            "profiles.json");

        JsonErrorProfileCatalogLoader loader = new();

        Response<ErrorProfileCatalogDocument> loadResponse =
            await loader.LoadFromFileAsync(profileCatalogFilePath);

        Assert.True(loadResponse.IsSuccess);
        Assert.NotNull(loadResponse.Data);

        ErrorProfileCatalogDocument normalizedDocument =
            new ErrorProfileCatalogDocumentNormalizer().Normalize(loadResponse.Data);

        ErrorProfileDefinition? profile =
            normalizedDocument.Profiles.FirstOrDefault(candidate =>
                string.Equals(
                    candidate.Name,
                    profileName,
                    StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(profile);
        return profile;
    }

    private static void AssertBackupWasCreated(string whenItFailsJsonsPath)
    {
        string[] backupFiles = Directory.GetFiles(
            whenItFailsJsonsPath,
            "profiles.*.bak.json",
            SearchOption.TopDirectoryOnly);

        Assert.NotEmpty(backupFiles);
    }

    private static void AssertNoBackupWasCreated(string whenItFailsJsonsPath)
    {
        string[] backupFiles = Directory.GetFiles(
            whenItFailsJsonsPath,
            "profiles.*.bak.json",
            SearchOption.TopDirectoryOnly);

        Assert.Empty(backupFiles);
    }
}
