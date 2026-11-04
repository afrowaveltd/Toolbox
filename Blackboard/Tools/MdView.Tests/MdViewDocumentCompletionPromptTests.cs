using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

public sealed class MdViewDocumentCompletionPromptTests
{
    [Fact]
    public async Task EnterReturnsToMainMenu()
    {
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.Enter);

        var action = await new MdViewDocumentCompletionPrompt(console).WaitAsync();

        Assert.Equal(MdViewCompletionAction.MainMenu, action);
        Assert.Contains("Enter: main menu", console.Output);
        Assert.Contains("Esc: exit", console.Output);
    }

    [Fact]
    public async Task EscapeExitsWithoutAnotherMenu()
    {
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.Escape);

        var action = await new MdViewDocumentCompletionPrompt(console).WaitAsync();

        Assert.Equal(MdViewCompletionAction.Exit, action);
    }

    [Fact]
    public async Task ReservedSearchAndPagingKeysAreIgnoredUntilEnter()
    {
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.F);
        console.Input.PushKey(ConsoleKey.PageUp);
        console.Input.PushKey(ConsoleKey.PageDown);
        console.Input.PushKey(ConsoleKey.UpArrow);
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.Enter);

        var action = await new MdViewDocumentCompletionPrompt(console).WaitAsync();

        Assert.Equal(MdViewCompletionAction.MainMenu, action);
    }

    [Fact]
    public async Task UnrelatedKeysAreIgnoredUntilEscape()
    {
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.Spacebar);
        console.Input.PushKey(ConsoleKey.Tab);
        console.Input.PushKey(ConsoleKey.F1);
        console.Input.PushKey(ConsoleKey.Escape);

        var action = await new MdViewDocumentCompletionPrompt(console).WaitAsync();

        Assert.Equal(MdViewCompletionAction.Exit, action);
    }

    [Fact]
    public async Task NoninteractiveConsoleDoesNotWaitForAnyKey()
    {
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = false;

        var action = await new MdViewDocumentCompletionPrompt(console).WaitAsync();

        Assert.Equal(MdViewCompletionAction.Exit, action);
        Assert.DoesNotContain("Enter: main menu", console.Output);
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
