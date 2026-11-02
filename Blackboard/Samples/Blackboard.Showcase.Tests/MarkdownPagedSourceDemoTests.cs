using System.Text;
using Afrowave.Toolbox.Blackboard.Showcase;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Showcase.Tests;

public sealed class MarkdownPagedSourceDemoTests
{
    [Fact]
    public async Task Navigates_forward_then_back_without_materializing_entire_document()
    {
        var directory = CreateDirectory();
        try
        {
            var lines = Enumerable.Range(1, 43)
                .Select(i => i == 43 ? "শেষ বাংলা 🐈 line 43\n" :
                    $"line {i:00} [not markup]\n");
            var markdown = string.Concat(lines);
            var path = Path.Combine(directory, "README.md");
            await File.WriteAllTextAsync(path, markdown, new UTF8Encoding(false));

            var console = InteractiveConsole();
            console.Input.PushKey(ConsoleKey.Enter);  // pick README.md
            console.Input.PushKey(ConsoleKey.Enter);  // page 1 -> 2
            console.Input.PushKey(ConsoleKey.Enter);  // page 2 -> 1
            console.Input.PushKey(ConsoleKey.Escape); // close

            await new MarkdownPagedSourceDemo().RunAsync(
                console, directory, CancellationToken.None);

            Assert.Contains("page 1/2", console.Output);
            Assert.Contains("page 2/2", console.Output);
            Assert.Contains("line 40", console.Output);
            Assert.Contains("বাংলা 🐈 line 43", console.Output);
            Assert.Contains("[not markup]", console.Output);
            Assert.Equal(markdown, await File.ReadAllTextAsync(path));
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    [Fact]
    public async Task Can_toggle_conservative_fence_aware_preview_and_return_to_literal_source()
    {
        var directory = CreateDirectory();
        try
        {
            const string source = "~~~csharp\n# not a heading\n- not a list\n~~~\n";
            await File.WriteAllTextAsync(Path.Combine(directory, "preview.md"),
                source, new UTF8Encoding(false));
            var console = InteractiveConsole();
            console.Input.PushKey(ConsoleKey.Enter);      // pick Markdown
            console.Input.PushKey(ConsoleKey.DownArrow);  // select preview toggle
            console.Input.PushKey(ConsoleKey.Enter);      // preview on
            console.Input.PushKey(ConsoleKey.DownArrow);  // select source toggle
            console.Input.PushKey(ConsoleKey.Enter);      // preview off
            console.Input.PushKey(ConsoleKey.Escape);     // close

            await new MarkdownPagedSourceDemo().RunAsync(
                console, directory, CancellationToken.None);

            Assert.Contains("Markdown source: preview.md", console.Output);
            Assert.Contains("# not a heading", console.Output);
            Assert.Contains("- not a list", console.Output);
            Assert.Contains("csharp", console.Output);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    [Fact]
    public async Task Invalid_utf8_source_is_refused_without_rendering_its_content()
    {
        var directory = CreateDirectory();
        try
        {
            await File.WriteAllBytesAsync(Path.Combine(directory, "bad.md"),
                [0x23, 0x20, 0x41, 0x0A, 0x1B, 0x5B, 0x33, 0x31, 0x6D]);
            var console = InteractiveConsole();
            console.Input.PushKey(ConsoleKey.Enter);

            await new MarkdownPagedSourceDemo().RunAsync(
                console, directory, CancellationToken.None);

            Assert.Contains("Cannot index this Markdown source", console.Output);
            Assert.DoesNotContain("page 1/", console.Output);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    [Fact]
    public async Task Empty_file_and_cancelled_picker_are_safe()
    {
        var directory = CreateDirectory();
        try
        {
            await File.WriteAllBytesAsync(Path.Combine(directory, "blank.md"), []);
            var selected = InteractiveConsole();
            selected.Input.PushKey(ConsoleKey.Enter);
            await new MarkdownPagedSourceDemo().RunAsync(
                selected, directory, CancellationToken.None);
            Assert.Contains("file is empty", selected.Output);

            var cancelled = InteractiveConsole();
            cancelled.Input.PushKey(ConsoleKey.Escape);
            await new MarkdownPagedSourceDemo().RunAsync(
                cancelled, directory, CancellationToken.None);
            Assert.Contains("paging cancelled", cancelled.Output);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }

    private static string CreateDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "afrowave-page-ui-" +
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static TestConsole InteractiveConsole()
    {
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        console.Profile.Capabilities.Unicode = true;
        console.Profile.Width = 160;
        return console;
    }
}
