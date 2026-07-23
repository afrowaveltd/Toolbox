using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Documentation;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsDocumentationKeySuggesterOptionsEmptyErrorCatalogTests
{
    [Fact]
    public async Task SuggestAsync_WithResolvedOptionsAndEmptyErrorCatalog_ReturnsBaseKey()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(
            workspace.ProjectRootPath);
        ErrorCategoryDefinition category = await LoadFirstCategoryAsync(
            workspace.WhenItFailsJsonsPath);
        string errorCatalogPath = Path.Combine(
            options.RootDirectory,
            options.PackageDirectoryName,
            options.ErrorCatalogFileName);

        await File.WriteAllTextAsync(
            errorCatalogPath,
            """
            {
              "schemaVersion": "1.0",
              "catalogId": "empty.errors",
              "catalogName": "Empty errors",
              "language": "en",
              "errors": []
            }
            """);

        Response<DocumentationKeySuggestion> response =
            await new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                options,
                category.Name,
                "Empty catalog sample");

        Assert.True(response.IsSuccess);
        Assert.Empty(response.Issues);
        DocumentationKeySuggestion suggestion =
            Assert.IsType<DocumentationKeySuggestion>(response.Data);
        Assert.Equal(category.Name, suggestion.Category);
        Assert.Equal("Empty catalog sample", suggestion.Title);
        Assert.EndsWith(
            "/empty-catalog-sample",
            suggestion.DocumentationKey,
            StringComparison.Ordinal);
        Assert.False(
            suggestion.DocumentationKey.EndsWith(
                "-2",
                StringComparison.Ordinal));
        Assert.True(
            DocumentationKeyFormat.IsCanonical(
                suggestion.DocumentationKey));
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
