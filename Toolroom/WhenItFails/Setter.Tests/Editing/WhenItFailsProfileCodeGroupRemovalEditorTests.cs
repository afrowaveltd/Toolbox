using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Editing;

public sealed class WhenItFailsProfileCodeGroupRemovalEditorTests
{
    [Fact]
    public async Task ProfileRemoveCodeGroupAsync_ShouldRemoveIncludedCodeGroupAndCreateBackup()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        await CreateTestProfileAsync(editor, temporaryWorkspace);

        ErrorCodeGroupDefinition codeGroup =
            await LoadFirstCodeGroupAsync(temporaryWorkspace.WhenItFailsJsonsPath);

        Response<ErrorProfileDefinition> addResponse =
            await editor.ProfileAddCodeGroupAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                codeGroup.Name);

        Assert.True(addResponse.IsSuccess);
        DeleteProfileBackups(temporaryWorkspace.WhenItFailsJsonsPath);

        Response<ErrorProfileDefinition> response =
            await editor.ProfileRemoveCodeGroupAsync(
                temporaryWorkspace.ProjectRootPath,
                "  dita test  ",
                $"  {codeGroup.CodePrefix.ToLowerInvariant()}  ");

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.DoesNotContain(
            response.Data.IncludeCodeGroups,
            includedCodeGroup => string.Equals(
                includedCodeGroup,
                codeGroup.Name,
                StringComparison.OrdinalIgnoreCase));

        ErrorProfileDefinition savedProfile = await LoadProfileAsync(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "DITA_TEST");

        Assert.DoesNotContain(
            savedProfile.IncludeCodeGroups,
            includedCodeGroup => string.Equals(
                includedCodeGroup,
                codeGroup.Name,
                StringComparison.OrdinalIgnoreCase));
        AssertBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task ProfileRemoveCodeGroupAsync_ShouldReturnNotFound_WhenCodeGroupIsNotIncluded()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        await CreateTestProfileAsync(editor, temporaryWorkspace);
        DeleteProfileBackups(temporaryWorkspace.WhenItFailsJsonsPath);

        ErrorCodeGroupDefinition codeGroup =
            await LoadFirstCodeGroupAsync(temporaryWorkspace.WhenItFailsJsonsPath);

        Response<ErrorProfileDefinition> response =
            await editor.ProfileRemoveCodeGroupAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                codeGroup.Name);

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "ProfileCodeGroupNotIncluded");
        AssertNoBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task ProfileRemoveCodeGroupAsync_ShouldReturnNotFound_WhenCodeGroupDoesNotExist()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        await CreateTestProfileAsync(editor, temporaryWorkspace);
        DeleteProfileBackups(temporaryWorkspace.WhenItFailsJsonsPath);

        Response<ErrorProfileDefinition> response =
            await editor.ProfileRemoveCodeGroupAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                "DOES_NOT_EXIST");

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "CodeGroupNotFound");
        AssertNoBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task ProfileRemoveCodeGroupAsync_ShouldReturnNotFound_WhenProfileDoesNotExist()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        ErrorCodeGroupDefinition codeGroup =
            await LoadFirstCodeGroupAsync(temporaryWorkspace.WhenItFailsJsonsPath);

        WhenItFailsProfileWorkspaceEditor editor = new();

        Response<ErrorProfileDefinition> response =
            await editor.ProfileRemoveCodeGroupAsync(
                temporaryWorkspace.ProjectRootPath,
                "DOES_NOT_EXIST",
                codeGroup.Name);

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "ProfileNotFound");
        AssertNoBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    [Theory]
    [InlineData("   ", "NETWORK", "ProfileNameIsEmpty")]
    [InlineData("WEB", "   ", "CodeGroupNameIsEmpty")]
    public async Task ProfileRemoveCodeGroupAsync_ShouldReturnInvalid_WhenRequiredValueIsEmpty(
        string profileName,
        string codeGroupName,
        string expectedIssueCode)
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();

        Response<ErrorProfileDefinition> response =
            await editor.ProfileRemoveCodeGroupAsync(
                temporaryWorkspace.ProjectRootPath,
                profileName,
                codeGroupName);

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == expectedIssueCode);
        AssertNoBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    private static async Task CreateTestProfileAsync(
        WhenItFailsProfileWorkspaceEditor editor,
        TemporaryWhenItFailsWorkspace temporaryWorkspace)
    {
        Response<ErrorProfileDefinition> response =
            await editor.AddProfileAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                "DiTa Test");

        Assert.True(response.IsSuccess);
    }

    private static async Task<ErrorCodeGroupDefinition> LoadFirstCodeGroupAsync(
        string whenItFailsJsonsPath)
    {
        string codeGroupCatalogFilePath = Path.Combine(
            whenItFailsJsonsPath,
            "code-groups.en.json");

        Response<ErrorCodeGroupCatalogDocument> loadResponse =
            await new JsonErrorCodeGroupCatalogLoader().LoadFromFileAsync(codeGroupCatalogFilePath);

        Assert.True(loadResponse.IsSuccess);
        Assert.NotNull(loadResponse.Data);

        ErrorCodeGroupCatalogDocument codeGroupCatalog =
            new ErrorCodeGroupCatalogDocumentNormalizer().Normalize(loadResponse.Data);

        ErrorCodeGroupDefinition? codeGroup = codeGroupCatalog.CodeGroups.FirstOrDefault();
        Assert.NotNull(codeGroup);
        return codeGroup;
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

    private static void DeleteProfileBackups(string whenItFailsJsonsPath)
    {
        foreach (string backupFilePath in Directory.GetFiles(
                     whenItFailsJsonsPath,
                     "profiles.*.bak.json",
                     SearchOption.TopDirectoryOnly))
        {
            File.Delete(backupFilePath);
        }
    }

    private static void AssertBackupWasCreated(string whenItFailsJsonsPath)
    {
        Assert.NotEmpty(Directory.GetFiles(
            whenItFailsJsonsPath,
            "profiles.*.bak.json",
            SearchOption.TopDirectoryOnly));
    }

    private static void AssertNoBackupWasCreated(string whenItFailsJsonsPath)
    {
        Assert.Empty(Directory.GetFiles(
            whenItFailsJsonsPath,
            "profiles.*.bak.json",
            SearchOption.TopDirectoryOnly));
    }
}
