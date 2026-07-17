using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class ListProfilesCommandTests
{
    [Fact]
    public void TryParseOptions_WithPathOnly_ReturnsRichOutput()
    {
        string[] args = ["list-profiles", "."];

        bool isValid = ListProfilesCommand.TryParseOptions(
            args,
            out bool usePlainOutput,
            out bool useJsonOutput);

        Assert.True(isValid);
        Assert.False(usePlainOutput);
        Assert.False(useJsonOutput);
    }

    [Theory]
    [InlineData("--plain")]
    [InlineData("--PLAIN")]
    public void TryParseOptions_WithPlainSwitch_ReturnsPlainOutput(string plainSwitch)
    {
        string[] args = ["list-profiles", ".", plainSwitch];

        bool isValid = ListProfilesCommand.TryParseOptions(
            args,
            out bool usePlainOutput,
            out bool useJsonOutput);

        Assert.True(isValid);
        Assert.True(usePlainOutput);
        Assert.False(useJsonOutput);
    }

    [Theory]
    [InlineData("--json")]
    [InlineData("--JSON")]
    public void TryParseOptions_WithJsonSwitch_ReturnsJsonOutput(string jsonSwitch)
    {
        string[] args = ["list-profiles", ".", jsonSwitch];

        bool isValid = ListProfilesCommand.TryParseOptions(
            args,
            out bool usePlainOutput,
            out bool useJsonOutput);

        Assert.True(isValid);
        Assert.False(usePlainOutput);
        Assert.True(useJsonOutput);
    }

    [Fact]
    public void TryParseOptions_WithUnknownArgument_ReturnsInvalid()
    {
        string[] args = ["list-profiles", ".", "unexpected"];

        bool isValid = ListProfilesCommand.TryParseOptions(
            args,
            out bool usePlainOutput,
            out bool useJsonOutput);

        Assert.False(isValid);
        Assert.False(usePlainOutput);
        Assert.False(useJsonOutput);
    }

    [Theory]
    [InlineData("--plain", "--plain")]
    [InlineData("--json", "--json")]
    [InlineData("--plain", "--json")]
    [InlineData("--json", "--plain")]
    public void TryParseOptions_WithDuplicateOrConflictingOutputSwitches_ReturnsInvalid(
        string firstSwitch,
        string secondSwitch)
    {
        string[] args = ["list-profiles", ".", firstSwitch, secondSwitch];

        bool isValid = ListProfilesCommand.TryParseOptions(
            args,
            out _,
            out _);

        Assert.False(isValid);
    }

    [Fact]
    public async Task ExecuteAsync_WithInitializedWorkspaceAndPlainOutput_ReturnsSuccess()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        int exitCode = await ListProfilesCommand.ExecuteAsync(
            ["list-profiles", temporaryWorkspace.ProjectRootPath, "--plain"]);

        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidWorkspace_ReturnsValidationError()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            TemporaryWhenItFailsWorkspace.CreateEmpty();

        int exitCode = await ListProfilesCommand.ExecuteAsync(
            ["list-profiles", temporaryWorkspace.ProjectRootPath, "--plain"]);

        Assert.Equal(2, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutPath_ReturnsCommandInputError()
    {
        int exitCode = await ListProfilesCommand.ExecuteAsync(
            ["list-profiles"]);

        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownArgument_ReturnsCommandInputError()
    {
        int exitCode = await ListProfilesCommand.ExecuteAsync(
            ["list-profiles", ".", "--unknown"]);

        Assert.Equal(1, exitCode);
    }
}
