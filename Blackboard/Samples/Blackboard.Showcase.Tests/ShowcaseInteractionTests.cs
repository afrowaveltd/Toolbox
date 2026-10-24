using Afrowave.Toolbox.Blackboard.Showcase;
using Spectre.Console;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Showcase.Tests;

public sealed class ShowcaseInteractionTests
{
    [Fact]
    public async Task Showcase_is_extendable_and_escape_exits_main_menu()
    {
        var console = ConsoleWithInput();
        var demo = new DummyDemo("first");
        console.Input.PushKey(ConsoleKey.Escape);

        await new BlackboardShowcaseHost([demo], console).RunAsync("/start");

        Assert.Equal(0, demo.Calls);
        Assert.Contains("Choose a demonstration", console.Output);
    }

    [Fact]
    public async Task Showcase_runs_selected_module_and_returns_to_menu()
    {
        var console = ConsoleWithInput();
        var demo = new DummyDemo("preview");
        console.Input.PushKey(ConsoleKey.Enter);
        console.Input.PushKey(ConsoleKey.Escape);

        await new BlackboardShowcaseHost([demo], console).RunAsync("/start");

        Assert.Equal(1, demo.Calls);
        Assert.Equal("/start", demo.LastDirectory);
    }

    [Fact]
    public void Duplicate_or_empty_module_identifiers_are_rejected()
    {
        var console = ConsoleWithInput();
        Assert.Throws<ArgumentException>(() =>
            new BlackboardShowcaseHost([new DummyDemo("dup"), new DummyDemo("dup")], console));
        Assert.Throws<ArgumentException>(() =>
            new BlackboardShowcaseHost([new DummyDemo("")], console));
    }

    [Fact]
    public async Task JSON_demo_filters_out_non_json_and_previews_selected_json_in_console()
    {
        var root = Path.Combine(Path.GetTempPath(), "ShowcaseJSON-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "dictionary.json"),
                """{"hello":"ahoj","count":3}""");
            await File.WriteAllTextAsync(Path.Combine(root, "notes.txt"), "not JSON");

            var console = ConsoleWithInput();
            console.Input.PushKey(ConsoleKey.Enter);
            await new FileBrowserJsonDemo().RunAsync(console, root, CancellationToken.None);

            Assert.Contains("dictionary.json", console.Output);
            Assert.Contains("hello", console.Output);
            Assert.Contains("ahoj", console.Output);
            Assert.DoesNotContain("notes.txt", console.Output);
            Assert.Contains("JSON preview", console.Output);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task JSON_demo_refuses_invalid_selected_json()
    {
        var root = Path.Combine(Path.GetTempPath(), "ShowcaseBadJSON-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "invalid.json"), "{not json}");
            var console = ConsoleWithInput();
            console.Input.PushKey(ConsoleKey.Enter);

            await new FileBrowserJsonDemo().RunAsync(console, root, CancellationToken.None);

            Assert.Contains("JSON preview unavailable", console.Output);
            Assert.Contains("Invalid JSON", console.Output);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }


    [Fact]
    public async Task General_text_preview_demo_displays_selected_readable_file()
    {
        var root = Path.Combine(Path.GetTempPath(), "ShowcaseText-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "README.txt"),
                "Český text pro Blackboard");
            var console = ConsoleWithInput();
            console.Input.PushKey(ConsoleKey.Enter);

            await new FileBrowserPreviewDemo().RunAsync(console, root, CancellationToken.None);

            Assert.Contains("README.txt", console.Output);
            Assert.Contains("Český text pro Blackboard", console.Output);
            Assert.Contains("Text preview", console.Output);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Directory_demo_can_select_current_folder_without_modification()
    {
        var root = Path.Combine(Path.GetTempPath(), "ShowcaseDirectory-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var console = ConsoleWithInput();
            // Empty directory: Parent is first menu item, Select current folder second.
            console.Input.PushKey(ConsoleKey.DownArrow);
            console.Input.PushKey(ConsoleKey.Enter);

            await new FileBrowserDirectoryDemo().RunAsync(console, root, CancellationToken.None);

            Assert.Contains("Selected directory: " + root, console.Output);
            Assert.True(Directory.Exists(root));
        }
        finally
        {
            Directory.Delete(root);
        }
    }

    private static TestConsole ConsoleWithInput()
    {
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        console.Profile.Capabilities.Unicode = true;
        return console;
    }

    private sealed class DummyDemo : IBlackboardShowcaseDemo
    {
        public DummyDemo(string id) { Id = id; }
        public string Id { get; }
        public string Title => "Dummy demo";
        public int Calls { get; private set; }
        public string? LastDirectory { get; private set; }

        public Task RunAsync(
            IAnsiConsole console, string startingDirectory, CancellationToken cancellationToken)
        {
            Calls++;
            LastDirectory = startingDirectory;
            return Task.CompletedTask;
        }
    }
}
