using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.ValueObjects;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class ProfileNameExtensionsTests
{
    [Theory]
    [InlineData("default", "default", true)]
    [InlineData("default", "DEFAULT", true)]
    [InlineData("DefaultProfile", "defaultprofile", true)]
    [InlineData("markdown-refine", "Markdown-Refine", true)]
    [InlineData("default", "production", false)]
    [InlineData("live-chat-fast", "markdown-refine", false)]
    public void EqualsIgnoreCase_ReturnsExpectedResult(
        string first,
        string second,
        bool expected)
    {
        var firstProfileName = new ProfileName(first);
        var secondProfileName = new ProfileName(second);

        var actual = firstProfileName.EqualsIgnoreCase(secondProfileName);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("DEFAULT", "default")]
    [InlineData("DefaultProfile", "defaultprofile")]
    [InlineData("Markdown-Refine", "markdown-refine")]
    [InlineData("LIVE-CHAT-FAST", "live-chat-fast")]
    public void ToLowerInvariantName_ReturnsLowercaseProfileName(
        string input,
        string expected)
    {
        var profileName = new ProfileName(input);

        var actual = profileName.ToLowerInvariantName();

        Assert.Equal(expected, actual.Value);
    }

    [Fact]
    public void ToLowerInvariantName_ReturnsNewProfileNameWithSameValueWhenAlreadyLowercase()
    {
        var profileName = new ProfileName("default");

        var actual = profileName.ToLowerInvariantName();

        Assert.Equal("default", actual.Value);
        Assert.Equal(profileName, actual);
    }
}