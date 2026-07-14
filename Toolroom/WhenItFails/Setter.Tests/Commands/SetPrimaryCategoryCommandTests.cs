using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class SetPrimaryCategoryCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidArguments_ChangesPrimaryCategory()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        (ErrorDefinition error, ErrorCategoryDefinition category) =
            await LoadErrorAndDifferentCategoryAsync(workspace.WhenItFailsJsonsPath);
        string categoryLookup = category.Aliases.FirstOrDefault() ?? category.Name.ToLowerInvariant();

        int exitCode = await SetPrimaryCategoryCommand.ExecuteAsync(
        [
            "set-primary-category",
            workspace.ProjectRootPath,
            error.Name,
            categoryLookup
        ]);

        Assert.Equal(0, exitCode);

        ErrorDefinition savedError = await LoadErrorAsync(
            workspace.WhenItFailsJsonsPath,
            error.Id);
        Assert.Equal(category.Name, savedError.PrimaryCategory);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownCategory_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);

        int exitCode = await SetPrimaryCategoryCommand.ExecuteAsync(
        [
            "set-primary-category",
            workspace.ProjectRootPath,
            error.Id,
            "DOES_NOT_EXIST"
        ]);

        Assert.Equal(2, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithAlreadySetCategory_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorDefinition error = await LoadFirstErrorAsync(workspace.WhenItFailsJsonsPath);

        int exitCode = await SetPrimaryCategoryCommand.ExecuteAsync(
        [
            "set-primary-category",
            workspace.ProjectRootPath,
            error.Code.ToString(),
            error.PrimaryCategory
        ]);

        Assert.Equal(2, exitCode);
    }

    public static TheoryData<string[]> InvalidArgumentCases =>
        new()
        {
            new[] { "set-primary-category" },
            new[] { "set-primary-category", "." },
            new[] { "set-primary-category", ".", "AFW_NET_0001" },
            new[] { "set-primary-category", ".", "AFW_NET_0001", "NETWORK", "EXTRA" }
        };

    [Theory]
    [MemberData(nameof(InvalidArgumentCases))]
    public async Task ExecuteAsync_WithInvalidArguments_ReturnsCommandInputError(string[] args)
    {
        Assert.Equal(1, await SetPrimaryCategoryCommand.ExecuteAsync(args));
    }

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

        throw new InvalidOperationException(
            "The test workspace contains no alternative primary category for any error.");
    }

    private static async Task<ErrorDefinition> LoadFirstErrorAsync(string jsonsPath)
    {
        ErrorCatalogDocument document = await LoadErrorsAsync(jsonsPath);
        ErrorDefinition? error = document.Errors.FirstOrDefault();
        Assert.NotNull(error);
        return error;
    }

    private static async Task<ErrorDefinition> LoadErrorAsync(
        string jsonsPath,
        string errorId)
    {
        ErrorCatalogDocument document = await LoadErrorsAsync(jsonsPath);
        ErrorDefinition? error = document.Errors.FirstOrDefault(candidate =>
            string.Equals(
                candidate.Id,
                errorId,
                StringComparison.OrdinalIgnoreCase));
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
