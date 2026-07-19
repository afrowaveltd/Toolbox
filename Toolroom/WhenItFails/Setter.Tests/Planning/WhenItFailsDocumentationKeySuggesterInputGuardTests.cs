using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.WhenItFails.Configuration;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsDocumentationKeySuggesterInputGuardTests
{
    [Fact]
    public async Task SuggestAsync_WithNullCategoryLookup_ThrowsArgumentNullException()
    {
        ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                new JsonsOptions(),
                null!,
                "Sample title"));

        Assert.Equal("categoryLookup", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SuggestAsync_WithEmptyCategoryLookup_ThrowsArgumentException(
        string categoryLookup)
    {
        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                new JsonsOptions(),
                categoryLookup,
                "Sample title"));

        Assert.Equal("categoryLookup", exception.ParamName);
    }

    [Fact]
    public async Task SuggestAsync_WithNullTitle_ThrowsArgumentNullException()
    {
        ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                new JsonsOptions(),
                "NETWORK",
                null!));

        Assert.Equal("title", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SuggestAsync_WithEmptyTitle_ThrowsArgumentException(string title)
    {
        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                new JsonsOptions(),
                "NETWORK",
                title));

        Assert.Equal("title", exception.ParamName);
    }
}
