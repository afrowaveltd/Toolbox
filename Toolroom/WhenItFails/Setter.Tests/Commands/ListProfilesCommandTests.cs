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
