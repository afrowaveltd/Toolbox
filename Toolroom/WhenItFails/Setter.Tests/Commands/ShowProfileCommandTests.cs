using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class ShowProfileCommandTests
{
    [Fact]
    public void TryParseOptions_WithoutSwitch_ReturnsFalsePlainOutput()
    {
        string[] args = ["show-profile", ".", "WEB"];

        bool result = ShowProfileCommand.TryParseOptions(
            args,
            out bool usePlainOutput);

        Assert.True(result);
        Assert.False(usePlainOutput);
    }

    [Fact]
    public void TryParseOptions_WithPlainSwitch_ReturnsTruePlainOutput()
    {
        string[] args = ["show-profile", ".", "WEB", "--plain"];

        bool result = ShowProfileCommand.TryParseOptions(
            args,
            out bool usePlainOutput);

        Assert.True(result);
        Assert.True(usePlainOutput);
    }

    [Fact]
    public void TryParseOptions_WithCaseInsensitivePlainSwitch_ReturnsTrue()
    {
        string[] args = ["show-profile", ".", "WEB", "--PLAIN"];

        bool result = ShowProfileCommand.TryParseOptions(
            args,
            out bool usePlainOutput);

        Assert.True(result);
        Assert.True(usePlainOutput);
    }

    [Fact]
    public void TryParseOptions_WithUnknownArgument_ReturnsFalse()
    {
        string[] args = ["show-profile", ".", "WEB", "--json"];

        bool result = ShowProfileCommand.TryParseOptions(
            args,
            out _);

        Assert.False(result);
    }

    [Fact]
    public void TryParseOptions_WithDuplicatePlainSwitch_ReturnsFalse()
    {
        string[] args = ["show-profile", ".", "WEB", "--plain", "--plain"];

        bool result = ShowProfileCommand.TryParseOptions(
            args,
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
