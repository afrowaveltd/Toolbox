using System.Buffers.Binary;
using System.Text;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>
/// Completed conservative list-wide spacing evidence. A loose candidate is
/// known only after the whole observed group has been scanned; this is not
/// CommonMark's definitive tight/loose list grammar.
/// </summary>
public sealed record MarkdownListSpacingGroup(
    long Start, long End, int Depth, int QuoteDepth,
    bool IsLooseCandidate);

/// <summary>
/// Optional disk-backed list spacing index. Groups and list-item markers are
/// kept in fixed-size temporary disk records; only 32 active group descriptors
/// are kept in RAM during construction.
/// </summary>
/// <remarks>
/// Built in one additional sequential scan after the source index. This
/// conservative list-wide evidence never stores source body text. Grouping
/// uses the existing streaming scanner's bounded list ownership, so the
/// result must not be represented as full CommonMark tight/loose fidelity.
/// </remarks>
public sealed class MarkdownListSpacingIndex : IDisposable, IAsyncDisposable
{
    private const int GroupBytes = 24;
    private const int ItemBytes = 16;
    private const long DefaultMaxIndexBytes = 256L * 1024 * 1024;
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private readonly MarkdownDiskPageIndex _pages;
    private readonly string _groupsPath;
    private readonly string _itemsPath;
    private int _disposed;

    private MarkdownListSpacingIndex(MarkdownDiskPageIndex pages,
        string groupsPath, string itemsPath, long groups, long items)
    {
        _pages = pages;
        _groupsPath = groupsPath;
        _itemsPath = itemsPath;
        GroupCount = groups;
        ItemCount = items;
    }

    /// <summary>Number of completed list-group records.</summary>
    public long GroupCount { get; }

    /// <summary>Number of indexed list-item markers.</summary>
    public long ItemCount { get; }

    /// <summary>
    /// Create an optional, separate list-wide spacing summary. The quota
    /// applies to both temporary files combined.
    /// </summary>
    public static async Task<MarkdownListSpacingIndex> BuildAsync(
        MarkdownDiskPageIndex pages,
        CancellationToken cancellationToken = default,
        string? tempDirectory = null,
        long maxIndexBytes = DefaultMaxIndexBytes)
    {
        ArgumentNullException.ThrowIfNull(pages);
        if (maxIndexBytes < GroupBytes + ItemBytes)
            throw new ArgumentOutOfRangeException(nameof(maxIndexBytes));

        var directory = tempDirectory ?? Path.GetTempPath();
        Directory.CreateDirectory(directory);
        var suffix = Guid.NewGuid().ToString("N");
        var groupsPath = Path.Combine(directory,
            "afrowave-markdown-list-groups-" + suffix + ".tmp");
        var itemsPath = Path.Combine(directory,
            "afrowave-markdown-list-items-" + suffix + ".tmp");

        try
        {
            if (pages.PageCount > 0)
                _ = await pages.ReadPageAsync(0, cancellationToken);

            await using var source = new FileStream(pages.SourcePath,
                FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            var initialSize = source.Length;
            var initialModified = File.GetLastWriteTimeUtc(pages.SourcePath);

            await using var groupsFile = new FileStream(groupsPath,
                FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None,
                4096, FileOptions.Asynchronous);
            await using var itemsFile = new FileStream(itemsPath,
                FileMode.CreateNew, FileAccess.Write, FileShare.None,
                4096, FileOptions.Asynchronous);
            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(groupsPath,
                    UnixFileMode.UserRead | UnixFileMode.UserWrite);
                File.SetUnixFileMode(itemsPath,
                    UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }

            using var reader = new StreamReader(source, StrictUtf8,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 8192, leaveOpen: true);

            var stack = new List<ActiveGroup>(32);
            long groupCount = 0, itemCount = 0, lines = 0, endOffset = 0;
            var options = new MarkdownStreamOptions
            {
                MaxLineChars = checked(pages.MaxLineBytes + 2)
            };

            async Task CompleteAsync(int keepDepth, long end)
            {
                while (stack.Count > keepDepth)
                {
                    var group = stack[^1];
                    stack.RemoveAt(stack.Count - 1);
                    await WriteGroupAsync(groupsFile, group,
                        group.Index, end, cancellationToken);
                }
            }

            await foreach (var line in MarkdownStreamingReader.ScanAsync(
                reader, options, cancellationToken))
            {
                endOffset = checked(line.Start + line.Length);
                lines++;

                if (line.Kind == MarkdownStreamLineKind.Blank)
                    continue; // A later line decides whether the list continues.

                var depth = line.ListOwnerStart.HasValue ? line.ListDepth : 0;
                if (depth > 32)
                    throw new InvalidDataException(
                        "List group depth exceeds 32 entries.");

                // An explicit quote-container transition starts a new group.
                if (stack.Count > 0 &&
                    stack[0].QuoteDepth != line.QuoteDepth)
                    await CompleteAsync(0, line.Start);
                await CompleteAsync(Math.Min(depth, stack.Count), line.Start);

                if (depth == 0)
                    continue;

                var isMarker = line.Kind == MarkdownStreamLineKind.Markdown &&
                    line.ListOwnerStart == line.Start;
                if (stack.Count < depth && !isMarker)
                {
                    // Keep conservative evidence only: an orphan continuation
                    // cannot introduce an unobserved item marker.
                    await CompleteAsync(0, line.Start);
                    continue;
                }

                if (isMarker)
                {
                    var markerKind = MarkerKind(line.Content.AsSpan());
                    if (stack.Count == depth &&
                        stack[^1].MarkerKind != markerKind)
                        await CompleteAsync(depth - 1, line.Start);

                    if (stack.Count < depth)
                    {
                        if (stack.Count + 1 != depth)
                            throw new InvalidDataException(
                                "Unexpected jump in list group depth.");
                        if (groupCount >= maxIndexBytes / GroupBytes ||
                            checked((groupCount + 1) * GroupBytes +
                                    (itemCount + 1) * ItemBytes) > maxIndexBytes)
                            throw new InvalidDataException(
                                "List spacing index exceeds temporary disk quota.");

                        var group = new ActiveGroup(groupCount, line.Start,
                            depth, line.QuoteDepth, markerKind);
                        stack.Add(group);
                        await WriteGroupAsync(groupsFile, group, groupCount,
                            -1, cancellationToken);
                        groupCount++;
                    }
                    else if (line.ListSpacingHint ==
                             MarkdownListSpacingHint.BlankSeparatedItem)
                    {
                        stack[^1].IsLooseCandidate = true;
                    }

                    if (checked(groupCount * GroupBytes +
                                (itemCount + 1) * ItemBytes) > maxIndexBytes)
                        throw new InvalidDataException(
                            "List spacing index exceeds temporary disk quota.");

                    var item = new byte[ItemBytes];
                    BinaryPrimitives.WriteInt64LittleEndian(item.AsSpan(0, 8),
                        line.Start);
                    BinaryPrimitives.WriteInt64LittleEndian(item.AsSpan(8, 8),
                        stack[^1].Index);
                    await itemsFile.WriteAsync(item, cancellationToken);
                    itemCount++;
                }
                else if (line.ListSpacingHint ==
                         MarkdownListSpacingHint.BlankSeparatedParagraph &&
                         stack.Count >= depth)
                {
                    stack[depth - 1].IsLooseCandidate = true;
                }
            }

            await CompleteAsync(0, endOffset);
            if (lines != pages.LineCount ||
                groupsFile.Length != groupCount * GroupBytes ||
                itemsFile.Length != itemCount * ItemBytes ||
                source.Length != initialSize ||
                File.GetLastWriteTimeUtc(pages.SourcePath) != initialModified)
                throw new IOException(
                    "Markdown source changed during list spacing indexing.");

            await groupsFile.FlushAsync(cancellationToken);
            await itemsFile.FlushAsync(cancellationToken);
            return new MarkdownListSpacingIndex(pages, groupsPath, itemsPath,
                groupCount, itemCount);
        }
        catch
        {
            foreach (var path in new[] { groupsPath, itemsPath })
            {
                try { File.Delete(path); }
                catch (IOException) { /* Original failure wins. */ }
                catch (UnauthorizedAccessException) { /* Original failure wins. */ }
            }
            throw;
        }
    }

    /// <summary>
    /// Find the completed group for a list marker. One binary search over
    /// source-ordered marker records and one fixed-size group record read.
    /// Continuations should use their `MarkdownStreamLine.ListOwnerStart`.
    /// </summary>
    public async Task<MarkdownListSpacingGroup?> FindForItemAsync(
        long markerStart, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
        if (markerStart < 0)
            throw new ArgumentOutOfRangeException(nameof(markerStart));
        if (ItemCount == 0)
            return null;

        // Check that the dependency was not changed after construction.
        if (_pages.PageCount > 0)
            _ = await _pages.ReadPageAsync(0, cancellationToken);

        await using var items = new FileStream(_itemsPath, FileMode.Open,
            FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
        var data = new byte[ItemBytes];
        long low = 0, high = ItemCount;
        while (low < high)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var mid = low + (high - low) / 2;
            items.Seek(checked(mid * ItemBytes), SeekOrigin.Begin);
            await items.ReadExactlyAsync(data, cancellationToken);
            var at = BinaryPrimitives.ReadInt64LittleEndian(data.AsSpan(0, 8));
            if (at < markerStart) low = mid + 1;
            else high = mid;
        }
        if (low >= ItemCount)
            return null;
        items.Seek(checked(low * ItemBytes), SeekOrigin.Begin);
        await items.ReadExactlyAsync(data, cancellationToken);
        if (BinaryPrimitives.ReadInt64LittleEndian(data.AsSpan(0, 8)) != markerStart)
            return null;
        var index = BinaryPrimitives.ReadInt64LittleEndian(data.AsSpan(8, 8));
        if (index < 0 || index >= GroupCount)
            throw new InvalidDataException("Invalid list item group reference.");

        var record = new byte[GroupBytes];
        await using var groups = new FileStream(_groupsPath, FileMode.Open,
            FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
        groups.Seek(checked(index * GroupBytes), SeekOrigin.Begin);
        await groups.ReadExactlyAsync(record, cancellationToken);
        var start = BinaryPrimitives.ReadInt64LittleEndian(record.AsSpan(0, 8));
        var end = BinaryPrimitives.ReadInt64LittleEndian(record.AsSpan(8, 8));
        if (start < 0 || end < start || markerStart < start || markerStart >= end ||
            record[20] > 1)
            throw new InvalidDataException("Invalid list spacing group record.");

        return new MarkdownListSpacingGroup(start, end,
            BinaryPrimitives.ReadInt32LittleEndian(record.AsSpan(16, 4)),
            record[21], record[20] == 1);
    }

    private static async Task WriteGroupAsync(FileStream stream,
        ActiveGroup group, long index, long end, CancellationToken ct)
    {
        var data = new byte[GroupBytes];
        BinaryPrimitives.WriteInt64LittleEndian(data.AsSpan(0, 8),
            group.Start);
        BinaryPrimitives.WriteInt64LittleEndian(data.AsSpan(8, 8), end);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(16, 4),
            group.Depth);
        data[20] = group.IsLooseCandidate ? (byte)1 : (byte)0;
        data[21] = checked((byte)group.QuoteDepth);
        stream.Seek(checked(index * GroupBytes), SeekOrigin.Begin);
        await stream.WriteAsync(data, ct);
    }

    private static char MarkerKind(ReadOnlySpan<char> line)
    {
        var body = line.TrimStart();
        if (body.IsEmpty) return '?';
        if (body[0] is '-' or '+' or '*') return body[0];
        return char.IsAsciiDigit(body[0]) ? '1' : '?';
    }

    private sealed class ActiveGroup
    {
        public ActiveGroup(long index, long start, int depth,
            int quoteDepth, char markerKind)
        {
            Index = index;
            Start = start;
            Depth = depth;
            QuoteDepth = quoteDepth;
            MarkerKind = markerKind;
        }

        public long Index { get; }
        public long Start { get; }
        public int Depth { get; }
        public int QuoteDepth { get; }
        public char MarkerKind { get; }
        public bool IsLooseCandidate { get; set; }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
        File.Delete(_groupsPath);
        File.Delete(_itemsPath);
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}
