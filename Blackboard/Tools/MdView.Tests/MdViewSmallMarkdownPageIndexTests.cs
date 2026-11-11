using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>Source-line page budgets and safe parser-backed code-fence boundaries.</summary>
public sealed class MdViewSmallMarkdownPageIndexTests
{
    [Fact]
    public void PagesStayWithinPhysicalLineBudgetAndPreserveSourceOrder()
    {
        var source = string.Join("\n",
            Enumerable.Range(1, 70).Select(number => "line " + number));
        var index = new MdViewSmallMarkdownPageIndex(source);
        var first = index.GetPage(0);
        var second = index.GetPage(first.NextLine);
        var last = index.GetPage(second.NextLine);

        Assert.Equal(70, index.LineCount);
        Assert.Equal((0, 24), (first.FirstLine, first.NextLine));
        Assert.Equal((24, 48), (second.FirstLine, second.NextLine));
        Assert.Equal((48, 70), (last.FirstLine, last.NextLine));
        Assert.True(first.HasMore);
        Assert.True(second.HasMore);
        Assert.False(last.HasMore);
        Assert.Contains("line 25", string.Join("", second.Segments.Select(x => x.Text)));
        Assert.DoesNotContain("line 49", string.Join("", second.Segments.Select(x => x.Text)));
    }

    [Fact]
    public void FenceContentStaysLiteralWhenPageStartsInsideCode()
    {
        const string markdown = """
            # Intro
            ```c
            int main(void)
            {
            # fake heading
            - fake list
            }
            ```
            # Real heading
            """;
        var index = new MdViewSmallMarkdownPageIndex(markdown);
        var page = index.GetPage(4, 2);

        Assert.Equal(2, page.Segments.Count);
        Assert.All(page.Segments, block => Assert.True(block.IsLiteralCode));
        Assert.Contains("# fake heading", page.Segments[0].Text);
        Assert.Contains("- fake list", page.Segments[1].Text);

        var console = new TestConsole();
        MdViewSmallMarkdownPageIndex.RenderPage(
            console, page, new MarkdownSpectreTheme());
        Assert.Contains("# fake heading", console.Output);
        Assert.Contains("- fake list", console.Output);
    }

    [Fact]
    public void HeadingSourceOffsetMapsToItsPhysicalLine()
    {
        const string markdown = "# Beginning\nOne\nČeský text\n## Target\nEnd";
        var index = new MdViewSmallMarkdownPageIndex(markdown);

        Assert.Equal(3, index.FindSourceLine(
            markdown.IndexOf("## Target", StringComparison.Ordinal)));
        Assert.Equal(2, index.FindSourceLine(
            markdown.IndexOf("Český", StringComparison.Ordinal)));
        Assert.Equal(0, index.FindSourceLine(0));
    }

    [Fact]
    public void OverlongPhysicalLineIsShortenedWithoutUnboundedConsoleOutput()
    {
        var source = new string('x', MdViewSmallMarkdownPageIndex.MaximumPageCharacters + 5000)
            + "\n# After";
        var page = new MdViewSmallMarkdownPageIndex(source).GetPage(0);

        Assert.True(page.TruncatedLine);
        Assert.True(page.HasMore);
        Assert.Equal(1, page.NextLine);
        Assert.True(page.Segments.Sum(x => x.Text.Length)
            <= MdViewSmallMarkdownPageIndex.MaximumPageCharacters);
        Assert.DoesNotContain("After", string.Concat(page.Segments.Select(x => x.Text)));
    }

    [Fact]
    public void PageCharacterBudgetDoesNotSplitUnicodeSurrogatePair()
    {
        var source = new string('a', MdViewSmallMarkdownPageIndex.MaximumPageCharacters - 1)
            + "😀" + new string('z', 400) + "\n# Next";
        var page = new MdViewSmallMarkdownPageIndex(source).GetPage(0);

        Assert.True(page.TruncatedLine);
        Assert.Single(page.Segments);
        var shown = page.Segments[0].Text;
        Assert.Equal(MdViewSmallMarkdownPageIndex.MaximumPageCharacters - 1,
            shown.Length);
        Assert.False(char.IsHighSurrogate(shown[^1]));
    }

    [Fact]
    public void RejectsInvalidPageRequestsAndDocumentOverBudget()
    {
        var index = new MdViewSmallMarkdownPageIndex("# Safe\nContent");
        Assert.Throws<ArgumentOutOfRangeException>(() => index.GetPage(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => index.GetPage(3));
        Assert.Throws<ArgumentOutOfRangeException>(() => index.GetPage(0, 129));
        Assert.Throws<ArgumentOutOfRangeException>(() => index.FindSourceLine(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MdViewSmallMarkdownPageIndex(
                new string('x', MdViewDocumentLinkExtractor.MaximumDocumentCharacters + 1)));
    }
}
