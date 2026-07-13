using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Editing;

public sealed class WhenItFailsProfileCategoryEditorTests
{
    [Fact]
    public async Task ProfileAddCategoryAsync_ShouldAddCanonicalCategoryAndCreateBackup()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        await CreateTestProfileAsync(editor, temporaryWorkspace);
        DeleteProfileBackups(temporaryWorkspace.WhenItFailsJsonsPath);

        ErrorCategoryDefinition category =
            await LoadFirstCategoryAsync(temporaryWorkspace.WhenItFailsJsonsPath);

        Response<ErrorProfileDefinition> response =
            await editor.ProfileAddCategoryAsync(
                temporaryWorkspace.ProjectRootPath,
                "  dita test  ",
                $"  {category.Name.ToLowerInvariant()}  ");

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Contains(
            response.Data.IncludeCategories,
            includedCategory => string.Equals(
                includedCategory,
                category.Name,
                StringComparison.Ordinal));

        ErrorProfileDefinition savedProfile =
            await LoadProfileAsync(
                temporaryWorkspace.WhenItFailsJsonsPath,
                "DITA_TEST");

        Assert.Contains(
            savedProfile.IncludeCategories,
            includedCategory => string.Equals(
                includedCategory,
                category.Name,
                StringComparison.Ordinal));
        AssertBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task ProfileAddCategoryAsync_ShouldReturnInvalid_WhenCategoryAlreadyIncluded()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        await CreateTestProfileAsync(editor, temporaryWorkspace);

        ErrorCategoryDefinition category =
            await LoadFirstCategoryAsync(temporaryWorkspace.WhenItFailsJsonsPath);

        Response<ErrorProfileDefinition> firstResponse =
            await editor.ProfileAddCategoryAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                category.Name);

        Assert.True(firstResponse.IsSuccess);
        DeleteProfileBackups(temporaryWorkspace.WhenItFailsJsonsPath);

        Response<ErrorProfileDefinition> duplicateResponse =
            await editor.ProfileAddCategoryAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                category.Name.ToLowerInvariant());

        Assert.False(duplicateResponse.IsSuccess);
        Assert.Contains(
            duplicateResponse.Issues,
            issue => issue.Code == "ProfileCategoryAlreadyIncluded");
        AssertNoBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task ProfileAddCategoryAsync_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        await CreateTestProfileAsync(editor, temporaryWorkspace);
        DeleteProfileBackups(temporaryWorkspace.WhenItFailsJsonsPath);

        Response<ErrorProfileDefinition> response =
            await editor.ProfileAddCategoryAsync(
                temporaryWorkspace.ProjectRootPath,
                "DITA_TEST",
                "DOES_NOT_EXIST");

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "CategoryNotFound");
        AssertNoBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task ProfileAddCategoryAsync_ShouldReturnNotFound_WhenProfileDoesNotExist()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        ErrorCategoryDefinition category =
            await LoadFirstCategoryAsync(temporaryWorkspace.WhenItFailsJsonsPath);

        WhenItFailsProfileWorkspaceEditor editor = new();

        Response<ErrorProfileDefinition> response =
            await editor.ProfileAddCategoryAsync(
                temporaryWorkspace.ProjectRootPath,
                "DOES_NOT_EXIST",
                category.Name);

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "ProfileNotFound");
        AssertNoBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    [Theory]
    [InlineData("   ", "GENERAL", "ProfileNameIsEmpty")]
    [InlineData("WEB", "   ", "CategoryNameIsEmpty")]
    public async Task ProfileAddCategoryAsync_ShouldReturnInvalid_WhenRequiredValueIsEmpty(
        string profileName,
        string categoryName,
        string expectedIssueCode)
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();

        Response<ErrorProfileDefinition> response =
            await editor.ProfileAddCategoryAsync(
                temporaryWorkspace.ProjectRootPath,
                profileName,
                categoryName);

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

    private static async Task<ErrorCategoryDefinition> LoadFirstCategoryAsync(
        string whenItFailsJsonsPath)
    {
        string categoryCatalogFilePath = Path.Combine(
            whenItFailsJsonsPath,
            "categories.en.json");

        Response<ErrorCategoryCatalogDocument> loadResponse =
            await new JsonErrorCategoryCatalogLoader().LoadFromFileAsync(categoryCatalogFilePath);

        Assert.True(loadResponse.IsSuccess);
        Assert.NotNull(loadResponse.Data);

        ErrorCategoryCatalogDocument categoryCatalog =
            new ErrorCategoryCatalogDocumentNormalizer().Normalize(loadResponse.Data);

        ErrorCategoryDefinition? category = categoryCatalog.Categories.FirstOrDefault();
        Assert.NotNull(category);
        return category;
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
