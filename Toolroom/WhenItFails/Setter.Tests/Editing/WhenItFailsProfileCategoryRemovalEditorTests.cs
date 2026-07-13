using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Editing;

public sealed class WhenItFailsProfileCategoryRemovalEditorTests
{
    [Fact]
    public async Task ProfileRemoveCategoryAsync_ShouldRemoveCategoryAndCreateBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        await CreateTestProfileAsync(editor, workspace);
        ErrorCategoryDefinition category =
            await LoadFirstCategoryAsync(workspace.WhenItFailsJsonsPath);

        Response<ErrorProfileDefinition> addResponse =
            await editor.ProfileAddCategoryAsync(
                workspace.ProjectRootPath,
                "DITA_TEST",
                category.Name);

        Assert.True(addResponse.IsSuccess);
        DeleteProfileBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorProfileDefinition> response =
            await editor.ProfileRemoveCategoryAsync(
                workspace.ProjectRootPath,
                "  dita test  ",
                $"  {category.Name.ToLowerInvariant()}  ");

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.DoesNotContain(
            response.Data.IncludeCategories,
            includedCategory => string.Equals(
                includedCategory,
                category.Name,
                StringComparison.OrdinalIgnoreCase));

        ErrorProfileDefinition savedProfile =
            await LoadProfileAsync(workspace.WhenItFailsJsonsPath, "DITA_TEST");

        Assert.DoesNotContain(
            savedProfile.IncludeCategories,
            includedCategory => string.Equals(
                includedCategory,
                category.Name,
                StringComparison.OrdinalIgnoreCase));
        AssertBackupWasCreated(workspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task ProfileRemoveCategoryAsync_ShouldReturnNotFound_WhenCategoryIsNotIncluded()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        await CreateTestProfileAsync(editor, workspace);
        DeleteProfileBackups(workspace.WhenItFailsJsonsPath);

        ErrorCategoryDefinition category =
            await LoadFirstCategoryAsync(workspace.WhenItFailsJsonsPath);

        Response<ErrorProfileDefinition> response =
            await editor.ProfileRemoveCategoryAsync(
                workspace.ProjectRootPath,
                "DITA_TEST",
                category.Name);

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "ProfileCategoryNotIncluded");
        AssertNoBackupWasCreated(workspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task ProfileRemoveCategoryAsync_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();
        await CreateTestProfileAsync(editor, workspace);
        DeleteProfileBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorProfileDefinition> response =
            await editor.ProfileRemoveCategoryAsync(
                workspace.ProjectRootPath,
                "DITA_TEST",
                "DOES_NOT_EXIST");

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "CategoryNotFound");
        AssertNoBackupWasCreated(workspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task ProfileRemoveCategoryAsync_ShouldReturnNotFound_WhenProfileDoesNotExist()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        ErrorCategoryDefinition category =
            await LoadFirstCategoryAsync(workspace.WhenItFailsJsonsPath);

        WhenItFailsProfileWorkspaceEditor editor = new();

        Response<ErrorProfileDefinition> response =
            await editor.ProfileRemoveCategoryAsync(
                workspace.ProjectRootPath,
                "DOES_NOT_EXIST",
                category.Name);

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "ProfileNotFound");
        AssertNoBackupWasCreated(workspace.WhenItFailsJsonsPath);
    }

    [Theory]
    [InlineData("   ", "GENERAL", "ProfileNameIsEmpty")]
    [InlineData("WEB", "   ", "CategoryNameIsEmpty")]
    public async Task ProfileRemoveCategoryAsync_ShouldReturnInvalid_WhenRequiredValueIsEmpty(
        string profileName,
        string categoryName,
        string expectedIssueCode)
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        WhenItFailsProfileWorkspaceEditor editor = new();

        Response<ErrorProfileDefinition> response =
            await editor.ProfileRemoveCategoryAsync(
                workspace.ProjectRootPath,
                profileName,
                categoryName);

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == expectedIssueCode);
        AssertNoBackupWasCreated(workspace.WhenItFailsJsonsPath);
    }

    private static async Task CreateTestProfileAsync(
        WhenItFailsProfileWorkspaceEditor editor,
        TemporaryWhenItFailsWorkspace workspace)
    {
        Response<ErrorProfileDefinition> response =
            await editor.AddProfileAsync(
                workspace.ProjectRootPath,
                "DITA_TEST",
                "DiTa Test");

        Assert.True(response.IsSuccess);
    }

    private static async Task<ErrorCategoryDefinition> LoadFirstCategoryAsync(
        string whenItFailsJsonsPath)
    {
        string filePath = Path.Combine(whenItFailsJsonsPath, "categories.en.json");
        Response<ErrorCategoryCatalogDocument> response =
            await new JsonErrorCategoryCatalogLoader().LoadFromFileAsync(filePath);

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        ErrorCategoryCatalogDocument catalog =
            new ErrorCategoryCatalogDocumentNormalizer().Normalize(response.Data);

        ErrorCategoryDefinition? category = catalog.Categories.FirstOrDefault();
        Assert.NotNull(category);
        return category;
    }

    private static async Task<ErrorProfileDefinition> LoadProfileAsync(
        string whenItFailsJsonsPath,
        string profileName)
    {
        string filePath = Path.Combine(whenItFailsJsonsPath, "profiles.json");
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

    private static void DeleteProfileBackups(string path)
    {
        foreach (string filePath in Directory.GetFiles(
                     path,
                     "profiles.*.bak.json",
                     SearchOption.TopDirectoryOnly))
        {
            File.Delete(filePath);
        }
    }

    private static void AssertBackupWasCreated(string path) =>
        Assert.NotEmpty(Directory.GetFiles(
            path,
            "profiles.*.bak.json",
            SearchOption.TopDirectoryOnly));

    private static void AssertNoBackupWasCreated(string path) =>
        Assert.Empty(Directory.GetFiles(
            path,
            "profiles.*.bak.json",
            SearchOption.TopDirectoryOnly));
}
