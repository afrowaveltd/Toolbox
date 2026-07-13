namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

public sealed class PlainOutputOptionParserTests
{
    [Fact]
    public void TryParse_WithoutOptionalArguments_ReturnsTrueAndPlainFalse()
    {
        bool result = PlainOutputOptionParser.TryParse(
            ["list-profiles", "."],
            optionStartIndex: 2,
            out bool usePlainOutput);

        Assert.True(result);
        Assert.False(usePlainOutput);
    }

    [Fact]
    public void TryParse_WithPlainSwitch_ReturnsTrueAndPlainTrue()
    {
        bool result = PlainOutputOptionParser.TryParse(
            ["list-profiles", ".", "--plain"],
            optionStartIndex: 2,
            out bool usePlainOutput);

        Assert.True(result);
        Assert.True(usePlainOutput);
    }

    [Fact]
    public void TryParse_WithCaseInsensitivePlainSwitch_ReturnsTrue()
    {
        bool result = PlainOutputOptionParser.TryParse(
            ["show-profile", ".", "WEB", "--PLAIN"],
            optionStartIndex: 3,
            out bool usePlainOutput);

        Assert.True(result);
        Assert.True(usePlainOutput);
    }

    [Fact]
    public void TryParse_WithUnknownArgument_ReturnsFalse()
    {
        bool result = PlainOutputOptionParser.TryParse(
            ["list-profiles", ".", "--unknown"],
            optionStartIndex: 2,
            out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParse_WithDuplicatePlainSwitch_ReturnsFalse()
    {
        bool result = PlainOutputOptionParser.TryParse(
            ["list-profiles", ".", "--plain", "--plain"],
            optionStartIndex: 2,
            out _);

        Assert.False(result);
    }
}
