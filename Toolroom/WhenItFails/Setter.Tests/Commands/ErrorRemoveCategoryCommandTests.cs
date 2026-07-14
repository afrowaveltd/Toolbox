using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class ErrorRemoveCategoryCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidArguments_RemovesCategory()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        (ErrorDefinition error, ErrorCategoryDefinition category) =
            await LoadErrorAndUnusedCategoryAsync(workspace.WhenItFailsJsonsPath);

        Response<ErrorDefinition> addResponse =
            await new WhenItFailsWorkspaceEditor().ErrorAddCategoryAsync(
                workspace.ProjectRootPath,
                error.Id,
                category.Name);
        Assert.True(addResponse.IsSuccess);

        string categoryLookup = category.Aliases.FirstOrDefault() ?? category.Name.ToLowerInvariant();
        int exitCode = await ErrorRemoveCategoryCommand.ExecuteAsync(
        [
            "error-remove-category",
            workspace.ProjectRootPath,
            error.Code.ToString(),
            categoryLookup
        ]);

        Assert.Equal(0, exitCode);

        ErrorDefinition savedError = await LoadErrorAsync(
            workspace.WhenItFailsJsonsPath,
            error.Id);
        Assert.DoesNotContain(category.Name, savedError.Categories, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WithCategoryNotIncluded_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        (ErrorDefinition error, ErrorCategoryDefinition category) =
            await LoadErrorAndUnusedCategoryAsync(workspace.WhenItFailsJsonsPath);

        int exitCode = await ErrorRemoveCategoryCommand.ExecuteAsync(
        [
            "error-remove-category",
            workspace.ProjectRootPath,
            error.Id,
            category.Name
        ]);

        Assert.Equal(2, exitCode);
    }

    public static TheoryData<string[]> InvalidArgumentCases =>
        new()
        {
            new[] { "error-remove-category" },
            new[] { "error-remove-category", "." },
            new[] { "error-remove-category", ".", "AFW_NET_0001" },
            new[] { "error-remove-category", ".", "AFW_NET_0001", "NETWORK", "EXTRA" }
        };

    [Theory]
    [MemberData(nameof(InvalidArgumentCases))]
    public async Task ExecuteAsync_WithInvalidArguments_ReturnsCommandInputError(string[] args)
    {
        Assert.Equal(1, await ErrorRemoveCategoryCommand.ExecuteAsync(args));
    }

    private static async Task<(ErrorDefinition Error, ErrorCategoryDefinition Category)>
        LoadErrorAndUnusedCategoryAsync(string jsonsPath)
    {
        ErrorCatalogDocument errors = await LoadErrorsAsync(jsonsPath);
        ErrorCategoryCatalogDocument categories = await LoadCategoriesAsync(jsonsPath);

        foreach (ErrorDefinition error in errors.Errors)
        {
            ErrorCategoryDefinition? category = categories.Categories.FirstOrDefault(candidate =>
                !error.Categories.Contains(candidate.Name, StringComparer.OrdinalIgnoreCase));
            if (category is not null)
            {
                return (error, category);
            }
        }

        throw new InvalidOperationException("The test workspace contains no unused category for any error.");
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
