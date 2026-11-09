using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>Regression checks for opt-in Markdown heading anchor navigation.</summary>
public sealed class MdViewHeadingIndexTests
{
    [Fact]
    public void HeadingIndexKeepsDuplicateSlugsUniqueAndExcludesCode()
    {
        const string markdown = """
            # Installation
            Description.
            ```markdown
            # Installation
            ```
            ## Installation
            # Česky a dál
            """;
        var headings = MdViewHeadingIndex.Create(markdown);
        Assert.Equal(3, headings.Count);
        Assert.Equal("installation", headings[0].Anchor);
        Assert.Equal("installation-1", headings[1].Anchor);
        Assert.Equal("česky-a-dál", headings[2].Anchor);
        Assert.Equal(markdown.LastIndexOf("## Installation", StringComparison.Ordinal),
            headings[1].SourceOffset);
        Assert.Null(MdViewHeadingIndex.Find(headings, "missing"));
    }

    [Fact]
    public void HeadingIndexRejectsOversizeDocuments()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MdViewHeadingIndex.Create(
                new string('x', MdViewDocumentLinkExtractor.MaximumDocumentCharacters + 1)));
    }

    [Fact]
    public async Task FragmentOnlyLinkStartsRenderingAtTargetHeading()
    {
        using var docs = new Docs();
        var home = docs.Write("README.md", """
            # Overview
            Before the jump.
            [Go to installation](#installation)
            ## Installation
            After the jump.
            """);
        var terminal = Interactive();
        terminal.Input.PushKey(ConsoleKey.Enter); // Follow
        terminal.Input.PushKey(ConsoleKey.Enter); // Choose anchor
        terminal.Input.PushKey(ConsoleKey.DownArrow); // Back from anchor
        terminal.Input.PushKey(ConsoleKey.Enter);
        terminal.Input.PushKey(ConsoleKey.DownArrow); // Exit original
        terminal.Input.PushKey(ConsoleKey.Enter);

        await new MdViewLocalLinkBrowser(terminal).RunAsync(home);
        Assert.Contains("Jumped to heading: Installation", terminal.Output);
        Assert.Contains("After the jump.", terminal.Output);
        Assert.Contains("Overview", terminal.Output);
    }

    [Fact]
    public async Task MissingHeadingShowsFeedbackWithoutCrashing()
    {
        using var docs = new Docs();
        var home = docs.Write("README.md", """
            # Home
            [Missing anchor](#does-not-exist)
            """);
        var terminal = Interactive();
        terminal.Input.PushKey(ConsoleKey.Enter);
        terminal.Input.PushKey(ConsoleKey.Enter);
        terminal.Input.PushKey(ConsoleKey.DownArrow);
        terminal.Input.PushKey(ConsoleKey.Enter);

        await new MdViewLocalLinkBrowser(terminal).RunAsync(home);
        Assert.Contains("Heading anchor not found: #does-not-exist", terminal.Output);
    }

    private static TestConsole Interactive()
    {
        var console = new TestConsole();
        console.Profile.Capabilities.Interactive = true;
        console.Profile.Capabilities.Unicode = true;
        console.Profile.Width = 120;
        return console;
    }

    private sealed class Docs : IDisposable
    {
        public string Root { get; } = Path.Combine(
            Path.GetTempPath(), "mdview-heading-test-" + Guid.NewGuid().ToString("N"));

        public Docs() => Directory.CreateDirectory(Root);

        public string Write(string name, string text)
        {
            var file = Path.Combine(Root, name);
            File.WriteAllText(file, text);
            return file;
        }

        public void Dispose() => Directory.Delete(Root, recursive: true);
    }
}
