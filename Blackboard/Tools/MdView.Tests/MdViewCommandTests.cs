using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

public sealed class MdViewCommandTests
{
    [Fact] public void NoArgumentsUsesMenu() => Assert.Equal(MdViewSourceKind.Interactive, MdViewCommand.Parse().Kind);
    [Fact] public void HelpSwitchIsRecognized() => Assert.Equal(MdViewSourceKind.Help, MdViewCommand.Parse("--help").Kind);
    [Fact] public void DirectLocalFile() => Assert.Equal("README.md", MdViewCommand.Parse("README.md").Value);
    [Fact] public void LocalVerbIgnoresCapitalization() =>
        Assert.Equal(MdViewSourceKind.Local, MdViewCommand.Parse("LOCAL", "Docs/Help/en.md").Kind);
    [Fact] public void DirectHttpsUrl() =>
        Assert.Equal(MdViewSourceKind.Url, MdViewCommand.Parse("https://example.org/manual.md").Kind);
    [Fact] public void GitHubColonShortcut()
    {
        var value = MdViewCommand.Parse("github:AfrowaveLtd/Toolbox");
        Assert.Equal(MdViewSourceKind.GitHub, value.Kind);
        Assert.Equal("AfrowaveLtd", value.Value);
        Assert.Equal("Toolbox", value.Repository);
    }
    [Fact] public void PositionalGitHub()
    {
        var value = MdViewCommand.Parse("GIT", "afrowaveltd", "toolbox");
        Assert.Equal(MdViewSourceKind.GitHub, value.Kind);
        Assert.Equal("afrowaveltd", value.Value);
        Assert.Equal("toolbox", value.Repository);
    }
    [Theory]
    [InlineData("git", "AfrowaveLtd")]
    [InlineData("GITHUB", "afrowaveltd")]
    public void OwnerOnlyVerbOpensRepositoryBrowser(string verb, string owner)
    {
        var command = MdViewCommand.Parse(verb, owner);
        Assert.Equal(MdViewSourceKind.GitHub, command.Kind);
        Assert.Equal(owner, command.Value);
        Assert.Null(command.Repository);
    }

    [Theory]
    [InlineData("github:afrowaveltd")]
    [InlineData("afrowaveltd")]
    public void OwnerOnlyShortcutOpensRepositoryBrowser(string input)
    {
        var command = MdViewCommand.Parse(input);
        Assert.Equal(MdViewSourceKind.GitHub, command.Kind);
        Assert.Equal("afrowaveltd", command.Value);
        Assert.Null(command.Repository);
    }

    [Fact]
    public void ExplicitLocalFileStillWinsOverOwnerDiscovery() =>
        Assert.Equal(MdViewSourceKind.Local, MdViewCommand.Parse("local", "afrowaveltd").Kind);

    [Theory]
    [InlineData("git", "owner/repo/extra")]
    [InlineData("unknown", "argument")]
    public void InvalidCommandsAreRejected(string verb, string value) =>
        Assert.Throws<ArgumentException>(() => MdViewCommand.Parse(verb, value));
}
