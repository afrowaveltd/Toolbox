using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.WhenItFails.Configuration;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsDocumentationKeySuggesterInputGuardTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SuggestAsync_WithMissingCategoryLookup_ThrowsArgumentException(
        string? categoryLookup)
    {
        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                new JsonsOptions(),
                categoryLookup!,
                "Sample title"));

        Assert.Equal("categoryLookup", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SuggestAsync_WithMissingTitle_ThrowsArgumentException(string? title)
    {
        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                new JsonsOptions(),
                "NETWORK",
                title!));

        Assert.Equal("title", exception.ParamName);
    }
}
