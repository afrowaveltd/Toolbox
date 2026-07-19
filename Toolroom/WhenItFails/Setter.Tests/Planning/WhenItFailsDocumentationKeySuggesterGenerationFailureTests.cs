using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsDocumentationKeySuggesterGenerationFailureTests
{
    [Fact]
    public async Task SuggestAsync_WhenTitleCannotProduceSlug_ReturnsStructuredFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        ErrorCategoryDefinition category = await LoadFirstCategoryAsync(
            workspace.WhenItFailsJsonsPath);

        Response<DocumentationKeySuggestion> response =
            await new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                workspace.ProjectRootPath,
                category.Name,
                "!!!");

        Assert.False(response.IsSuccess);
        Assert.Null(response.Data);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "DocumentationKeyGenerationFailed");
        Assert.False(string.IsNullOrWhiteSpace(response.Message));
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
