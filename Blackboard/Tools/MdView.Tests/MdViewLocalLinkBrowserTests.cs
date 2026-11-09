using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>End-to-end opt-in Markdown link browsing with a virtual terminal.</summary>
public sealed class MdViewLocalLinkBrowserTests
{
    [Fact]
    public void BrowseCommandIsExplicitAndDoesNotChangeLocalMode()
    {
        var command = MdViewCommand.Parse("BROWSE", "README.md");
        Assert.Equal(MdViewSourceKind.LocalLinkBrowser, command.Kind);
        Assert.Equal("README.md", command.Value);
        Assert.Equal(MdViewSourceKind.Local,
            MdViewCommand.Parse("local", "README.md").Kind);
    }

    [Fact]
    public async Task FollowingLinkAndBackReopenOriginalDocument()
    {
        using var docs = new Docs();
        var home = docs.Write("README.md", "# Home\n[Go to docs](Docs/en.md)");
        docs.Write("Docs/en.md", "# Linked documentation");
        var terminal = Interactive();

        // README Follow -> picker Go -> Docs Back -> README Forward -> Docs Exit.
        terminal.Input.PushKey(ConsoleKey.Enter);
        terminal.Input.PushKey(ConsoleKey.Enter);
        terminal.Input.PushKey(ConsoleKey.Enter);
        terminal.Input.PushKey(ConsoleKey.DownArrow);
        terminal.Input.PushKey(ConsoleKey.Enter);
        terminal.Input.PushKey(ConsoleKey.DownArrow);
        terminal.Input.PushKey(ConsoleKey.Enter);

        await new MdViewLocalLinkBrowser(terminal).RunAsync(home);

        Assert.Contains("Linked documentation", terminal.Output);
        Assert.Contains("Home", terminal.Output);
        Assert.Contains("Back", terminal.Output);
    }

    [Fact]
    public async Task MissingDestinationLeavesCurrentPageAndAllowsExit()
    {
        using var docs = new Docs();
        var home = docs.Write("README.md", "# Home\n[Missing](missing.md)");
        var terminal = Interactive();

        // Follow, select the broken link; after the recoverable error,
        // exit the browser from the unchanged source document.
        terminal.Input.PushKey(ConsoleKey.Enter);
        terminal.Input.PushKey(ConsoleKey.Enter);
        terminal.Input.PushKey(ConsoleKey.DownArrow);
        terminal.Input.PushKey(ConsoleKey.Enter);

        await new MdViewLocalLinkBrowser(terminal).RunAsync(home);

        Assert.Contains("Unable to follow Markdown link", terminal.Output);
        Assert.Contains("Home", terminal.Output);
    }

    [Fact]
    public async Task CodeOnlyLinksAreNotOfferedAndEscapeExits()
    {
        using var docs = new Docs();
        var home = docs.Write("README.md",
            "# Home\n```markdown\n[not a link](secret.md)\n```");
        var terminal = Interactive();
        terminal.Input.PushKey(ConsoleKey.Escape);

        await new MdViewLocalLinkBrowser(terminal).RunAsync(home);

        Assert.DoesNotContain("Follow a link", terminal.Output);
    }

    [Fact]
    public async Task NoninteractiveBrowserRejectsWithoutReadingDocument()
    {
        var terminal = new TestConsole();
        terminal.Profile.Capabilities.Interactive = false;
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new MdViewLocalLinkBrowser(terminal).RunAsync(
                Path.Combine(Path.GetTempPath(), "does-not-exist.md")));
    }

    private static TestConsole Interactive()
    {
        var terminal = new TestConsole();
        terminal.Profile.Capabilities.Interactive = true;
        terminal.Profile.Capabilities.Unicode = true;
        terminal.Profile.Width = 120;
        return terminal;
    }

    private sealed class Docs : IDisposable
    {
        public string Root { get; } = Path.Combine(
            Path.GetTempPath(), "mdview-browse-" + Guid.NewGuid().ToString("N"));

        public Docs() => Directory.CreateDirectory(Root);

        public string Write(string name, string text)
        {
            var path = Path.Combine(Root,
                name.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, text);
            return path;
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }
}
