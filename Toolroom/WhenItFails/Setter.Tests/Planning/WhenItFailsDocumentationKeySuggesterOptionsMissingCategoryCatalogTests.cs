using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Configuration;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsDocumentationKeySuggesterOptionsMissingCategoryCatalogTests
{
    [Fact]
    public async Task SuggestAsync_WithResolvedOptionsAndMissingCategoryCatalog_ReturnsStructuredFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(
            workspace.ProjectRootPath);
        File.Delete(options.CategoryCatalogPath);

        Response<DocumentationKeySuggestion> response =
            await new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                options,
                "NETWORK",
                "Missing category catalog");

        Assert.False(response.IsSuccess);
        Assert.Null(response.Data);
        Assert.NotEmpty(response.Issues);
        Assert.False(string.IsNullOrWhiteSpace(response.Message));
    }
}
