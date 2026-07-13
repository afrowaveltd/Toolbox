namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

public sealed class ListCodeGroupsCommandTests
{
    [Fact]
    public void TryParseOptions_WithoutOptionalSwitch_ReturnsTrueAndPlainFalse()
    {
        bool result = ListCodeGroupsCommand.TryParseOptions(
            ["list-code-groups", "."],
            out bool usePlainOutput);

        Assert.True(result);
        Assert.False(usePlainOutput);
    }

    [Fact]
    public void TryParseOptions_WithPlainSwitch_ReturnsTrueAndPlainTrue()
    {
        bool result = ListCodeGroupsCommand.TryParseOptions(
            ["list-code-groups", ".", "--plain"],
            out bool usePlainOutput);

        Assert.True(result);
        Assert.True(usePlainOutput);
    }

    [Fact]
    public void TryParseOptions_WithCaseInsensitivePlainSwitch_ReturnsTrue()
    {
        bool result = ListCodeGroupsCommand.TryParseOptions(
            ["list-code-groups", ".", "--PLAIN"],
            out bool usePlainOutput);

        Assert.True(result);
        Assert.True(usePlainOutput);
    }

    [Fact]
    public void TryParseOptions_WithUnknownArgument_ReturnsFalse()
    {
        bool result = ListCodeGroupsCommand.TryParseOptions(
            ["list-code-groups", ".", "--unknown"],
            out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParseOptions_WithDuplicatePlainSwitch_ReturnsFalse()
    {
        bool result = ListCodeGroupsCommand.TryParseOptions(
            ["list-code-groups", ".", "--plain", "--plain"],
            out _);

        Assert.False(result);
    }
}
