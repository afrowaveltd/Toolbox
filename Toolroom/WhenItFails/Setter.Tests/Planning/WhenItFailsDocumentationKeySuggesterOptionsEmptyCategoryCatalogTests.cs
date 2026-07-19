using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Configuration;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsDocumentationKeySuggesterOptionsEmptyCategoryCatalogTests
{
    [Fact]
    public async Task SuggestAsync_WithEmptyCategoryCatalog_ReturnsCategoryNotFound()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(
            workspace.ProjectRootPath);
        string categoryCatalogPath = Path.Combine(
            options.RootDirectory,
            options.PackageDirectoryName,
            options.CategoryCatalogFileName);

        await File.WriteAllTextAsync(
            categoryCatalogPath,
            """
            {
              "schemaVersion": "1.0",
              "catalogId": "empty.categories",
              "catalogName": "Empty categories",
              "language": "en",
              "categories": []
            }
            """);

        Response<DocumentationKeySuggestion> response =
            await new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                options,
                "NETWORK",
                "Empty category catalog sample");

        Assert.False(response.IsSuccess);
        Assert.Null(response.Data);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "CategoryNotFound");
        Assert.False(string.IsNullOrWhiteSpace(response.Message));
    }
}
