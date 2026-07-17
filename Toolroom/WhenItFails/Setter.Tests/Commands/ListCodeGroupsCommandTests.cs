namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

public sealed class ListCodeGroupsCommandTests
{
    [Fact]
    public void TryParseOptions_WithoutOptionalSwitch_ReturnsRichOutput()
    {
        bool result = ListCodeGroupsCommand.TryParseOptions(
            ["list-code-groups", "."],
            out bool usePlainOutput,
            out bool useJsonOutput);

        Assert.True(result);
        Assert.False(usePlainOutput);
        Assert.False(useJsonOutput);
    }

    [Fact]
    public void TryParseOptions_WithPlainSwitch_ReturnsPlainOutput()
    {
        bool result = ListCodeGroupsCommand.TryParseOptions(
            ["list-code-groups", ".", "--plain"],
            out bool usePlainOutput,
            out bool useJsonOutput);

        Assert.True(result);
        Assert.True(usePlainOutput);
        Assert.False(useJsonOutput);
    }

    [Fact]
    public void TryParseOptions_WithJsonSwitch_ReturnsJsonOutput()
    {
        bool result = ListCodeGroupsCommand.TryParseOptions(
            ["list-code-groups", ".", "--JSON"],
            out bool usePlainOutput,
            out bool useJsonOutput);

        Assert.True(result);
        Assert.False(usePlainOutput);
        Assert.True(useJsonOutput);
    }

    [Fact]
    public void TryParseOptions_WithUnknownArgument_ReturnsFalse()
    {
        bool result = ListCodeGroupsCommand.TryParseOptions(
            ["list-code-groups", ".", "--unknown"],
            out _,
            out _);

        Assert.False(result);
    }

    [Theory]
    [InlineData("--plain", "--plain")]
    [InlineData("--json", "--json")]
    [InlineData("--plain", "--json")]
    [InlineData("--json", "--plain")]
    public void TryParseOptions_WithDuplicateOrConflictingOutputSwitches_ReturnsFalse(
        string firstSwitch,
        string secondSwitch)
    {
        bool result = ListCodeGroupsCommand.TryParseOptions(
            ["list-code-groups", ".", firstSwitch, secondSwitch],
            out _,
            out _);

        Assert.False(result);
    }
}
