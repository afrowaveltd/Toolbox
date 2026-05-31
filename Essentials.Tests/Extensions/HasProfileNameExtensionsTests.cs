using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Interfaces;
using Afrowave.Toolbox.Essentials.ValueObjects;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class HasProfileNameExtensionsTests
{
    [Fact]
    public void HasProfileName_WhenSourceIsNull_ThrowsArgumentNullException()
    {
        IHasProfileName? source = null;

        Assert.Throws<ArgumentNullException>(() =>
            source!.HasProfileName());
    }

    [Fact]
    public void HasProfileName_ReturnsTrue()
    {
        var source = new TestProfileNameSource
        {
            ProfileName = new ProfileName("default")
        };

        var actual = source.HasProfileName();

        Assert.True(actual);
    }

    [Fact]
    public void HasProfileName_WithExpectedProfileName_WhenSourceIsNull_ThrowsArgumentNullException()
    {
        IHasProfileName? source = null;

        var expectedProfileName = new ProfileName("default");

        Assert.Throws<ArgumentNullException>(() =>
            source!.HasProfileName(expectedProfileName));
    }


    [Fact]
    public void HasProfileName_WithExpectedProfileName_WhenProfileNameMatches_ReturnsTrue()
    {
        var source = new TestProfileNameSource
        {
            ProfileName = new ProfileName("default")
        };

        var expectedProfileName = new ProfileName("default");

        var actual = source.HasProfileName(expectedProfileName);

        Assert.True(actual);
    }

    [Fact]
    public void HasProfileName_WithExpectedProfileName_UsesCaseInsensitiveComparison()
    {
        var source = new TestProfileNameSource
        {
            ProfileName = new ProfileName("DefaultProfile")
        };

        var expectedProfileName = new ProfileName("defaultprofile");

        var actual = source.HasProfileName(expectedProfileName);

        Assert.True(actual);
    }

    [Fact]
    public void HasProfileName_WithExpectedProfileName_WhenProfileNameDoesNotMatch_ReturnsFalse()
    {
        var source = new TestProfileNameSource
        {
            ProfileName = new ProfileName("default")
        };

        var expectedProfileName = new ProfileName("production");

        var actual = source.HasProfileName(expectedProfileName);

        Assert.False(actual);
    }

    private sealed class TestProfileNameSource : IHasProfileName
    {
        public ProfileName ProfileName { get; set; } = new ProfileName("default");
    }

    [Fact]
    public void GetNormalizedProfileName_WhenSourceIsNull_ThrowsArgumentNullException()
    {
        IHasProfileName? source = null;

        Assert.Throws<ArgumentNullException>(() =>
            source!.GetNormalizedProfileName());
    }

    [Theory]
    [InlineData("DEFAULT", "default")]
    [InlineData("DefaultProfile", "defaultprofile")]
    [InlineData("Markdown-Refine", "markdown-refine")]
    [InlineData("LIVE-CHAT-FAST", "live-chat-fast")]
    public void GetNormalizedProfileName_ReturnsLowercaseProfileName(
        string input,
        string expected)
    {
        var source = new TestProfileNameSource
        {
            ProfileName = new ProfileName(input)
        };

        var actual = source.GetNormalizedProfileName();

        Assert.Equal(expected, actual.Value);
    }

    [Fact]
    public void GetNormalizedProfileName_WhenProfileNameIsAlreadyLowercase_ReturnsSameValue()
    {
        var source = new TestProfileNameSource
        {
            ProfileName = new ProfileName("default")
        };

        var actual = source.GetNormalizedProfileName();

        Assert.Equal("default", actual.Value);
    }
}