using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Editing;

public sealed class WhenItFailsWorkspaceEditorCategoryTests
{
    private const string TestErrorId = "AFW_NET_0001";

    [Fact]
    public async Task ErrorAddCategoryAsync_WithExistingCategory_AddsCanonicalCategoryAndBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition originalError = await LoadErrorAsync(workspace, TestErrorId);
        ErrorCategoryDefinition category = await LoadCategoryNotIncludedAsync(workspace, originalError);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorAddCategoryAsync(
                workspace.ProjectRootPath,
                TestErrorId.ToLowerInvariant(),
                $" {category.Name.ToLowerInvariant()} ");

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Contains(category.Name, response.Data.Categories);

        ErrorDefinition savedError = await LoadErrorAsync(workspace, TestErrorId);
        Assert.Contains(category.Name, savedError.Categories);
        AssertSingleBackup(workspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task ErrorRemoveCategoryAsync_WithNumericLookup_RemovesCategoryAndCreatesBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        WhenItFailsWorkspaceEditor editor = new();
        ErrorDefinition originalError = await LoadErrorAsync(workspace, TestErrorId);
        ErrorCategoryDefinition category = await LoadCategoryNotIncludedAsync(workspace, originalError);

        Assert.True((await editor.ErrorAddCategoryAsync(
            workspace.ProjectRootPath,
            TestErrorId,
            category.Name)).IsSuccess);
        DeleteBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response = await editor.ErrorRemoveCategoryAsync(
            workspace.ProjectRootPath,
            originalError.Code.ToString(),
            category.Name.ToLowerInvariant());

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.DoesNotContain(
            response.Data.Categories,
            value => string.Equals(value, category.Name, StringComparison.OrdinalIgnoreCase));

        ErrorDefinition savedError = await LoadErrorAsync(workspace, TestErrorId);
        Assert.DoesNotContain(
            savedError.Categories,
            value => string.Equals(value, category.Name, StringComparison.OrdinalIgnoreCase));
        AssertSingleBackup(workspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task ErrorAddCategoryAsync_WhenAlreadyIncluded_ReturnsInvalidWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = await LoadErrorAsync(workspace, TestErrorId);
        string categoryName = Assert.Single(error.Categories.Take(1));

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorAddCategoryAsync(
                workspace.ProjectRootPath,
                error.Name,
                categoryName.ToLowerInvariant());

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "ErrorCategoryAlreadyIncluded");
        AssertNoBackup(workspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task ErrorRemoveCategoryAsync_WhenNotIncluded_ReturnsNotFoundWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = await LoadErrorAsync(workspace, TestErrorId);
        ErrorCategoryDefinition category = await LoadCategoryNotIncludedAsync(workspace, error);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorRemoveCategoryAsync(
                workspace.ProjectRootPath,
                TestErrorId,
                category.Name);

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "ErrorCategoryNotIncluded");
        AssertNoBackup(workspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task ErrorAddCategoryAsync_WithUnknownCategory_ReturnsNotFoundWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorAddCategoryAsync(
                workspace.ProjectRootPath,
                TestErrorId,
                "DOES_NOT_EXIST");

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "CategoryNotFound");
        AssertNoBackup(workspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task ErrorAddCategoryAsync_WithUnknownError_ReturnsNotFoundWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorCategoryDefinition category = await LoadFirstCategoryAsync(workspace);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorAddCategoryAsync(
                workspace.ProjectRootPath,
                "AFW_UNKNOWN_9999",
                category.Name);

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "ErrorDefinitionNotFound");
        AssertNoBackup(workspace.WhenItFailsJsonsPath);
    }

    [Theory]
    [InlineData("", "GENERAL", "ErrorLookupIsEmpty")]
    [InlineData("AFW_NET_0001", "", "CategoryNameIsEmpty")]
    public async Task ErrorAddCategoryAsync_WithEmptyValue_ReturnsInvalidWithoutBackup(
        string lookup,
        string categoryName,
        string expectedCode)
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorAddCategoryAsync(
                workspace.ProjectRootPath,
                lookup,
                categoryName);

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == expectedCode);
        AssertNoBackup(workspace.WhenItFailsJsonsPath);
    }

    private static async Task<ErrorDefinition> LoadErrorAsync(
        TemporaryWhenItFailsWorkspace workspace,
        string errorId)
    {
        Response<ErrorCatalogDocument> response =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                Path.Combine(workspace.WhenItFailsJsonsPath, "errors.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        ErrorDefinition? error = new ErrorCatalogDocumentNormalizer()
            .Normalize(response.Data)
            .Errors
            .FirstOrDefault(candidate => string.Equals(
                candidate.Id,
                errorId,
                StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(error);
        return error;
    }

    private static async Task<ErrorCategoryDefinition> LoadCategoryNotIncludedAsync(
        TemporaryWhenItFailsWorkspace workspace,
        ErrorDefinition error)
    {
        ErrorCategoryCatalogDocument catalog = await LoadCategoryCatalogAsync(workspace);
        ErrorCategoryDefinition? category = catalog.Categories.FirstOrDefault(candidate =>
            !error.Categories.Any(existing => string.Equals(
                existing,
                candidate.Name,
                StringComparison.OrdinalIgnoreCase)));

        Assert.NotNull(category);
        return category;
    }

    private static async Task<ErrorCategoryDefinition> LoadFirstCategoryAsync(
        TemporaryWhenItFailsWorkspace workspace)
    {
        ErrorCategoryDefinition? category = (await LoadCategoryCatalogAsync(workspace))
            .Categories
            .FirstOrDefault();
        Assert.NotNull(category);
        return category;
    }

    private static async Task<ErrorCategoryCatalogDocument> LoadCategoryCatalogAsync(
        TemporaryWhenItFailsWorkspace workspace)
    {
        Response<ErrorCategoryCatalogDocument> response =
            await new JsonErrorCategoryCatalogLoader().LoadFromFileAsync(
                Path.Combine(workspace.WhenItFailsJsonsPath, "categories.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorCategoryCatalogDocumentNormalizer().Normalize(response.Data);
    }

    private static void DeleteBackups(string jsonsPath)
    {
        foreach (string filePath in Directory.GetFiles(jsonsPath, "errors.en.*.bak.json"))
        {
            File.Delete(filePath);
        }
    }

    private static void AssertSingleBackup(string jsonsPath)
    {
        Assert.Single(Directory.GetFiles(jsonsPath, "errors.en.*.bak.json"));
    }

    private static void AssertNoBackup(string jsonsPath)
    {
        Assert.Empty(Directory.GetFiles(jsonsPath, "errors.en.*.bak.json"));
    }
}
