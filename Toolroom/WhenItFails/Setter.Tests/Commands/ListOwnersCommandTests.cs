namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

public sealed class ListOwnersCommandTests
{
    [Fact]
    public void TryParseOptions_WithoutOptionalSwitch_ReturnsRichOutput()
    {
        bool result = ListOwnersCommand.TryParseOptions(
            ["list-owners", "."],
            out bool usePlainOutput,
            out bool useJsonOutput);

        Assert.True(result);
        Assert.False(usePlainOutput);
        Assert.False(useJsonOutput);
    }

    [Fact]
    public void TryParseOptions_WithPlainSwitch_ReturnsPlainOutput()
    {
        bool result = ListOwnersCommand.TryParseOptions(
            ["list-owners", ".", "--plain"],
            out bool usePlainOutput,
            out bool useJsonOutput);

        Assert.True(result);
        Assert.True(usePlainOutput);
        Assert.False(useJsonOutput);
    }

    [Fact]
    public void TryParseOptions_WithJsonSwitch_ReturnsJsonOutput()
    {
        bool result = ListOwnersCommand.TryParseOptions(
            ["list-owners", ".", "--JSON"],
            out bool usePlainOutput,
            out bool useJsonOutput);

        Assert.True(result);
        Assert.False(usePlainOutput);
        Assert.True(useJsonOutput);
    }

    [Fact]
    public void TryParseOptions_WithUnknownArgument_ReturnsFalse()
    {
        bool result = ListOwnersCommand.TryParseOptions(
            ["list-owners", ".", "--unknown"],
            out _,
            out _);

        Assert.False(result);
    }

    [Theory]
    [InlineData("--plain", "--plain")]
    [InlineData("--json", "--json")]
    [InlineData("--plain", "--json")]
    public void TryParseOptions_WithDuplicateOrConflictingSwitches_ReturnsFalse(
        string first,
        string second)
    {
        bool result = ListOwnersCommand.TryParseOptions(
            ["list-owners", ".", first, second],
            out _,
            out _);

        Assert.False(result);
    }
}
