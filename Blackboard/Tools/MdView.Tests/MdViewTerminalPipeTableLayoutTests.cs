using System.Text;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Afrowave.Toolbox.Blackboard.TextFiles;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Afrowave.Toolbox.ExchangeHouse.Markdown;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>Bounded pipe table presentation and source-page parity regressions.</summary>
public sealed class MdViewTerminalPipeTableLayoutTests
{
    [Fact]
    public async Task FramedHeaderRowsBecomeNumberedCompactCells()
    {
        var line = Assert.Single(await ScanAsync("| Name | Description |"));
        var rows = MdViewTerminalLineLayout.Layout(line, 24);

        Assert.Equal(2, rows.Count);
        Assert.Equal("1 │ Name", rows[0].Text);
        Assert.Equal("2 │ Description", rows[1].Text);
        Assert.All(rows, row => Assert.True(
            MdViewTerminalWordWrap.MeasureColumns(row.Text) <= 24));
    }

    [Fact]
    public async Task TableSeparatorUsesSingleVisualRow()
    {
        var line = Assert.Single(await ScanAsync("| :--- | ---: |"));
        var rows = MdViewTerminalLineLayout.Layout(line, 24);

        Assert.Single(rows);
        Assert.Equal(new string('─', 24), rows[0].Text);
        Assert.All(rows[0].Spans, span =>
            Assert.Equal(MdViewTerminalSpanRole.Marker, span.Role));
    }

    [Fact]
    public async Task EscapedAndInlineCodePipesAreNotColumnBoundaries()
    {
        const string source =
            @"| first\|part | " + "\u0060" + "code|sample" + "\u0060" + " | end |";
        var line = Assert.Single(await ScanAsync(source));
        var rows = MdViewTerminalLineLayout.Layout(line, 60);

        Assert.Equal(3, rows.Count);
        Assert.Contains("first|part", rows[0].Text);
        Assert.Contains("code|sample", rows[1].Text);
        Assert.Contains(rows[1].Spans, span =>
            span.Role == MdViewTerminalSpanRole.Code &&
            span.Text.Contains("code|sample", StringComparison.Ordinal));
        Assert.Equal("3 │ end", rows[2].Text);
    }

    [Fact]
    public async Task EscapedPipeInsideInlineCodeRemainsLiteralSource()
    {
        var line = Assert.Single(await ScanAsync(
            "| \u0060literal\\|pipe\u0060 | escaped\\|pipe |"));
        var rows = MdViewTerminalLineLayout.Layout(line, 50);

        Assert.Equal(2, rows.Count);
        Assert.Contains(@"literal\|pipe", rows[0].Text);
        Assert.Contains("escaped|pipe", rows[1].Text);
        Assert.Contains(rows[0].Spans, span =>
            span.Role == MdViewTerminalSpanRole.Code);
    }

    [Fact]
    public async Task LinkInTableCellRetainsStyleAndHidesDestination()
    {
        var line = Assert.Single(await ScanAsync(
            "| [Installation instructions](Docs/Install/en.md) | OK |"));
        var rows = MdViewTerminalLineLayout.Layout(line, 19);

        Assert.True(rows.Count >= 3);
        Assert.All(rows, row => Assert.True(
            MdViewTerminalWordWrap.MeasureColumns(row.Text) <= 19));
        Assert.Contains(rows.SelectMany(row => row.Spans),
            span => span.Role == MdViewTerminalSpanRole.Link);
        Assert.DoesNotContain(rows, row =>
            row.Text.Contains("Docs/Install", StringComparison.Ordinal));
        Assert.Contains(rows, row => row.Text.StartsWith("2 │ OK",
            StringComparison.Ordinal));
    }

    [Fact]
    public async Task UnframedOrMalformedPipeTextRemainsOrdinaryMarkdown()
    {
        foreach (var text in new[]
        {
            "A | B | C",
            "| single |",
            "| malformed | another \\|",
            "| \u0060unterminated code | second |"
        })
        {
            var line = Assert.Single(await ScanAsync(text));
            Assert.False(MdViewTerminalPipeTableLayout.TryLayout(
                line, 80, out _));
        }
    }

    [Fact]
    public async Task CodeFencePipeRowsStayLiteral()
    {
        var lines = await ScanAsync(
            "\u0060\u0060\u0060md\n| this | is not a table |\n\u0060\u0060\u0060");
        Assert.Equal(3, lines.Count);
        var rows = MdViewTerminalLineLayout.Layout(lines[1], 40);

        Assert.Single(rows);
        Assert.Equal("| this | is not a table |", rows[0].Text);
        Assert.Equal(MdViewTerminalSpanRole.Code, rows[0].Spans[0].Role);
    }

    [Fact]
    public async Task ExtraNarrowViewportDoesNotOverflowCells()
    {
        var line = Assert.Single(await ScanAsync(
            "| Alpha | Some considerably longer text with multiple words |"));
        var rows = MdViewTerminalLineLayout.Layout(line, 8);

        Assert.True(rows.Count > 4);
        Assert.All(rows, row =>
            Assert.True(MdViewTerminalWordWrap.MeasureColumns(row.Text) <= 8));
        Assert.Contains(rows, row => row.Text.Contains("Alpha",
            StringComparison.Ordinal));
    }

    [Fact]
    public async Task TooManyColumnsFallBackWithoutHugeCellCatalog()
    {
        var source = "|" +
            string.Join("|", Enumerable.Repeat("x", 
                MdViewTerminalPipeTableLayout.MaximumColumns + 1)) + "|";
        var line = Assert.Single(await ScanAsync(source));

        Assert.False(MdViewTerminalPipeTableLayout.TryLayout(
            line, 80, out _));
    }

    [Fact]
    public async Task DiskPageBoundariesMatchRenderedCompactTableRows()
    {
        var markdown = "| Name | Notes |\n| --- | --- |\n" +
            "| First | [read more](Docs/en.md) with several extra words |\n" +
            "| Second | Important additional information goes here |\n# End";
        var folder = Path.Combine(Path.GetTempPath(),
            "mdview-table-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var path = Path.Combine(folder, "table.md");
            await File.WriteAllTextAsync(path, markdown, new UTF8Encoding(false));
            await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(path);
            await using var pages = await MdViewTerminalPageIndex.CreateAsync(
                snapshot, width: 24, height: 22, maximumSourceLinesPerPage: 2);
            await using var cursor = new MdViewContainerPageCursor(snapshot);
            var console = new TestConsole();
            console.Profile.Width = 24;
            console.Profile.Height = 22;

            Assert.True(pages.PageCount >= 5);
            for (var page = 0L; page < pages.PageCount; page++)
            {
                var start = await pages.GetStartAsync(page);
                var rendered = await MdViewTerminalPageRenderer.RenderAsync(
                    console, cursor, start, pages.Columns,
                    pages.RowsPerPage, new MarkdownSpectreTheme());
                if (page + 1 < pages.PageCount)
                    Assert.Equal(await pages.GetStartAsync(page + 1),
                        rendered.Next);
                else
                    Assert.Equal(snapshot.Lines.LineCount, rendered.Next.Line);
            }

            Assert.Contains("1 │ Name", console.Output);
            Assert.Contains("2 │ Notes", console.Output);
            Assert.Contains("First", console.Output);
            Assert.DoesNotContain("Docs/en.md", console.Output);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static async Task<IReadOnlyList<MarkdownContainerLine>> ScanAsync(
        string text)
    {
        using var reader = new StringReader(text);
        var result = new List<MarkdownContainerLine>();
        await foreach (var line in MarkdownIncrementalContainerScanner.ScanAsync(reader))
            result.Add(line);
        return result;
    }
}
