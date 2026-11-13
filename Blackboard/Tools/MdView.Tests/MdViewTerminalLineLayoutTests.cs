using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Afrowave.Toolbox.Blackboard.TextFiles;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Afrowave.Toolbox.ExchangeHouse.Markdown;
using Spectre.Console.Testing;
using System.Text;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>Common index/render layout for lists, quotes and wrapped inline content.</summary>
public sealed class MdViewTerminalLineLayoutTests
{
    [Fact]
    public async Task LongListItemUsesOneBulletAndHangingIndentAcrossRows()
    {
        var line = Assert.Single(await ScanAsync(
            "- This is an ordinary list item, with a link to read."));
        var rows = MdViewTerminalLineLayout.Layout(line, 22);

        Assert.True(rows.Count > 1);
        Assert.StartsWith("• ", rows[0].Text);
        Assert.All(rows.Skip(1), row => Assert.StartsWith("  ", row.Text));
        Assert.DoesNotContain(rows.Skip(1), row => row.Text.StartsWith("•"));
        Assert.All(rows, row =>
            Assert.True(MdViewTerminalWordWrap.MeasureColumns(row.Text) <= 22));
        Assert.Equal("This is an ordinary list item, with a link to read.",
            string.Join(" ", rows.Select(row => row.Text.Trim()
                .TrimStart('•').Trim())));
    }

    [Fact]
    public async Task QuotedProseKeepsQuoteBarOnEveryWrappedRow()
    {
        var line = Assert.Single(await ScanAsync(
            "> One rather long quotation, with many different words and clauses."));
        var rows = MdViewTerminalLineLayout.Layout(line, 24);

        Assert.True(rows.Count >= 3);
        Assert.All(rows, row => Assert.StartsWith("│ ", row.Text));
        Assert.DoesNotContain(rows, row => row.Text.StartsWith(">"));
        Assert.All(rows, row =>
            Assert.True(MdViewTerminalWordWrap.MeasureColumns(row.Text) <= 24));
        Assert.All(rows, row =>
            Assert.Equal(MdViewTerminalSpanRole.Marker, row.Spans[0].Role));
    }

    [Fact]
    public async Task LinkLabelKeepsLinkStyleAcrossSoftWrapButHidesDestination()
    {
        var line = Assert.Single(await ScanAsync(
            "[Read the detailed documentation of this component](Docs/Manual/en.md)"));
        var rows = MdViewTerminalLineLayout.Layout(line, 18);

        Assert.True(rows.Count >= 2);
        Assert.All(rows, row =>
            Assert.All(row.Spans, span =>
                Assert.Equal(MdViewTerminalSpanRole.Link, span.Role)));
        var visible = string.Join(" ", rows.Select(row => row.Text));
        Assert.Equal("Read the detailed documentation of this component", visible);
        Assert.DoesNotContain("Docs/Manual", visible);
    }

    [Fact]
    public async Task InlineEmphasisAndCodeKeepStylesAfterWrapping()
    {
        var line = Assert.Single(await ScanAsync(
            "**Strong language** and important \u0060inline code\u0060 are preserved."));
        var rows = MdViewTerminalLineLayout.Layout(line, 20);
        var spans = rows.SelectMany(row => row.Spans).ToArray();

        Assert.Contains(spans, span => span.Role == MdViewTerminalSpanRole.Strong);
        Assert.Contains(spans, span => span.Role == MdViewTerminalSpanRole.Code);
        Assert.DoesNotContain(rows, row => row.Text.Contains("**"));
        Assert.DoesNotContain(rows, row => row.Text.Contains("\u0060"));
        Assert.All(rows, row =>
            Assert.True(MdViewTerminalWordWrap.MeasureColumns(row.Text) <= 20));
    }

    [Fact]
    public async Task QuotedNumberedListCarriesBothPrefixesWithoutRepeatingNumber()
    {
        var line = Assert.Single(await ScanAsync(
            "> 12. A long numbered list item that continues across the terminal."));
        var rows = MdViewTerminalLineLayout.Layout(line, 26);

        Assert.True(rows.Count > 1);
        Assert.StartsWith("│ 12. ", rows[0].Text);
        Assert.All(rows.Skip(1), row => Assert.StartsWith("│     ", row.Text));
        Assert.DoesNotContain(rows.Skip(1), row => row.Text.Contains("12."));
    }

    [Fact]
    public async Task QuotedLinkKeepsQuotePrefixAndLinkColorOnEachWrappedRow()
    {
        var line = Assert.Single(await ScanAsync(
            "> [Read the full installation instructions](Docs/Installation/en.md)"));
        var rows = MdViewTerminalLineLayout.Layout(line, 20);

        Assert.True(rows.Count > 1);
        Assert.All(rows, row =>
        {
            Assert.StartsWith("│ ", row.Text);
            Assert.Contains(row.Spans, span =>
                span.Role == MdViewTerminalSpanRole.Link);
            Assert.True(MdViewTerminalWordWrap.MeasureColumns(row.Text) <= 20);
        });
        Assert.DoesNotContain(rows, row => row.Text.Contains("Docs/Installation"));
    }

    [Fact]
    public async Task MalformedAndEscapedMarkdownAreRenderedAsLiteralSafeText()
    {
        var line = Assert.Single(await ScanAsync(
            @"\[literal] and [not-a-link] plus [red]safe[/]"));
        var rows = MdViewTerminalLineLayout.Layout(line, 80);
        var text = string.Concat(rows.Select(row => row.Text));

        Assert.Contains("[literal]", text);
        Assert.Contains("[not-a-link]", text);
        Assert.Contains("[red]safe[/]", text);
        Assert.DoesNotContain(rows.SelectMany(row => row.Spans),
            span => span.Role == MdViewTerminalSpanRole.Link);
    }

    [Fact]
    public async Task LiteralCodeStillShowsSourceMarkersWithoutMarkdownParsing()
    {
        var lines = await ScanAsync(
            "intro\n\u0060\u0060\u0060c\n# this is C code, not a heading\n\u0060\u0060\u0060");
        var code = MdViewTerminalLineLayout.Layout(lines[2], 24);

        Assert.Single(code);
        Assert.True(code[0].IsClippedCode);
        Assert.StartsWith("# this is C code", code[0].Text);
        Assert.EndsWith("→", code[0].Text);
        Assert.Equal(MdViewTerminalSpanRole.Code, code[0].Spans[0].Role);
    }

    [Fact]
    public async Task VisualPageIndexAgreesWithRendererAcrossWrappedLinkAndList()
    {
        var source = "- [Detailed documentation for the system](Docs/en.md) and several extra words." +
            "\n> Some quoted text that wraps to several lines of the terminal." +
            "\n# Final heading";
        var root = Path.Combine(Path.GetTempPath(),
            "mdview-layout-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, "sample.md");
        try
        {
            await File.WriteAllTextAsync(path, source, new UTF8Encoding(false));
            await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(path);
            await using var pages = await MdViewTerminalPageIndex.CreateAsync(
                snapshot, width: 30, height: 30,
                maximumSourceLinesPerPage: 2);
            await using var cursor = new MdViewContainerPageCursor(snapshot);
            var console = new TestConsole();
            console.Profile.Width = 30;
            console.Profile.Height = 30;

            Assert.True(pages.PageCount >= 4);
            for (long number = 0; number < pages.PageCount; number++)
            {
                var start = await pages.GetStartAsync(number);
                var rendered = await MdViewTerminalPageRenderer.RenderAsync(
                    console, cursor, start, pages.Columns,
                    pages.RowsPerPage, new MarkdownSpectreTheme());
                if (number + 1 < pages.PageCount)
                    Assert.Equal(await pages.GetStartAsync(number + 1),
                        rendered.Next);
                else
                    Assert.Equal(snapshot.Lines.LineCount, rendered.Next.Line);
            }
            Assert.DoesNotContain("Docs/en.md", console.Output);
            Assert.Contains("Detailed documentation", console.Output);
            Assert.Contains("│ ", console.Output);
            Assert.Contains("• ", console.Output);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static async Task<IReadOnlyList<MarkdownContainerLine>> ScanAsync(
        string source)
    {
        using var reader = new StringReader(source);
        var result = new List<MarkdownContainerLine>();
        await foreach (var line in MarkdownIncrementalContainerScanner.ScanAsync(reader))
            result.Add(line);
        return result;
    }
}
