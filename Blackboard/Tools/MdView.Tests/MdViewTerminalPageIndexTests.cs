using System.Text;
using Afrowave.Toolbox.Blackboard.TextFiles;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Spectre.Console.Testing;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>Viewport-index paging, resize, source offsets and code preservation.</summary>
public sealed class MdViewTerminalPageIndexTests
{
    [Fact]
    public async Task NarrowTerminalProducesMorePagesAndRebuildDoesNotChangeSource()
    {
        using var fixture = new Fixture(string.Join("\n",
            Enumerable.Range(0, 60).Select(i =>
                "Chapter " + i + ". A complete sentence, with some ordinary words.")));
        var original = await File.ReadAllBytesAsync(fixture.Path);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var narrow = await MdViewTerminalPageIndex.CreateAsync(
            snapshot, width: 36, height: 24);
        await using var wide = await MdViewTerminalPageIndex.CreateAsync(
            snapshot, width: 120, height: 40);
        await using var tall = await MdViewTerminalPageIndex.CreateAsync(
            snapshot, width: 120, height: 80);
        await using var explicitCap = await MdViewTerminalPageIndex.CreateAsync(
            snapshot, width: 120, height: 80, maximumSourceLinesPerPage: 2);

        Assert.Equal(34, narrow.Columns);
        Assert.Equal(8, narrow.RowsPerPage);
        Assert.Equal(24, wide.RowsPerPage);
        Assert.Equal(64, tall.RowsPerPage);
        Assert.Equal(2, explicitCap.RowsPerPage);
        Assert.True(narrow.PageCount > wide.PageCount);
        Assert.True(wide.PageCount > tall.PageCount);
        Assert.True(explicitCap.PageCount > tall.PageCount);
        Assert.Equal(new MdViewVisualPosition(0, 0),
            await narrow.GetStartAsync(0));
        Assert.Equal(original, await File.ReadAllBytesAsync(fixture.Path));
    }

    [Fact]
    public async Task OneVeryLongPhysicalLineSpansMultiplePagesWithoutOmission()
    {
        using var fixture = new Fixture(new string('a', 160) + "\n# Ending");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var index = await MdViewTerminalPageIndex.CreateAsync(
            snapshot, width: 20, height: 18);
        Assert.Equal(2, index.RowsPerPage);
        Assert.True(index.PageCount > 3);

        var secondStart = await index.GetStartAsync(1);
        Assert.Equal(0, secondStart.Line);
        Assert.Equal(2, secondStart.WrapRow);
        Assert.Equal(1, await index.FindPageAsync(secondStart));

        var console = new TestConsole();
        console.Profile.Width = 20;
        console.Profile.Height = 18;
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var first = await MdViewTerminalPageRenderer.RenderAsync(
            console, cursor, await index.GetStartAsync(0), index.Columns,
            index.RowsPerPage, new MarkdownSpectreTheme());
        Assert.Equal(secondStart, first.Next);
        Assert.Equal(2, first.Rows);
        Assert.DoesNotContain("# Ending", console.Output);
    }

    [Fact]
    public async Task CodeLineClippedWithArrowIsOneVisualRow()
    {
        using var fixture = new Fixture(
            "\u0060\u0060\u0060c\n    int extremely_long_identifier = 123456789;\n\u0060\u0060\u0060\n# Ending");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var index = await MdViewTerminalPageIndex.CreateAsync(
            snapshot, width: 24, height: 36);
        Assert.Equal(1, index.PageCount);

        var console = new TestConsole();
        console.Profile.Width = 24;
        console.Profile.Height = 36;
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var page = await MdViewTerminalPageRenderer.RenderAsync(
            console, cursor, await index.GetStartAsync(0), index.Columns,
            index.RowsPerPage, new MarkdownSpectreTheme());

        Assert.True(page.ContainsClippedCode);
        Assert.Contains("→", console.Output);
        Assert.Contains("# Ending", console.Output);
    }

    [Fact]
    public async Task FourSpaceIndentedCodeIsNotSoftWrapped()
    {
        using var fixture = new Fixture(
            "    int very_long_identifier_with_spaces = value + another_value;\nNormal prose");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        await using var index = await MdViewTerminalPageIndex.CreateAsync(
            snapshot, width: 24, height: 36);
        Assert.Equal(1, index.PageCount);

        var console = new TestConsole();
        console.Profile.Width = 24;
        console.Profile.Height = 36;
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var rendered = await MdViewTerminalPageRenderer.RenderAsync(
            console, cursor, await index.GetStartAsync(0),
            index.Columns, index.RowsPerPage, new MarkdownSpectreTheme());

        Assert.True(rendered.ContainsClippedCode);
        Assert.Contains("→", console.Output);
        Assert.Contains("Normal prose", console.Output);
    }

    [Fact]
    public async Task DiskBudgetRejectsIndexWithoutChangingValidatedSnapshot()
    {
        using var fixture = new Fixture(string.Join("\n",
            Enumerable.Range(0, 50).Select(i => "line " + i)));
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            MdViewTerminalPageIndex.CreateAsync(snapshot, 20, 18,
                maximumIndexBytes: 12));
        Assert.Contains("line 0",
            (await snapshot.Lines.ReadPageAsync(0, 1)).Lines[0]);
    }

    [Fact]
    public async Task RejectsOutOfBoundsPositionsAndReleasedIndex()
    {
        using var fixture = new Fixture("one\ntwo\nthree");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(fixture.Path);
        var index = await MdViewTerminalPageIndex.CreateAsync(
            snapshot, width: 80, height: 30);
        try
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                index.GetStartAsync(-1));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                index.GetStartAsync(index.PageCount));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                index.FindPageAsync(new MdViewVisualPosition(-1, 0)));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                index.FindPageAsync(new MdViewVisualPosition(0, -1)));
        }
        finally
        {
            await index.DisposeAsync();
        }
        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            index.GetStartAsync(0));
    }

    private sealed class Fixture : IDisposable
    {
        private readonly string _directory = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "mdview-responsive-" + Guid.NewGuid().ToString("N"));

        public Fixture(string text)
        {
            Directory.CreateDirectory(_directory);
            Path = System.IO.Path.Combine(_directory, "source.md");
            File.WriteAllText(Path, text, new UTF8Encoding(false));
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(_directory))
                Directory.Delete(_directory, recursive: true);
        }
    }
}
