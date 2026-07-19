using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Configuration;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsDocumentationKeySuggesterUnknownCategoryMalformedErrorCatalogTests
{
    [Fact]
    public async Task SuggestAsync_WithUnknownCategoryAndMalformedErrorCatalog_ReturnsCategoryNotFound()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(
            workspace.ProjectRootPath);
        string errorCatalogPath = Path.Combine(
            options.RootDirectory,
            options.PackageDirectoryName,
            options.ErrorCatalogFileName);

        await File.WriteAllTextAsync(
            errorCatalogPath,
            "{ this is not valid json }");

        Response<DocumentationKeySuggestion> response =
            await new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                options,
                "CATEGORY_THAT_DOES_NOT_EXIST",
                "Unknown category sample");

        Assert.False(response.IsSuccess);
        Assert.Null(response.Data);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "CategoryNotFound");
        Assert.False(string.IsNullOrWhiteSpace(response.Message));
    }
}
