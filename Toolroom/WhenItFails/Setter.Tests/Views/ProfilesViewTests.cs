using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Views;

public sealed class ProfilesViewTests
{
    [Fact]
    public void CreateSelectionText_WithEmptySelection_ReturnsAll()
    {
        string result = ProfilesView.CreateSelectionText([]);

        Assert.Equal("All", result);
    }

    [Fact]
    public void CreateSelectionText_WithValues_ReturnsCommaSeparatedText()
    {
        string result = ProfilesView.CreateSelectionText(
            ["NETWORK", "FILE_SYSTEM"]);

        Assert.Equal("NETWORK, FILE_SYSTEM", result);
    }
}
