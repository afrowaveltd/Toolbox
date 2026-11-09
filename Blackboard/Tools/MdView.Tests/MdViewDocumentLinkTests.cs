using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>Regression coverage for opt-in parsed Markdown link selection.</summary>
public sealed class MdViewDocumentLinkTests
{
    [Fact]
    public void ExtractsNestedLinksAndPreservesSourceOffsets()
    {
        const string markdown = """
            # Welcome [install](Docs/Install/en.md)
            Read **[details](Docs/Detail/en.md#start)** here.
            """;
        var origin = MdViewDocumentOrigin.FromGitHub(
            "afrowaveltd", "Toolbox", "main", "README.md");
        var links = MdViewDocumentLinkExtractor.Extract(markdown, origin);

        Assert.Equal(2, links.Count);
        Assert.Equal("install", links[0].Label);
        Assert.Equal(markdown.IndexOf("[install]", StringComparison.Ordinal),
            links[0].SourceOffset);
        Assert.Equal("Docs/Install/en.md", links[0].Target.Origin.Location);
        Assert.Equal("start", links[1].Target.Fragment);
        Assert.Equal(markdown.IndexOf("[details]", StringComparison.Ordinal),
            links[1].SourceOffset);
    }

    [Fact]
    public void FencedCodeInlineCodeImagesAndUnsafeTargetsAreNotNavigable()
    {
        const string markdown = """
            [valid](Docs/en.md)
            `[inline](no.md)`
            ![picture](photo.png)
            ```markdown
            [fenced](secret.md)
            ```
            \[escaped](hidden.md)
            [external](https://outside.example/path.md)
            [traversal](../private.md)
            """;
        var root = Path.Combine(Path.GetTempPath(), "mdview-links-test");
        var origin = MdViewDocumentOrigin.FromLocal(
            Path.Combine(root, "README.md"), root);
        var links = MdViewDocumentLinkExtractor.Extract(markdown, origin);

        Assert.Single(links);
        Assert.Equal("valid", links[0].Label);
    }

    [Fact]
    public void DocumentSizeIsExplicitlyBounded()
    {
        var origin = MdViewDocumentOrigin.FromHttps(
            "https://docs.example.org/README.md");
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MdViewDocumentLinkExtractor.Extract(
                new string('a', MdViewDocumentLinkExtractor.MaximumDocumentCharacters + 1),
                origin));
    }

    [Fact]
    public async Task SelectorReturnsOnlyExplicitChoiceAndEscapeCancels()
    {
        var origin = MdViewDocumentOrigin.FromHttps(
            "https://docs.example.org/README.md");
        var links = MdViewDocumentLinkExtractor.Extract(
            "[First](one.md) and [Second](two.md)", origin);
        var console = Interactive();
        console.Input.PushKey(ConsoleKey.DownArrow);
        console.Input.PushKey(ConsoleKey.Enter);

        var chosen = await new MdViewDocumentLinkPicker(console).PickAsync(links);
        Assert.Equal("Second", chosen?.Label);

        var cancelled = Interactive();
        cancelled.Input.PushKey(ConsoleKey.Escape);
        Assert.Null(await new MdViewDocumentLinkPicker(cancelled).PickAsync(links));
    }

    [Fact]
    public async Task EmptyAndNoninteractivePickersDoNotOpenAnything()
    {
        var console = Interactive();
        Assert.Null(await new MdViewDocumentLinkPicker(console).PickAsync(
            Array.Empty<MdViewDocumentLink>()));

        var noninteractive = new TestConsole();
        noninteractive.Profile.Capabilities.Interactive = false;
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new MdViewDocumentLinkPicker(noninteractive).PickAsync(
                Array.Empty<MdViewDocumentLink>()));
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
