using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

public sealed class MdViewSessionTests
{
    [Fact]
    public async Task EnterOpensMainMenuThenNextDocumentCanBeDisplayed()
    {
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.Escape);
        var displayed = new List<string?>();
        var selectionCount = 0;

        var code = await new MdViewSession(console).RunAsync(
            new MdViewCommand(MdViewSourceKind.Local, "first.md"),
            _ =>
            {
                selectionCount++;
                return Task.FromResult<MdViewCommand?>(
                    new MdViewCommand(MdViewSourceKind.Local, "second.md"));
            },
            (command, _) =>
            {
                displayed.Add(command.Value);
                return Task.FromResult(new MdViewDisplayResult(0, true));
            },
            waitForCompletion: true);

        Assert.Equal(0, code);
        Assert.Equal(1, selectionCount);
        Assert.Equal(new[] { "first.md", "second.md" }, displayed);
        Assert.Equal(2, console.Output.Split("Enter: main menu", StringSplitOptions.None).Length - 1);
    }

    [Fact]
    public async Task EnterThenCancelMainMenuEndsCleanly()
    {
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.Enter);
        var selections = 0;
        var renders = 0;

        var code = await new MdViewSession(console).RunAsync(
            new MdViewCommand(MdViewSourceKind.GitHub, "owner", "repo"),
            _ =>
            {
                selections++;
                return Task.FromResult<MdViewCommand?>(null);
            },
            (_, _) =>
            {
                renders++;
                return Task.FromResult(new MdViewDisplayResult(0, true));
            },
            waitForCompletion: true);

        Assert.Equal(0, code);
        Assert.Equal(1, renders);
        Assert.Equal(1, selections);
    }

    [Fact]
    public async Task EscapeAfterDocumentDoesNotOpenMenu()
    {
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.Escape);
        var renders = 0;

        var code = await new MdViewSession(console).RunAsync(
            new MdViewCommand(MdViewSourceKind.Url, "https://example.org/readme.md"),
            _ => throw new InvalidOperationException("Should never open menu"),
            (_, _) =>
            {
                renders++;
                return Task.FromResult(new MdViewDisplayResult(0, true));
            },
            waitForCompletion: true);

        Assert.Equal(0, code);
        Assert.Equal(1, renders);
    }

    [Fact]
    public async Task NoninteractiveExecutionDoesNotWaitOrReturnToMenu()
    {
        var console = Interactive();
        var renders = 0;
        var code = await new MdViewSession(console).RunAsync(
            new MdViewCommand(MdViewSourceKind.Local, "file.md"),
            _ => throw new InvalidOperationException("Should never open menu"),
            (_, _) =>
            {
                renders++;
                return Task.FromResult(new MdViewDisplayResult(0, true));
            },
            waitForCompletion: false);

        Assert.Equal(0, code);
        Assert.Equal(1, renders);
        Assert.DoesNotContain("Enter: main menu", console.Output);
    }

    [Fact]
    public async Task ErrorDoesNotWaitForKeysOrHideExitCode()
    {
        var console = Interactive();
        var code = await new MdViewSession(console).RunAsync(
            new MdViewCommand(MdViewSourceKind.Local, "missing.md"),
            _ => throw new InvalidOperationException("Should never open menu"),
            (_, _) => Task.FromResult(new MdViewDisplayResult(1, false)),
            waitForCompletion: true);

        Assert.Equal(1, code);
        Assert.DoesNotContain("Enter: main menu", console.Output);
    }

    [Fact]
    public async Task CancelledRepositoryChooserDoesNotPromptForAnUnreadDocument()
    {
        var console = Interactive();
        var code = await new MdViewSession(console).RunAsync(
            new MdViewCommand(MdViewSourceKind.GitHub, "owner"),
            _ => throw new InvalidOperationException("Should never open menu"),
            (_, _) => Task.FromResult(new MdViewDisplayResult(0, false)),
            waitForCompletion: true);

        Assert.Equal(0, code);
        Assert.DoesNotContain("Enter: main menu", console.Output);
    }

    [Fact]
    public async Task SourcePagerKeepsItsOwnInteractionAndIsNotPromptedTwice()
    {
        var console = Interactive();
        var code = await new MdViewSession(console).RunAsync(
            new MdViewCommand(MdViewSourceKind.SourcePager, "file.md"),
            _ => throw new InvalidOperationException("Should never open menu"),
            (_, _) => Task.FromResult(new MdViewDisplayResult(0, true)),
            waitForCompletion: true);

        Assert.Equal(0, code);
        Assert.DoesNotContain("Enter: main menu", console.Output);
    }

    [Fact]
    public async Task MainMenuGitHubSelectionUsesLinkBrowserNotOneShotRenderer()
    {
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.DownArrow); // URL
        console.Input.PushKey(ConsoleKey.DownArrow); // GitHub navigation
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushTextWithEnter("afrowaveltd/Toolbox");

        MdViewCommand? dispatched = null;
        var code = await new MdViewSession(console).RunAsync(
            new MdViewCommand(MdViewSourceKind.Interactive),
            token => new MdViewInteractiveSourcePicker(console)
                .PickAsync(Path.GetTempPath(), token),
            (command, _) =>
            {
                dispatched = command;
                // Link browsing owns its own terminal loop, not the legacy
                // post-render completion prompt.
                return Task.FromResult(new MdViewDisplayResult(0, false));
            },
            waitForCompletion: true);

        Assert.Equal(0, code);
        Assert.Equal(MdViewSourceKind.GitHubLinkBrowser, dispatched?.Kind);
        Assert.Equal("afrowaveltd", dispatched?.Value);
        Assert.Equal("Toolbox", dispatched?.Repository);
        Assert.DoesNotContain("Enter: main menu", console.Output);
    }

    [Fact]
    public async Task StartingInInteractiveModeUsesMenuImmediately()
    {
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.Escape);
        var selections = 0;
        var renders = 0;
        var code = await new MdViewSession(console).RunAsync(
            new MdViewCommand(MdViewSourceKind.Interactive),
            _ =>
            {
                selections++;
                return Task.FromResult<MdViewCommand?>(
                    new MdViewCommand(MdViewSourceKind.Local, "README.md"));
            },
            (_, _) =>
            {
                renders++;
                return Task.FromResult(new MdViewDisplayResult(0, true));
            },
            waitForCompletion: true);

        Assert.Equal(0, code);
        Assert.Equal(1, selections);
        Assert.Equal(1, renders);
    }

    private static TestConsole Interactive()
    {
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        console.Profile.Capabilities.Unicode = true;
        console.Profile.Width = 120;
        return console;
    }
}
