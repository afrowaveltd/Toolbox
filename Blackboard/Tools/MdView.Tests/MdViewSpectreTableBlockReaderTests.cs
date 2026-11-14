using System.Text;
using Afrowave.Toolbox.Blackboard.TextFiles;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>
/// Source-line and chunk-boundary regressions for a future native
/// Spectre table paginator. Uses only validated local disk snapshots.
/// </summary>
public sealed class MdViewSpectreTableBlockReaderTests
{
    [Fact]
    public async Task RecognizesHeaderAtVisiblePageEndWithDelimiterOnNextPage()
    {
        using var source = new SourceFixture(
            "Before\n| Name | Amount |\n| :--- | ---: |\n" +
            "| Apple | 10 |\n| Pear | 20 |\nAfter");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(source.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);

        var header = await MdViewSpectreTableBlockReader.FindFirstHeaderAsync(
            cursor, firstLine: 0, lastLineExclusive: 2);

        Assert.NotNull(header);
        Assert.Equal(1, header.SourceLine);
        Assert.Equal(new[] { "Name", "Amount" }, header.Columns);
        Assert.Equal(new[] { ":---", "---:" }, header.Separators);

        var rows = await MdViewSpectreTableBlockReader.ReadChunkAsync(
            cursor, header, firstDataLine: 3);
        Assert.Equal(2, rows.Rows.Count);
        Assert.Equal(3, rows.FirstDataLine);
        Assert.Equal(5, rows.NextDataLine);
        Assert.False(rows.HasMore);
        Assert.Equal(new[] { "Apple", "10" }, rows.Rows[0]);
        Assert.Equal(new[] { "Pear", "20" }, rows.Rows[1]);
    }

    [Fact]
    public async Task SixRowChunksResumeWithoutDroppingOrRepeatingSourceLines()
    {
        var markdown = "| Name | ID |\n| --- | ---: |\n" +
            string.Join("\n", Enumerable.Range(1, 14)
                .Select(i => "| Item " + i + " | " + i + " |"));
        using var source = new SourceFixture(markdown);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(source.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 0);
        Assert.NotNull(header);

        var first = await MdViewSpectreTableBlockReader.ReadChunkAsync(
            cursor, header, 2);
        var second = await MdViewSpectreTableBlockReader.ReadChunkAsync(
            cursor, header, first.NextDataLine);
        var third = await MdViewSpectreTableBlockReader.ReadChunkAsync(
            cursor, header, second.NextDataLine);

        Assert.Equal(6, first.Rows.Count);
        Assert.Equal(6, second.Rows.Count);
        Assert.Equal(2, third.Rows.Count);
        Assert.True(first.HasMore);
        Assert.True(second.HasMore);
        Assert.False(third.HasMore);
        Assert.Equal(8, first.NextDataLine);
        Assert.Equal(14, second.NextDataLine);
        Assert.Equal(16, third.NextDataLine);

        var values = first.Rows.Concat(second.Rows).Concat(third.Rows)
            .Select(row => row[1]).ToArray();
        Assert.Equal(Enumerable.Range(1, 14).Select(i => i.ToString()), values);
    }

    [Fact]
    public async Task ANonTableLineTerminatesTheBlockWithoutSkippingIt()
    {
        using var source = new SourceFixture(
            "| Name | Number |\n| --- | --- |\n" +
            "| First | 1 |\nNo table here\n| Later | 2 |");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(source.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 0);
        Assert.NotNull(header);

        var chunk = await MdViewSpectreTableBlockReader.ReadChunkAsync(
            cursor, header, 2);

        Assert.Single(chunk.Rows);
        Assert.Equal(3, chunk.NextDataLine);
        Assert.False(chunk.HasMore);
        Assert.Equal("No table here",
            (await cursor.ReadAsync(chunk.NextDataLine, 1))[0].Source.Text);
    }

    [Fact]
    public async Task EscapedAndInlineCodePipesRemainInsideTheirOriginalCells()
    {
        const string markdown =
            "| Label | Code |\n| :--- | ---: |\n" +
            "| first \\| second | `a|b` |";
        using var source = new SourceFixture(markdown);
        var before = await File.ReadAllBytesAsync(source.Path);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(source.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 0);
        Assert.NotNull(header);

        var chunk = await MdViewSpectreTableBlockReader.ReadChunkAsync(
            cursor, header, 2);
        Assert.Single(chunk.Rows);
        Assert.Equal("first | second", chunk.Rows[0][0]);
        Assert.Equal("`a|b`", chunk.Rows[0][1]);
        Assert.Equal(before, await File.ReadAllBytesAsync(source.Path));
    }

    [Fact]
    public async Task FencesQuotesAndMismatchedDelimitersNeverFormHeaders()
    {
        using var source = new SourceFixture(
            "```md\n| A | B |\n| --- | --- |\n```\n" +
            "> | Quoted | Header |\n> | --- | --- |\n" +
            "| Wrong | Columns |\n| --- | --- | --- |\nPlain");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(source.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);

        Assert.Null(await MdViewSpectreTableBlockReader.FindFirstHeaderAsync(
            cursor, 0, 9));
        Assert.Null(await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 1));
        Assert.Null(await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 4));
        Assert.Null(await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 6));
    }

    [Fact]
    public async Task InvalidLineRangesChunkBudgetsAndResumePointsAreRejected()
    {
        using var source = new SourceFixture(
            "| Name | Value |\n| --- | --- |\n| One | Two |");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(source.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        var header = await MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, 0);
        Assert.NotNull(header);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            MdViewSpectreTableBlockReader.ReadHeaderAsync(cursor, -1));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            MdViewSpectreTableBlockReader.FindFirstHeaderAsync(cursor, 2, 1));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            MdViewSpectreTableBlockReader.ReadChunkAsync(cursor, header, 1));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            MdViewSpectreTableBlockReader.ReadChunkAsync(cursor, header, 2, 0));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            MdViewSpectreTableBlockReader.ReadChunkAsync(cursor, header, 2, 7));

        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            MdViewSpectreTableBlockReader.ReadHeaderAsync(
                cursor, 0, cancelled.Token));
    }

    private sealed class SourceFixture : IDisposable
    {
        private readonly string _directory = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "mdview-table-block-" + Guid.NewGuid().ToString("N"));

        public SourceFixture(string source)
        {
            Directory.CreateDirectory(_directory);
            Path = System.IO.Path.Combine(_directory, "document.md");
            File.WriteAllText(Path, source, new UTF8Encoding(false));
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(_directory))
                Directory.Delete(_directory, recursive: true);
        }
    }
}
