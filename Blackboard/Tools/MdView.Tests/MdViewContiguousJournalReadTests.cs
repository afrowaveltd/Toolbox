using System.Reflection;
using System.Text;
using Afrowave.Toolbox.Blackboard.TextFiles;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Xunit;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView.Tests;

/// <summary>
/// Exercises bounded contiguous event-journal reads, source line integrity,
/// oversized individual records and corruption rejection.
/// </summary>
public sealed class MdViewContiguousJournalReadTests
{
    [Fact]
    public async Task SmallSourceWindowReadsManyEventsInOneStreamOperation()
    {
        var original = Enumerable.Range(0, 20)
            .Select(i => "Source line " + i).ToArray();
        using var fixture = new Fixture(string.Join("\n", original));
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(
            fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);

        var source = await cursor.ReadAsync(0, original.Length);
        Assert.Equal(original, source.Select(item => item.Source.Text));
        Assert.Equal(1, cursor.ReadRequestCount);
        Assert.Equal(1, cursor.EventReadRequestCount);
        Assert.Equal(original.Length, cursor.CachedLineCount);
        var bytes = cursor.JournalBytes;

        var backwards = await cursor.ReadAsync(5, 7);
        Assert.Equal(original.Skip(5).Take(7),
            backwards.Select(item => item.Source.Text));
        Assert.Equal(2, cursor.ReadRequestCount);
        Assert.Equal(2, cursor.EventReadRequestCount);
        Assert.Equal(original.Length, cursor.CachedLineCount);
        Assert.Equal(bytes, cursor.JournalBytes);
    }

    [Fact]
    public async Task LargeSourceWindowSplitsOnlyAtCompleteRecordBoundaries()
    {
        var original = Enumerable.Range(0, 90)
            .Select(i => $"Row-{i:000} " + new string('x', 4000))
            .ToArray();
        using var fixture = new Fixture(string.Join("\n", original));
        var before = await File.ReadAllBytesAsync(fixture.Path);
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(
            fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);

        var lines = await cursor.ReadAsync(0, original.Length);

        Assert.Equal(original, lines.Select(x => x.Source.Text));
        Assert.True(cursor.JournalBytes >
            MdViewContainerPageCursor.MaximumContiguousEventReadBytes);
        Assert.Equal(1, cursor.ReadRequestCount);
        Assert.InRange(cursor.EventReadRequestCount, 2L, 10L);
        Assert.Equal(before, await File.ReadAllBytesAsync(fixture.Path));

        var initialEventReads = cursor.EventReadRequestCount;
        var subset = await cursor.ReadAsync(70, 12);
        Assert.Equal(original.Skip(70).Take(12),
            subset.Select(x => x.Source.Text));
        Assert.Equal(initialEventReads + 1, cursor.EventReadRequestCount);
        Assert.Equal(original.Length, cursor.CachedLineCount);
    }

    [Fact]
    public async Task LargeIndividualRecordStillUsesOneBoundedEventRead()
    {
        var large = new string('Q', 320_000);
        using var fixture = new Fixture("Start\n" + large + "\nFinish");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(
            fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);

        var row = Assert.Single(await cursor.ReadAsync(1, 1));

        Assert.Equal(large, row.Source.Text);
        Assert.True(cursor.JournalBytes >
            MdViewContainerPageCursor.MaximumContiguousEventReadBytes);
        Assert.Equal(1, cursor.EventReadRequestCount);
        Assert.Equal(1, cursor.ReadRequestCount);

        var rest = await cursor.ReadAsync(2, 1);
        Assert.Equal("Finish", Assert.Single(rest).Source.Text);
        Assert.Equal(2, cursor.EventReadRequestCount);
    }

    [Fact]
    public async Task FollowingOffsetAndEndOfJournalBothBoundExactRecordSpan()
    {
        var original = Enumerable.Range(0, 30)
            .Select(i => $"Line {i}: Český text 🌍").ToArray();
        using var fixture = new Fixture(string.Join("\r\n", original));
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(
            fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);

        // The initial request scans all lines and ends at _eventBytes.
        var all = await cursor.ReadAsync(0, original.Length);
        Assert.Equal(original, all.Select(x => x.Source.Text));

        // A later read in the middle requires the extra following
        // offset, not the end of the whole journal.
        var before = cursor.EventReadRequestCount;
        var middle = await cursor.ReadAsync(11, 5);
        Assert.Equal(original.Skip(11).Take(5),
            middle.Select(x => x.Source.Text));
        Assert.Equal(before + 1, cursor.EventReadRequestCount);

        // At the scanned end, _eventBytes serves as the final bound.
        var tail = await cursor.ReadAsync(27, 3);
        Assert.Equal(original.Skip(27), tail.Select(x => x.Source.Text));
        Assert.Equal(30, cursor.CachedLineCount);
    }

    [Fact]
    public async Task CorruptedStoredRecordLengthIsRejectedBeforeJsonDecode()
    {
        using var fixture = new Fixture("First\nSecond\nThird");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(
            fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        Assert.Equal(3, (await cursor.ReadAsync(0, 3)).Count);

        // Intentionally damage only the private test journal's first
        // 4-byte record-length header, never the original Markdown.
        var eventsField = typeof(MdViewContainerPageCursor).GetField(
            "_events", BindingFlags.Instance | BindingFlags.NonPublic);
        var journal = Assert.IsType<FileStream>(eventsField?.GetValue(cursor));
        journal.Position = 0;
        await journal.WriteAsync(new byte[sizeof(int)]);
        await journal.FlushAsync();

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            cursor.ReadAsync(0, 3));
        Assert.Equal("First\nSecond\nThird",
            await File.ReadAllTextAsync(fixture.Path));
    }

    [Fact]
    public async Task CorruptedOffsetMonotonicityIsRejectedBeforeBulkAllocation()
    {
        using var fixture = new Fixture("First\nSecond\nThird");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(
            fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        Assert.Equal(3, (await cursor.ReadAsync(0, 3)).Count);

        // Change the second event's offset to zero so the first
        // span is non-progressing. No invalid bulk buffer is created.
        var offsetsField = typeof(MdViewContainerPageCursor).GetField(
            "_offsets", BindingFlags.Instance | BindingFlags.NonPublic);
        var index = Assert.IsType<FileStream>(offsetsField?.GetValue(cursor));
        index.Position = sizeof(long);
        await index.WriteAsync(new byte[sizeof(long)]);
        await index.FlushAsync();

        await Assert.ThrowsAsync<InvalidDataException>(() =>
            cursor.ReadAsync(0, 3));
    }

    [Fact]
    public async Task PreCancelledReadDoesNotAttemptAnyEventStreamOperation()
    {
        using var fixture = new Fixture("One\nTwo\nThree");
        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(
            fixture.Path);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        Assert.Equal(3, (await cursor.ReadAsync(0, 3)).Count);

        var requests = cursor.ReadRequestCount;
        var events = cursor.EventReadRequestCount;
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            cursor.ReadAsync(0, 3, cancellation.Token));

        Assert.Equal(requests, cursor.ReadRequestCount);
        Assert.Equal(events, cursor.EventReadRequestCount);
        Assert.Equal("Two",
            Assert.Single(await cursor.ReadAsync(1, 1)).Source.Text);
    }

    private sealed class Fixture : IDisposable
    {
        private readonly string _directory = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "mdview-contiguous-journal-" + Guid.NewGuid().ToString("N"));

        public Fixture(string content)
        {
            Directory.CreateDirectory(_directory);
            Path = System.IO.Path.Combine(_directory, "README.md");
            File.WriteAllText(Path, content, new UTF8Encoding(false));
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(_directory))
                Directory.Delete(_directory, recursive: true);
        }
    }
}
