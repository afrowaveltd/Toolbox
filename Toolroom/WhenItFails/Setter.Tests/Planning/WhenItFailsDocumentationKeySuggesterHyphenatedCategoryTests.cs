using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsDocumentationKeySuggesterHyphenatedCategoryTests
{
    [Fact]
    public async Task SuggestAsync_WithHyphenatedCategoryLookup_ReturnsCanonicalCategory()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        Response<DocumentationKeySuggestion> response =
            await new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                workspace.ProjectRootPath,
                "file-system",
                "Hyphenated category lookup sample");

        Assert.True(response.IsSuccess);
        Assert.Empty(response.Issues);
        DocumentationKeySuggestion suggestion =
            Assert.IsType<DocumentationKeySuggestion>(response.Data);
        Assert.Equal("FILE_SYSTEM", suggestion.Category);
        Assert.Equal(
            "when-it-fails/errors/file-system/hyphenated-category-lookup-sample",
            suggestion.DocumentationKey);
    }
}
