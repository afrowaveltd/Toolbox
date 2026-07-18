using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsDocumentationKeySuggesterTests
{
    [Fact]
    public async Task SuggestAsync_WithCanonicalCategory_ReturnsCanonicalSuggestion()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorCategoryDefinition category = await LoadFirstCategoryAsync(workspace.WhenItFailsJsonsPath);

        Response<DocumentationKeySuggestion> response =
            await new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                workspace.ProjectRootPath,
                category.Name,
                "Příliš žluťoučký soubor");

        Assert.True(response.IsSuccess);
        DocumentationKeySuggestion suggestion = Assert.IsType<DocumentationKeySuggestion>(response.Data);
        Assert.Equal(category.Name, suggestion.Category);
        Assert.Equal("Příliš žluťoučký soubor", suggestion.Title);
        Assert.EndsWith(
            "/prilis-zlutoucky-soubor",
            suggestion.DocumentationKey,
            StringComparison.Ordinal);
        Assert.True(WhenItFailsDocumentationKeyFormatChecker.IsCanonical(suggestion.DocumentationKey));
    }

    [Fact]
    public async Task SuggestAsync_WithCategoryAlias_ResolvesCanonicalCategory()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorCategoryDefinition category = await LoadCategoryWithAliasAsync(workspace.WhenItFailsJsonsPath);
        string alias = category.Aliases[0];

        Response<DocumentationKeySuggestion> response =
            await new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                workspace.ProjectRootPath,
                alias,
                "Alias sample");

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal(category.Name, response.Data.Category);
    }

    [Fact]
    public async Task SuggestAsync_WhenBaseKeyExists_ReturnsFirstAvailableSuffix()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorCatalogDocument errors = await LoadErrorsAsync(workspace.WhenItFailsJsonsPath);
        ErrorDefinition existing = Assert.Single(
            errors.Errors.Where(error => !string.IsNullOrWhiteSpace(error.DocumentationKey)).Take(1));

        Response<DocumentationKeySuggestion> response =
            await new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                workspace.ProjectRootPath,
                existing.PrimaryCategory,
                existing.Title);

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal($"{existing.DocumentationKey}-2", response.Data.DocumentationKey);
    }

    [Fact]
    public async Task SuggestAsync_WithUnknownCategory_ReturnsNotFound()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        Response<DocumentationKeySuggestion> response =
            await new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                workspace.ProjectRootPath,
                "DOES_NOT_EXIST",
                "Sample title");

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "CategoryNotFound");
    }

    [Fact]
    public async Task SuggestAsync_WithMissingCategoryCatalog_ReturnsStructuredFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        File.Delete(Path.Combine(workspace.WhenItFailsJsonsPath, "categories.en.json"));

        Response<DocumentationKeySuggestion> response =
            await new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                workspace.ProjectRootPath,
                "NETWORK",
                "Sample title");

        Assert.False(response.IsSuccess);
        Assert.Null(response.Data);
        Assert.NotEmpty(response.Issues);
        Assert.False(string.IsNullOrWhiteSpace(response.Message));
    }

    [Fact]
    public async Task SuggestAsync_WithMalformedErrorCatalog_ReturnsStructuredFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorCategoryDefinition category = await LoadFirstCategoryAsync(workspace.WhenItFailsJsonsPath);
        await File.WriteAllTextAsync(
            Path.Combine(workspace.WhenItFailsJsonsPath, "errors.en.json"),
            "{ this is not valid json }");

        Response<DocumentationKeySuggestion> response =
            await new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                workspace.ProjectRootPath,
                category.Name,
                "Sample title");

        Assert.False(response.IsSuccess);
        Assert.Null(response.Data);
        Assert.NotEmpty(response.Issues);
        Assert.False(string.IsNullOrWhiteSpace(response.Message));
    }

    [Fact]
    public async Task SuggestAsync_WithCanceledToken_ThrowsOperationCanceledException()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                workspace.ProjectRootPath,
                "NETWORK",
                "Canceled suggestion",
                cancellation.Token));
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

    private static async Task<ErrorCategoryDefinition> LoadFirstCategoryAsync(string jsonsPath)
    {
        ErrorCategoryCatalogDocument catalog = await LoadCategoriesAsync(jsonsPath);
        ErrorCategoryDefinition? category = catalog.Categories.FirstOrDefault();
        Assert.NotNull(category);
        return category;
    }

    private static async Task<ErrorCategoryDefinition> LoadCategoryWithAliasAsync(string jsonsPath)
    {
        ErrorCategoryCatalogDocument catalog = await LoadCategoriesAsync(jsonsPath);
        ErrorCategoryDefinition? category = catalog.Categories.FirstOrDefault(candidate => candidate.Aliases.Count > 0);
        Assert.NotNull(category);
        return category;
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
