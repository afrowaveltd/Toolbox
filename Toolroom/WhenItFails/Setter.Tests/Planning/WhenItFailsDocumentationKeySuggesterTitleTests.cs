using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsDocumentationKeySuggesterTitleTests
{
    [Fact]
    public async Task SuggestAsync_WithPaddedTitle_ReturnsTrimmedTitleAndMatchingKey()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorCategoryDefinition category = await LoadFirstCategoryAsync(
            workspace.WhenItFailsJsonsPath);

        Response<DocumentationKeySuggestion> response =
            await new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                workspace.ProjectRootPath,
                category.Name,
                "  Padded title  ");

        Assert.True(response.IsSuccess);
        DocumentationKeySuggestion suggestion =
            Assert.IsType<DocumentationKeySuggestion>(response.Data);
        Assert.Equal("Padded title", suggestion.Title);
        Assert.EndsWith(
            "/padded-title",
            suggestion.DocumentationKey,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task SuggestAsync_WithRepeatedInternalWhitespace_CollapsesKeySeparators()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorCategoryDefinition category = await LoadFirstCategoryAsync(
            workspace.WhenItFailsJsonsPath);

        Response<DocumentationKeySuggestion> response =
            await new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                workspace.ProjectRootPath,
                category.Name,
                "Multiple   spaced\t title");

        Assert.True(response.IsSuccess);
        DocumentationKeySuggestion suggestion =
            Assert.IsType<DocumentationKeySuggestion>(response.Data);
        Assert.Equal("Multiple   spaced\t title", suggestion.Title);
        Assert.EndsWith(
            "/multiple-spaced-title",
            suggestion.DocumentationKey,
            StringComparison.Ordinal);
        Assert.DoesNotContain("--", suggestion.DocumentationKey, StringComparison.Ordinal);
    }

    private static async Task<ErrorCategoryDefinition> LoadFirstCategoryAsync(
        string jsonsPath)
    {
        Response<ErrorCategoryCatalogDocument> response =
            await new JsonErrorCategoryCatalogLoader().LoadFromFileAsync(
                Path.Combine(jsonsPath, "categories.en.json"));
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        ErrorCategoryCatalogDocument catalog =
            new ErrorCategoryCatalogDocumentNormalizer().Normalize(response.Data);
        ErrorCategoryDefinition? category = catalog.Categories.FirstOrDefault();
        Assert.NotNull(category);
        return category;
    }
}
