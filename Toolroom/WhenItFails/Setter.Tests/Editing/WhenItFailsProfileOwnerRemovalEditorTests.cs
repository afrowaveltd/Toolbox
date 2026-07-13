using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Editing;

public sealed class WhenItFailsProfileOwnerRemovalEditorTests
{
    [Fact]
    public async Task ProfileRemoveOwnerAsync_ShouldRemoveCanonicalOwnerAndCreateBackup()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        ErrorOwnerDefinition owner = await PrepareProfileWithOwnerAsync(
            editor,
            temporaryWorkspace);
        DeleteProfileBackups(temporaryWorkspace.WhenItFailsJsonsPath);

        Response<ErrorProfileDefinition> response =
            await editor.ProfileRemoveOwnerAsync(
                temporaryWorkspace.ProjectRootPath,
                "  dita test  ",
                $"  {owner.Name.ToLowerInvariant()}  ");

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.DoesNotContain(
            response.Data.IncludeOwners,
            includedOwner => string.Equals(
                includedOwner,
                owner.Name,
                StringComparison.OrdinalIgnoreCase));

        ErrorProfileDefinition savedProfile = await LoadProfileAsync(
            temporaryWorkspace.WhenItFailsJsonsPath,
            "DITA_TEST");

        Assert.DoesNotContain(
            savedProfile.IncludeOwners,
            includedOwner => string.Equals(
                includedOwner,
                owner.Name,
                StringComparison.OrdinalIgnoreCase));
        AssertBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task ProfileRemoveOwnerAsync_ShouldReturnNotFound_WhenOwnerIsNotIncluded()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        await CreateTestProfileAsync(editor, temporaryWorkspace);
        ErrorOwnerDefinition owner =
            await LoadFirstOwnerAsync(temporaryWorkspace.WhenItFailsJsonsPath);
        DeleteProfileBackups(temporaryWorkspace.WhenItFailsJsonsPath);

        Response<ErrorProfileDefinition> response =
            await editor.ProfileRemoveOwnerAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                owner.Name);

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "ProfileOwnerNotIncluded");
        AssertNoBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task ProfileRemoveOwnerAsync_ShouldReturnNotFound_WhenOwnerDoesNotExist()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        await CreateTestProfileAsync(editor, temporaryWorkspace);
        DeleteProfileBackups(temporaryWorkspace.WhenItFailsJsonsPath);

        Response<ErrorProfileDefinition> response =
            await editor.ProfileRemoveOwnerAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                "DOES_NOT_EXIST");

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "OwnerNotFound");
        AssertNoBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task ProfileRemoveOwnerAsync_ShouldReturnNotFound_WhenProfileDoesNotExist()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        ErrorOwnerDefinition owner =
            await LoadFirstOwnerAsync(temporaryWorkspace.WhenItFailsJsonsPath);
        WhenItFailsProfileWorkspaceEditor editor = new();

        Response<ErrorProfileDefinition> response =
            await editor.ProfileRemoveOwnerAsync(
                temporaryWorkspace.ProjectRootPath,
                "DOES_NOT_EXIST",
                owner.Name);

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "ProfileNotFound");
        AssertNoBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    [Theory]
    [InlineData("   ", "AFW", "ProfileNameIsEmpty")]
    [InlineData("WEB", "   ", "OwnerNameIsEmpty")]
    public async Task ProfileRemoveOwnerAsync_ShouldReturnInvalid_WhenRequiredValueIsEmpty(
        string profileName,
        string ownerName,
        string expectedIssueCode)
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();

        Response<ErrorProfileDefinition> response =
            await editor.ProfileRemoveOwnerAsync(
                temporaryWorkspace.ProjectRootPath,
                profileName,
                ownerName);

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == expectedIssueCode);
        AssertNoBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    private static async Task<ErrorOwnerDefinition> PrepareProfileWithOwnerAsync(
        WhenItFailsProfileWorkspaceEditor editor,
        TemporaryWhenItFailsWorkspace temporaryWorkspace)
    {
        await CreateTestProfileAsync(editor, temporaryWorkspace);
        ErrorOwnerDefinition owner =
            await LoadFirstOwnerAsync(temporaryWorkspace.WhenItFailsJsonsPath);

        Response<ErrorProfileDefinition> response =
            await editor.ProfileAddOwnerAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                owner.Name);

        Assert.True(response.IsSuccess);
        return owner;
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

    private static async Task<ErrorOwnerDefinition> LoadFirstOwnerAsync(
        string whenItFailsJsonsPath)
    {
        string ownerCatalogFilePath = Path.Combine(
            whenItFailsJsonsPath,
            "owners.en.json");

        Response<ErrorOwnerCatalogDocument> loadResponse =
            await new JsonErrorOwnerCatalogLoader().LoadFromFileAsync(ownerCatalogFilePath);

        Assert.True(loadResponse.IsSuccess);
        Assert.NotNull(loadResponse.Data);

        ErrorOwnerCatalogDocument ownerCatalog =
            new ErrorOwnerCatalogDocumentNormalizer().Normalize(loadResponse.Data);

        ErrorOwnerDefinition? owner = ownerCatalog.Owners.FirstOrDefault();
        Assert.NotNull(owner);
        return owner;
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