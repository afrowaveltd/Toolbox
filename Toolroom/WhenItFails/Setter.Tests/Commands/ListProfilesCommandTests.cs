using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class ListProfilesCommandTests
{
    [Fact]
    public void TryParseOptions_WithPathOnly_ReturnsRichOutput()
    {
        string[] args = ["list-profiles", "."];

        bool isValid = ListProfilesCommand.TryParseOptions(
            args,
            out bool usePlainOutput);

        Assert.True(isValid);
        Assert.False(usePlainOutput);
    }

    [Theory]
    [InlineData("--plain")]
    [InlineData("--PLAIN")]
    public void TryParseOptions_WithPlainSwitch_ReturnsPlainOutput(string plainSwitch)
    {
        string[] args = ["list-profiles", ".", plainSwitch];

        bool isValid = ListProfilesCommand.TryParseOptions(
            args,
            out bool usePlainOutput);

        Assert.True(isValid);
        Assert.True(usePlainOutput);
    }

    [Theory]
    [InlineData("--json")]
    [InlineData("unexpected")]
    public void TryParseOptions_WithUnknownArgument_ReturnsInvalid(string unknownArgument)
    {
        string[] args = ["list-profiles", ".", unknownArgument];

        bool isValid = ListProfilesCommand.TryParseOptions(
            args,
            out bool usePlainOutput);

        Assert.False(isValid);
        Assert.False(usePlainOutput);
    }

    [Fact]
    public void TryParseOptions_WithDuplicatePlainSwitch_ReturnsInvalid()
    {
        string[] args = ["list-profiles", ".", "--plain", "--plain"];

        bool isValid = ListProfilesCommand.TryParseOptions(
            args,
            out bool usePlainOutput);

        Assert.False(isValid);
        Assert.True(usePlainOutput);
    }
}
