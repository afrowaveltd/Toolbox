using System.Text;
using Afrowave.Toolbox.Blackboard.TextFiles;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Afrowave.Toolbox.ExchangeHouse.Markdown;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>Disk-journal regression coverage for scanner-backed preview navigation.</summary>
public sealed class MdViewContainerPageCursorTests
{
    [Fact]
    public async Task BackwardReadUsesJournalWithoutReplayingClassifiedLines()
    {
        using var file = new TestFile();
        var path = await file.WriteAsync("intro\n```text\n# literal\n- literal\n```\n# heading");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);

        var first = await cursor.ReadAsync(0, 2);
        var second = await cursor.ReadAsync(2, 2);
        Assert.Equal(MarkdownContainerLineKind.CodeLine, second[0].Kind);
        Assert.Equal(MarkdownContainerLineKind.CodeLine, second[1].Kind);
        Assert.Equal(4, cursor.CachedLineCount);
        var journalBytes = cursor.JournalBytes;

        var repeat = await cursor.ReadAsync(2, 2);
        var backward = await cursor.ReadAsync(0, 2);

        Assert.Equal(journalBytes, cursor.JournalBytes);
        Assert.Equal(4, cursor.CachedLineCount);
        Assert.Equal(first, backward);
        Assert.Equal(second, repeat);
    }

    [Fact]
    public async Task ForwardAfterBackwardKeepsOriginalFenceState()
    {
        using var file = new TestFile();
        var path = await file.WriteAsync(
            "start\n```csharp\n# code\n- code\n```\n# real heading");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);

        var middle = await cursor.ReadAsync(2, 2);
        Assert.Equal(4, cursor.CachedLineCount);
        Assert.All(middle, x => Assert.Equal(MarkdownContainerLineKind.CodeLine, x.Kind));

        await cursor.ReadAsync(0, 2);
        var last = await cursor.ReadAsync(4, 2);
        Assert.Equal(MarkdownContainerLineKind.FenceClose, last[0].Kind);
        Assert.Equal(MarkdownContainerLineKind.Plain, last[1].Kind);
        Assert.Equal(6, cursor.CachedLineCount);
    }

    [Fact]
    public async Task JournalBudgetExhaustionFailsClosed()
    {
        using var file = new TestFile();
        var path = await file.WriteAsync("# Heading\nsecond");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(path);
        await using var cursor = new MdViewContainerPageCursor(snapshot,
            maximumJournalBytes: 1);

        await Assert.ThrowsAsync<InvalidDataException>(() => cursor.ReadAsync(0, 2));
        await Assert.ThrowsAsync<InvalidOperationException>(() => cursor.ReadAsync(0, 1));
    }

    [Fact]
    public async Task OffsetBudgetRejectsFurtherLinesButKeepsBoundedDiskUsage()
    {
        using var file = new TestFile();
        var path = await file.WriteAsync("alpha\nbeta\ngamma");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(path);
        await using var cursor = new MdViewContainerPageCursor(snapshot,
            maximumOffsetBytes: sizeof(long));

        Assert.Single(await cursor.ReadAsync(0, 1));
        Assert.Equal(1, cursor.CachedLineCount);
        await Assert.ThrowsAsync<InvalidDataException>(() => cursor.ReadAsync(1, 1));
    }

    [Fact]
    public async Task EmptySnapshotAndInvalidPageArgumentsAreHandled()
    {
        using var file = new TestFile();
        var path = await file.WriteAsync("");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);

        Assert.Empty(await cursor.ReadAsync(0, 24));
        Assert.Equal(0, cursor.CachedLineCount);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => cursor.ReadAsync(-1, 2));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => cursor.ReadAsync(0, 129));
    }

    [Fact]
    public async Task CancellingAnEarlierCompletedRequestDoesNotCancelForwardNavigation()
    {
        using var file = new TestFile();
        var path = await file.WriteAsync(
            "intro\n```text\n# literal\n```\n# heading");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        using var firstRequest = new CancellationTokenSource();

        Assert.Equal(2, (await cursor.ReadAsync(
            0, 2, firstRequest.Token)).Count);

        // The token belongs to the completed call, not the persistent scanner.
        firstRequest.Cancel();
        var next = await cursor.ReadAsync(2, 3);
        Assert.Equal(3, next.Count);
        Assert.Equal(MarkdownContainerLineKind.CodeLine, next[0].Kind);
        Assert.Equal(MarkdownContainerLineKind.FenceClose, next[1].Kind);
        Assert.Equal(MarkdownContainerLineKind.Plain, next[2].Kind);
    }

    [Fact]
    public async Task PreCancelledRequestDoesNotPoisonAWorkingCursor()
    {
        using var file = new TestFile();
        var path = await file.WriteAsync("one\ntwo\nthree");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        Assert.Single(await cursor.ReadAsync(0, 1));

        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            cursor.ReadAsync(1, 1, cancelled.Token));

        var next = await cursor.ReadAsync(1, 2);
        Assert.Equal(new[] { "two", "three" },
            next.Select(x => x.Source.Text));
        Assert.Equal(3, cursor.CachedLineCount);
    }

    [Fact]
    public async Task CheckpointReaderResumesFenceAcrossIndependentPageReaders()
    {
        using var file = new TestFile();
        var path = await file.WriteAsync("intro\n```cs\n# literal\n- literal\n```\n# heading");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(path);
        await using var offsets = await Utf8DocumentOffsetIndex.CreateAsync(snapshot.Lines);
        var reader = new MdViewCheckpointPageReader(snapshot, offsets);

        var first = await reader.ReadAsync(reader.Start, 2);
        Assert.Equal(2, first.Next.NextLineNumber);
        var middle = await reader.ReadAsync(first.Next, 2);
        Assert.All(middle.Lines,
            x => Assert.Equal(MarkdownContainerLineKind.CodeLine, x.Kind));
        var last = await reader.ReadAsync(middle.Next, 2);
        Assert.Equal(MarkdownContainerLineKind.FenceClose, last.Lines[0].Kind);
        Assert.Equal(MarkdownContainerLineKind.Plain, last.Lines[1].Kind);

        // Reusing a checkpoint must be repeatable, not consume a shared iterator.
        var repeated = await reader.ReadAsync(first.Next, 2);
        Assert.Equal(middle.Lines.Select(x => x.Kind),
            repeated.Lines.Select(x => x.Kind));
        Assert.Equal(middle.Next.NextUtf16Offset, repeated.Next.NextUtf16Offset);
    }

    [Fact]
    public async Task CheckpointReaderHandlesBomMixedTerminatorsAndEndOfFile()
    {
        using var file = new TestFile();
        var path = await file.WriteAsync("Č 🌍\r\n> quote\r- item\nlast");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(path);
        await using var offsets = await Utf8DocumentOffsetIndex.CreateAsync(snapshot.Lines);
        var reader = new MdViewCheckpointPageReader(snapshot, offsets);
        var first = await reader.ReadAsync(reader.Start, 1);
        var second = await reader.ReadAsync(first.Next, 3);

        Assert.Equal("Č 🌍", first.Lines[0].Source.Text);
        Assert.Equal("\r\n", first.Lines[0].Source.Terminator);
        Assert.Equal(new[] { "> quote", "- item", "last" },
            second.Lines.Select(x => x.Source.Text));
        Assert.Equal(offsets.Utf16Length, second.Next.NextUtf16Offset);
        Assert.Empty((await reader.ReadAsync(second.Next, 3)).Lines);
    }

    [Fact]
    public async Task CheckpointCannotBeTransferredBetweenSnapshotReaders()
    {
        using var file = new TestFile();
        var path = await file.WriteAsync("alpha\nbeta");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(path);
        await using var offsets = await Utf8DocumentOffsetIndex.CreateAsync(snapshot.Lines);
        var first = new MdViewCheckpointPageReader(snapshot, offsets);
        var other = new MdViewCheckpointPageReader(snapshot, offsets);
        var checkpoint = (await first.ReadAsync(first.Start, 1)).Next;

        await Assert.ThrowsAsync<ArgumentException>(() =>
            other.ReadAsync(checkpoint, 1));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            first.ReadAsync(first.Start, 129));
    }

    private sealed class TestFile : IDisposable
    {
        private readonly string _dir = Path.Combine(
            Path.GetTempPath(), "afrowave-journal-test-" + Guid.NewGuid().ToString("N"));
        public TestFile() => Directory.CreateDirectory(_dir);

        public async Task<string> WriteAsync(string content)
        {
            var path = Path.Combine(_dir, "README.md");
            await File.WriteAllTextAsync(path, content, new UTF8Encoding(false));
            return path;
        }

        public void Dispose() => Directory.Delete(_dir, recursive: true);
    }
}
