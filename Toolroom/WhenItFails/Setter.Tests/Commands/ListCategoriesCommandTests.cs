namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

public sealed class ListCategoriesCommandTests
{
    [Fact]
    public void TryParseOptions_WithoutOptionalSwitch_ReturnsTrueAndPlainFalse()
    {
        bool result = ListCategoriesCommand.TryParseOptions(
            ["list-categories", "."],
            out bool usePlainOutput);

        Assert.True(result);
        Assert.False(usePlainOutput);
    }

    [Fact]
    public void TryParseOptions_WithPlainSwitch_ReturnsTrueAndPlainTrue()
    {
        bool result = ListCategoriesCommand.TryParseOptions(
            ["list-categories", ".", "--plain"],
            out bool usePlainOutput);

        Assert.True(result);
        Assert.True(usePlainOutput);
    }

    [Fact]
    public void TryParseOptions_WithCaseInsensitivePlainSwitch_ReturnsTrue()
    {
        bool result = ListCategoriesCommand.TryParseOptions(
            ["list-categories", ".", "--PLAIN"],
            out bool usePlainOutput);

        Assert.True(result);
        Assert.True(usePlainOutput);
    }

    [Fact]
    public void TryParseOptions_WithUnknownArgument_ReturnsFalse()
    {
        bool result = ListCategoriesCommand.TryParseOptions(
            ["list-categories", ".", "--unknown"],
            out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParseOptions_WithDuplicatePlainSwitch_ReturnsFalse()
    {
        bool result = ListCategoriesCommand.TryParseOptions(
            ["list-categories", ".", "--plain", "--plain"],
            out _);

        Assert.False(result);
    }
}
