using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Configuration;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsDocumentationKeySuggesterCategoryLookupNormalizationTests
{
    [Fact]
    public async Task SuggestAsync_WhenCanonicalCategoryUsesSpaces_NormalizesLookup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(
            workspace.ProjectRootPath);

        Response<DocumentationKeySuggestion> response =
            await new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                options,
                "  file system  ",
                "Normalized category lookup sample");

        Assert.True(response.IsSuccess);
        Assert.Empty(response.Issues);

        DocumentationKeySuggestion suggestion =
            Assert.IsType<DocumentationKeySuggestion>(response.Data);
        Assert.Equal("FILE_SYSTEM", suggestion.Category);
        Assert.Equal(
            "when-it-fails/errors/file-system/normalized-category-lookup-sample",
            suggestion.DocumentationKey);
    }
}
