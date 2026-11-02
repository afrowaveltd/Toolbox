using System.Text;
using Afrowave.Toolbox.Blackboard.Showcase;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Showcase.Tests;

public sealed class MarkdownFilePreviewDemoTests
{
    [Theory]
    [InlineData("README.md", true)]
    [InlineData("Docs.MARKDOWN", true)]
    [InlineData("notes.txt", false)]
    [InlineData("README.md.exe", false)]
    public void Extension_is_validated_independently_of_picker_filter(
        string filename, bool expected)
    {
        Assert.Equal(expected, MarkdownFilePreviewDemo.IsMarkdownPath(filename));
    }

    [Fact]
    public async Task Picks_real_readme_and_renders_markdown_code_as_literal_text()
    {
        var root = NewTempDirectory();
        try
        {
            const string markdown =
                "# Český README\n\n- **Start**\n  12) Nested\n\n```csharp\npublic static void Main(string[] args)\n{\n    Console.WriteLine(\"[red]not Spectre markup[/]\");\n}\n```\n";
            var path = Path.Combine(root, "README.md");
            await File.WriteAllTextAsync(path, markdown, new UTF8Encoding(false));
            await File.WriteAllTextAsync(Path.Combine(root, "notes.txt"), "Not Markdown");

            var console = InteractiveConsole();
            console.Input.PushKey(ConsoleKey.Enter);
            await new MarkdownFilePreviewDemo().RunAsync(console, root, CancellationToken.None);

            Assert.Contains("README.md", console.Output);
            Assert.Contains("Český README", console.Output);
            Assert.Contains("12) Nested", console.Output);
            Assert.Contains("public static void Main(string[] args)", console.Output);
            Assert.Contains("[red]not Spectre markup[/]", console.Output);
            Assert.DoesNotContain("```csharp", console.Output);
            Assert.DoesNotContain("notes.txt", console.Output);
            Assert.Equal(markdown, await File.ReadAllTextAsync(path));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Uppercase_extension_and_unicode_bom_are_accepted()
    {
        var root = NewTempDirectory();
        try
        {
            var path = Path.Combine(root, "README.MD");
            var bytes = Encoding.Unicode.GetPreamble()
                .Concat(Encoding.Unicode.GetBytes("# Překlad\n**Důležité**")).ToArray();
            await File.WriteAllBytesAsync(path, bytes);

            var console = InteractiveConsole();
            console.Input.PushKey(ConsoleKey.Enter);
            await new MarkdownFilePreviewDemo().RunAsync(console, root, CancellationToken.None);

            Assert.Contains("Překlad", console.Output);
            Assert.Contains("Důležité", console.Output);
            Assert.Contains("UTF-16 LE", console.Output);
            Assert.DoesNotContain("**Důležité**", console.Output);
            Assert.Equal(bytes, await File.ReadAllBytesAsync(path));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Invalid_or_unsafe_markdown_is_refused_before_rendering()
    {
        foreach (var bytes in new byte[][] {
                     [0x00, 0x01, 0x02],
                     Encoding.UTF8.GetBytes("# Hello\u001b[31mbad"),
                     [0xC3, 0x28]
                 })
        {
            var root = NewTempDirectory();
            try
            {
                await File.WriteAllBytesAsync(Path.Combine(root, "unsafe.md"), bytes);
                var console = InteractiveConsole();
                console.Input.PushKey(ConsoleKey.Enter);
                await new MarkdownFilePreviewDemo().RunAsync(console, root, CancellationToken.None);
                Assert.Contains("Markdown preview unavailable", console.Output);
                Assert.DoesNotContain("Hello", console.Output);
            }
            finally { Directory.Delete(root, recursive: true); }
        }
    }

    [Fact]
    public async Task Oversize_file_is_marked_partial_and_read_is_bounded()
    {
        var root = NewTempDirectory();
        try
        {
            var path = Path.Combine(root, "long.markdown");
            var source = "# Header\n" + new string('x', MarkdownFilePreviewDemo.MaxPreviewBytes + 256);
            await File.WriteAllTextAsync(path, source);

            var console = InteractiveConsole();
            console.Input.PushKey(ConsoleKey.Enter);
            await new MarkdownFilePreviewDemo().RunAsync(console, root, CancellationToken.None);

            Assert.Contains("[Partial preview: only the first 64 KiB", console.Output);
            Assert.Contains("Header", console.Output);
            Assert.DoesNotContain("[Preview truncated at byte limit]", console.Output);
            Assert.Equal(source, await File.ReadAllTextAsync(path));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Empty_file_and_escape_are_non_destructive()
    {
        var root = NewTempDirectory();
        try
        {
            var path = Path.Combine(root, "blank.md");
            await File.WriteAllBytesAsync(path, []);
            var console = InteractiveConsole();
            console.Input.PushKey(ConsoleKey.Enter);
            await new MarkdownFilePreviewDemo().RunAsync(console, root, CancellationToken.None);
            Assert.Contains("Markdown file is empty", console.Output);
            Assert.Empty(await File.ReadAllBytesAsync(path));

            var cancelled = InteractiveConsole();
            cancelled.Input.PushKey(ConsoleKey.Escape);
            await new MarkdownFilePreviewDemo().RunAsync(cancelled, root, CancellationToken.None);
            Assert.Contains("Markdown selection cancelled", cancelled.Output);
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    private static string NewTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "ShowcaseMarkdown-" +
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
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
