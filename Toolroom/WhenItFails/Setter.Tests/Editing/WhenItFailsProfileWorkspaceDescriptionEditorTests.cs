using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Editing;

public sealed class WhenItFailsProfileWorkspaceDescriptionEditorTests
{
    [Fact]
    public async Task SetProfileDescriptionAsync_ShouldSetNormalizedDescriptionAndCreateBackup()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();

        Response<ErrorProfileDefinition> response =
            await editor.SetProfileDescriptionAsync(
                temporaryWorkspace.ProjectRootPath,
                "  web  ",
                "  Web-facing applications and services.  ");

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal(
            "Web-facing applications and services.",
            response.Data.Description);

        ErrorProfileDefinition savedProfile =
            await LoadProfileAsync(
                temporaryWorkspace.WhenItFailsJsonsPath,
                "WEB");

        Assert.Equal(
            "Web-facing applications and services.",
            savedProfile.Description);
        AssertBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task SetProfileDescriptionAsync_ShouldClearDescription_WhenValueIsWhitespace()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();

        Response<ErrorProfileDefinition> initialResponse =
            await editor.SetProfileDescriptionAsync(
                temporaryWorkspace.ProjectRootPath,
                "WEB",
                "Temporary description");

        Assert.True(initialResponse.IsSuccess);

        Response<ErrorProfileDefinition> clearResponse =
            await editor.SetProfileDescriptionAsync(
                temporaryWorkspace.ProjectRootPath,
                "WEB",
                "   ");

        Assert.True(clearResponse.IsSuccess);
        Assert.NotNull(clearResponse.Data);
        Assert.Null(clearResponse.Data.Description);

        ErrorProfileDefinition savedProfile =
            await LoadProfileAsync(
                temporaryWorkspace.WhenItFailsJsonsPath,
                "WEB");

        Assert.Null(savedProfile.Description);
    }

    [Fact]
    public async Task SetProfileDescriptionAsync_ShouldReturnNotFound_WhenProfileDoesNotExist()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();

        Response<ErrorProfileDefinition> response =
            await editor.SetProfileDescriptionAsync(
                temporaryWorkspace.ProjectRootPath,
                "DOES_NOT_EXIST",
                "Description");

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "ProfileNotFound");
        AssertNoBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task SetProfileDescriptionAsync_ShouldReturnInvalid_WhenNameIsEmpty()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();

        Response<ErrorProfileDefinition> response =
            await editor.SetProfileDescriptionAsync(
                temporaryWorkspace.ProjectRootPath,
                "   ",
                "Description");

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "ProfileNameIsEmpty");
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
