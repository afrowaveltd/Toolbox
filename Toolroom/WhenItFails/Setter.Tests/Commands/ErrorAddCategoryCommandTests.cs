using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class ErrorAddCategoryCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidArguments_AddsCanonicalCategory()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        (ErrorDefinition error, ErrorCategoryDefinition category) =
            await LoadErrorAndUnusedCategoryAsync(workspace.WhenItFailsJsonsPath);

        string categoryLookup = category.Aliases.FirstOrDefault() ?? category.Name.ToLowerInvariant();
        int exitCode = await ErrorAddCategoryCommand.ExecuteAsync(
        [
            "error-add-category",
            workspace.ProjectRootPath,
            error.Name,
            categoryLookup
        ]);

        Assert.Equal(0, exitCode);

        ErrorDefinition savedError = await LoadErrorAsync(
            workspace.WhenItFailsJsonsPath,
            error.Id);
        Assert.Contains(category.Name, savedError.Categories, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownCategory_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);

        int exitCode = await ErrorAddCategoryCommand.ExecuteAsync(
        [
            "error-add-category",
            workspace.ProjectRootPath,
            error.Id,
            "DOES_NOT_EXIST"
        ]);

        Assert.Equal(2, exitCode);
    }

    public static TheoryData<string[]> InvalidArgumentCases =>
        new()
        {
            new[] { "error-add-category" },
            new[] { "error-add-category", "." },
            new[] { "error-add-category", ".", "AFW_NET_0001" },
            new[] { "error-add-category", ".", "AFW_NET_0001", "NETWORK", "EXTRA" }
        };

    [Theory]
    [MemberData(nameof(InvalidArgumentCases))]
    public async Task ExecuteAsync_WithInvalidArguments_ReturnsCommandInputError(string[] args)
    {
        Assert.Equal(1, await ErrorAddCategoryCommand.ExecuteAsync(args));
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

    private static async Task<ErrorDefinition> LoadFirstErrorAsync(string jsonsPath)
    {
        ErrorCatalogDocument document = await LoadErrorsAsync(jsonsPath);
        ErrorDefinition? error = document.Errors.FirstOrDefault();
        Assert.NotNull(error);
        return error;
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
