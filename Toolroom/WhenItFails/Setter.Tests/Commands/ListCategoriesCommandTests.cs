namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

public sealed class ListCategoriesCommandTests
{
    [Fact]
    public void TryParseOptions_WithoutOptionalSwitch_ReturnsRichOutput()
    {
        bool result = ListCategoriesCommand.TryParseOptions(
            ["list-categories", "."],
            out bool usePlainOutput,
            out bool useJsonOutput);

        Assert.True(result);
        Assert.False(usePlainOutput);
        Assert.False(useJsonOutput);
    }

    [Fact]
    public void TryParseOptions_WithPlainSwitch_ReturnsPlainOutput()
    {
        bool result = ListCategoriesCommand.TryParseOptions(
            ["list-categories", ".", "--plain"],
            out bool usePlainOutput,
            out bool useJsonOutput);

        Assert.True(result);
        Assert.True(usePlainOutput);
        Assert.False(useJsonOutput);
    }

    [Fact]
    public void TryParseOptions_WithJsonSwitch_ReturnsJsonOutput()
    {
        bool result = ListCategoriesCommand.TryParseOptions(
            ["list-categories", ".", "--JSON"],
            out bool usePlainOutput,
            out bool useJsonOutput);

        Assert.True(result);
        Assert.False(usePlainOutput);
        Assert.True(useJsonOutput);
    }

    [Fact]
    public void TryParseOptions_WithUnknownArgument_ReturnsFalse()
    {
        bool result = ListCategoriesCommand.TryParseOptions(
            ["list-categories", ".", "--unknown"],
            out _,
            out _);

        Assert.False(result);
    }

    [Theory]
    [InlineData("--plain", "--plain")]
    [InlineData("--json", "--json")]
    [InlineData("--plain", "--json")]
    public void TryParseOptions_WithConflictingSwitches_ReturnsFalse(
        string first,
        string second)
    {
        bool result = ListCategoriesCommand.TryParseOptions(
            ["list-categories", ".", first, second],
            out _,
            out _);

        Assert.False(result);
    }
}
