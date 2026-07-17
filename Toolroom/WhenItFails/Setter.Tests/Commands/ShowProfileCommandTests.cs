using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class ShowProfileCommandTests
{
    [Fact]
    public void TryParseOptions_WithoutSwitch_ReturnsFalseOutputs()
    {
        string[] args = ["show-profile", ".", "WEB"];

        bool result = ShowProfileCommand.TryParseOptions(
            args,
            out bool usePlainOutput,
            out bool useJsonOutput);

        Assert.True(result);
        Assert.False(usePlainOutput);
        Assert.False(useJsonOutput);
    }

    [Fact]
    public void TryParseOptions_WithPlainSwitch_ReturnsTruePlainOutput()
    {
        string[] args = ["show-profile", ".", "WEB", "--plain"];

        bool result = ShowProfileCommand.TryParseOptions(
            args,
            out bool usePlainOutput,
            out bool useJsonOutput);

        Assert.True(result);
        Assert.True(usePlainOutput);
        Assert.False(useJsonOutput);
    }

    [Fact]
    public void TryParseOptions_WithCaseInsensitivePlainSwitch_ReturnsTrue()
    {
        string[] args = ["show-profile", ".", "WEB", "--PLAIN"];

        bool result = ShowProfileCommand.TryParseOptions(
            args,
            out bool usePlainOutput,
            out bool useJsonOutput);

        Assert.True(result);
        Assert.True(usePlainOutput);
        Assert.False(useJsonOutput);
    }

    [Theory]
    [InlineData("--json")]
    [InlineData("--JSON")]
    public void TryParseOptions_WithJsonSwitch_ReturnsTrueJsonOutput(string jsonSwitch)
    {
        string[] args = ["show-profile", ".", "WEB", jsonSwitch];

        bool result = ShowProfileCommand.TryParseOptions(
            args,
            out bool usePlainOutput,
            out bool useJsonOutput);

        Assert.True(result);
        Assert.False(usePlainOutput);
        Assert.True(useJsonOutput);
    }

    [Fact]
    public void TryParseOptions_WithUnknownArgument_ReturnsFalse()
    {
        string[] args = ["show-profile", ".", "WEB", "--unknown"];

        bool result = ShowProfileCommand.TryParseOptions(
            args,
            out _,
            out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParseOptions_WithDuplicatePlainSwitch_ReturnsFalse()
    {
        string[] args = ["show-profile", ".", "WEB", "--plain", "--plain"];

        bool result = ShowProfileCommand.TryParseOptions(
            args,
            out _,
            out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParseOptions_WithPlainAndJsonSwitches_ReturnsFalse()
    {
        string[] args = ["show-profile", ".", "WEB", "--plain", "--json"];

        bool result = ShowProfileCommand.TryParseOptions(
            args,
            out _,
            out _);

        Assert.False(result);
    }

    [Fact]
    public void FindProfile_FindsProfileByNormalizedNameIgnoringCase()
    {
        WhenItFailsWorkspaceSummary summary = CreateSummary();

        ErrorProfileDefinition? profile = ShowProfileCommand.FindProfile(
            summary,
            "web");

        Assert.NotNull(profile);
        Assert.Equal("WEB", profile.Name);
    }

    [Fact]
    public void FindProfile_FindsProfileByDisplayNameIgnoringCase()
    {
        WhenItFailsWorkspaceSummary summary = CreateSummary();

        ErrorProfileDefinition? profile = ShowProfileCommand.FindProfile(
            summary,
            "web application");

        Assert.NotNull(profile);
        Assert.Equal("WEB", profile.Name);
    }

    [Fact]
    public void FindProfile_WithUnknownName_ReturnsNull()
    {
        WhenItFailsWorkspaceSummary summary = CreateSummary();

        ErrorProfileDefinition? profile = ShowProfileCommand.FindProfile(
            summary,
            "UNKNOWN");

        Assert.Null(profile);
    }

    [Fact]
    public async Task ExecuteAsync_WithInitializedWorkspaceAndPlainOutput_ReturnsSuccess()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        int exitCode = await ShowProfileCommand.ExecuteAsync(
            ["show-profile", temporaryWorkspace.ProjectRootPath, "WEB", "--plain"]);

        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownProfile_ReturnsLookupError()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        int exitCode = await ShowProfileCommand.ExecuteAsync(
            ["show-profile", temporaryWorkspace.ProjectRootPath, "DOES_NOT_EXIST", "--plain"]);

        Assert.Equal(2, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutProfileName_ReturnsCommandInputError()
    {
        int exitCode = await ShowProfileCommand.ExecuteAsync(
            ["show-profile", "."]);

        Assert.Equal(1, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidWorkspace_ReturnsValidationError()
    {
        using TemporaryWhenItFailsWorkspace temporaryWorkspace =
            TemporaryWhenItFailsWorkspace.CreateEmpty();

        int exitCode = await ShowProfileCommand.ExecuteAsync(
            ["show-profile", temporaryWorkspace.ProjectRootPath, "WEB", "--plain"]);

        Assert.Equal(2, exitCode);
    }

    private static WhenItFailsWorkspaceSummary CreateSummary()
    {
        return new WhenItFailsWorkspaceSummary
        {
            ProfileCatalog = new ErrorProfileCatalogDocument
            {
                Profiles =
                [
                    new ErrorProfileDefinition
                    {
                        Name = "WEB",
                        DisplayName = "Web Application"
                    }
                ]
            }
        };
    }
}
