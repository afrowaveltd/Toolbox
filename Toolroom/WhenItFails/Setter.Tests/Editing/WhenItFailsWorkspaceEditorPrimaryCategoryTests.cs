using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Editing;

public sealed class WhenItFailsWorkspaceEditorPrimaryCategoryTests
{
    [Fact]
    public async Task SetPrimaryCategoryAsync_ShouldPersistCanonicalCategoryAndCreateBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        (ErrorDefinition error, ErrorCategoryDefinition category) =
            await LoadErrorAndDifferentCategoryAsync(workspace.WhenItFailsJsonsPath);

        string lookup = category.Aliases.FirstOrDefault() ?? category.Name.ToLowerInvariant();
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().SetPrimaryCategoryAsync(
                workspace.ProjectRootPath,
                error.Code.ToString(),
                lookup);

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal(category.Name, response.Data.PrimaryCategory);

        ErrorDefinition saved = await LoadErrorAsync(
            workspace.WhenItFailsJsonsPath,
            error.Id);

        Assert.Equal(category.Name, saved.PrimaryCategory);
        Assert.Equal(backupsBefore + 1, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task SetPrimaryCategoryAsync_ShouldRejectSameCategoryWithoutBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().SetPrimaryCategoryAsync(
                workspace.ProjectRootPath,
                error.Name,
                error.PrimaryCategory.ToLowerInvariant());

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "PrimaryCategoryAlreadySet");
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task SetPrimaryCategoryAsync_ShouldReturnNotFoundForUnknownCategory()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().SetPrimaryCategoryAsync(
                workspace.ProjectRootPath,
                error.Id,
                "DOES_NOT_EXIST");

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "CategoryNotFound");
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Fact]
    public async Task SetPrimaryCategoryAsync_ShouldReturnNotFoundForUnknownError()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        ErrorCategoryDefinition category =
            await LoadFirstCategoryAsync(workspace.WhenItFailsJsonsPath);
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().SetPrimaryCategoryAsync(
                workspace.ProjectRootPath,
                "DOES_NOT_EXIST",
                category.Name);

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "ErrorDefinitionNotFound");
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    [Theory]
    [InlineData("", "NETWORK", "ErrorLookupIsEmpty")]
    [InlineData("AFW_NET_0001", "", "PrimaryCategoryNameIsEmpty")]
    public async Task SetPrimaryCategoryAsync_ShouldRejectEmptyValues(
        string lookupValue,
        string categoryName,
        string expectedCode)
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        int backupsBefore = CountBackups(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().SetPrimaryCategoryAsync(
                workspace.ProjectRootPath,
                lookupValue,
                categoryName);

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == expectedCode);
        Assert.Equal(backupsBefore, CountBackups(workspace.WhenItFailsJsonsPath));
    }

    private static int CountBackups(string jsonsPath) => Directory.GetFiles(
        jsonsPath,
        "errors.en.*.bak.json",
        SearchOption.TopDirectoryOnly).Length;

    private static async Task<(ErrorDefinition Error, ErrorCategoryDefinition Category)>
        LoadErrorAndDifferentCategoryAsync(string jsonsPath)
    {
        ErrorCatalogDocument errors = await LoadErrorsAsync(jsonsPath);
        ErrorCategoryCatalogDocument categories = await LoadCategoriesAsync(jsonsPath);

        foreach (ErrorDefinition error in errors.Errors)
        {
            ErrorCategoryDefinition? category = categories.Categories.FirstOrDefault(candidate =>
                !string.Equals(
                    candidate.Name,
                    error.PrimaryCategory,
                    StringComparison.OrdinalIgnoreCase));

            if (category is not null)
            {
                return (error, category);
            }
        }

        throw new InvalidOperationException("The test workspace contains no alternate primary category.");
    }

    private static async Task<ErrorDefinition> LoadFirstErrorAsync(string jsonsPath)
    {
        ErrorCatalogDocument document = await LoadErrorsAsync(jsonsPath);
        ErrorDefinition? error = document.Errors.FirstOrDefault(candidate =>
            !string.IsNullOrWhiteSpace(candidate.PrimaryCategory));
        Assert.NotNull(error);
        return error;
    }

    private static async Task<ErrorCategoryDefinition> LoadFirstCategoryAsync(string jsonsPath)
    {
        ErrorCategoryCatalogDocument document = await LoadCategoriesAsync(jsonsPath);
        ErrorCategoryDefinition? category = document.Categories.FirstOrDefault();
        Assert.NotNull(category);
        return category;
    }

    private static async Task<ErrorDefinition> LoadErrorAsync(string jsonsPath, string errorId)
    {
        ErrorCatalogDocument document = await LoadErrorsAsync(jsonsPath);
        ErrorDefinition? error = document.Errors.FirstOrDefault(candidate =>
            string.Equals(candidate.Id, errorId, StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(error);
        return error;
    }

    private static async Task<ErrorCatalogDocument> LoadErrorsAsync(string jsonsPath)
    {
        Response<ErrorCatalogDocument> response =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "errors.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorCatalogDocumentNormalizer().Normalize(response.Data);
    }

    private static async Task<ErrorCategoryCatalogDocument> LoadCategoriesAsync(string jsonsPath)
    {
        Response<ErrorCategoryCatalogDocument> response =
            await new JsonErrorCategoryCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "categories.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        return new ErrorCategoryCatalogDocumentNormalizer().Normalize(response.Data);
    }
}
