using Afrowave.Toolbox.Blackboard.Showcase;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Showcase.Tests;

public sealed class MarkdownThemeDemoTests
{
    [Fact]
    public async Task Escape_from_gallery_returns_without_rendering_or_touching_files()
    {
        var console = InteractiveConsole();
        console.Input.PushKey(ConsoleKey.Escape);
        await new MarkdownThemeDemo().RunAsync(console, "/unused", CancellationToken.None);

        Assert.Contains("Choose a Markdown theme", console.Output);
        Assert.DoesNotContain("Theme preview:", console.Output);
    }

    [Fact]
    public async Task Default_preset_renders_parsed_markdown_then_returns_to_gallery()
    {
        var console = InteractiveConsole();
        console.Input.PushKey(ConsoleKey.Enter);  // Classic
        console.Input.PushKey(ConsoleKey.Enter);  // Return to gallery
        console.Input.PushKey(ConsoleKey.Escape); // Back to Showcase

        await new MarkdownThemeDemo().RunAsync(console, "/unused", CancellationToken.None);

        Assert.Contains("Theme preview: Classic", console.Output);
        Assert.Contains("Markdown theme gallery", console.Output);
        Assert.Contains("One document can have many appearances.", console.Output);
        Assert.Contains("../Docs/en.md", console.Output);
        Assert.Contains("MarkdownSpectreRenderer.Render", console.Output);
        Assert.DoesNotContain("**document**", console.Output);
    }

    [Fact]
    public async Task Switching_preset_renders_ocean_then_forest_with_same_source()
    {
        var console = InteractiveConsole();
        console.Input.PushKey(ConsoleKey.DownArrow); // Ocean
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.Enter); // Return
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.DownArrow); // Forest
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.Enter); // Return
        console.Input.PushKey(ConsoleKey.Escape);

        await new MarkdownThemeDemo().RunAsync(console, "/unused", CancellationToken.None);

        Assert.Contains("Theme preview: Ocean", console.Output);
        Assert.Contains("Theme preview: Forest", console.Output);
        Assert.Equal(2, console.Output.Split("Markdown theme gallery", StringSplitOptions.None).Length - 1);
    }

    [Fact]
    public async Task Compare_option_renders_all_four_presets()
    {
        var console = InteractiveConsole();
        for (var i = 0; i < 4; i++)
            console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.Enter); // Compare all
        console.Input.PushKey(ConsoleKey.Enter); // Return to gallery
        console.Input.PushKey(ConsoleKey.Escape);

        await new MarkdownThemeDemo().RunAsync(console, "/unused", CancellationToken.None);

        foreach (var name in new[] { "Classic", "Ocean", "Forest", "Monochrome" })
            Assert.Contains("Theme preview: " + name, console.Output);
    }

    [Fact]
    public async Task Gallery_can_run_through_the_registered_showcase_host()
    {
        var console = InteractiveConsole();
        console.Input.PushKey(ConsoleKey.Enter); // Host: Markdown module
        console.Input.PushKey(ConsoleKey.Escape); // Gallery: Back
        console.Input.PushKey(ConsoleKey.Escape); // Host: Exit

        var demo = new MarkdownThemeDemo();
        Assert.Equal("markdown-themes", demo.Id);
        await new BlackboardShowcaseHost([demo], console).RunAsync("/unused");

        Assert.Contains("Markdown gallery", console.Output);
        Assert.Contains("Choose a demonstration", console.Output);
    }

    [Fact]
    public async Task Cancellation_prevents_gallery_prompts()
    {
        using var token = new CancellationTokenSource();
        token.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new MarkdownThemeDemo().RunAsync(InteractiveConsole(), "/unused", token.Token));
    }

    private static TestConsole InteractiveConsole()
    {
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        console.Profile.Capabilities.Unicode = true;
        console.Profile.Width = 120;
        return console;
    }
}
