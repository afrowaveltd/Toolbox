using System.Buffers.Binary;
using System.Diagnostics;
using System.Text.Json;
using Afrowave.Toolbox.Blackboard.TextFiles;
using Afrowave.Toolbox.ExchangeHouse.Markdown;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Bounded Markdown container cursor with a disk-backed event journal.
/// Forward scanning adds classified events once; previously scanned source
/// ranges are read from the journal without replaying the parser.
/// </summary>
/// <remarks>
/// The snapshot is caller-owned. This journal stores complete classified
/// line events, including their text, in temporary files, not in RAM.
/// Jumping beyond the scanned high-water mark still scans intervening lines.
/// </remarks>
public sealed class MdViewContainerPageCursor : IAsyncDisposable
{
    /// <summary>Default maximum serialized event journal size (512 MiB).</summary>
    public const long DefaultMaximumJournalBytes = 512L * 1024 * 1024;
    /// <summary>Default maximum offset table size (256 MiB).</summary>
    public const long DefaultMaximumOffsetBytes = 256L * 1024 * 1024;

    /// <summary>
    /// Maximum contiguous event-journal read for ordinary source rows.
    /// An individual valid record larger than this is read on its own,
    /// never rejected merely for exceeding the batching threshold.
    /// </summary>
    internal const int MaximumContiguousEventReadBytes = 256 * 1024;

    private const int MaximumSerializedEventBytes = 8 * 1024 * 1024;

    private readonly Utf8ValidatedSnapshot _snapshot;
    private readonly FileStream _events;
    private readonly FileStream _offsets;
    private readonly long _maximumJournalBytes;
    private readonly long _maximumOffsetBytes;
    private StreamReader? _reader;
    private IAsyncEnumerator<MarkdownContainerLine>? _scanner;
    private long _scannedLines;
    private long _eventBytes;
    private bool _faulted;
    private bool _disposed;

    /// <summary>
    /// Optional single-caller diagnostic counters. Null in production, with
    /// no per-line timestamp collection unless explicitly enabled by tests
    /// or the manual benchmark. No disk or console logging occurs here.
    /// </summary>
    internal MdViewJournalScanTimings? ScanTimings { get; set; }

    /// <summary>Creates a disk-backed cursor without taking ownership of its snapshot.</summary>
    /// <param name="snapshot">Validated snapshot that must outlive this cursor.</param>
    /// <param name="maximumJournalBytes">Maximum temporary bytes for serialized events.</param>
    /// <param name="maximumOffsetBytes">Maximum temporary bytes for line offsets.</param>
    public MdViewContainerPageCursor(
        Utf8ValidatedSnapshot snapshot,
        long maximumJournalBytes = DefaultMaximumJournalBytes,
        long maximumOffsetBytes = DefaultMaximumOffsetBytes)
    {
        _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        if (maximumJournalBytes < 0)
            throw new ArgumentOutOfRangeException(nameof(maximumJournalBytes));
        if (maximumOffsetBytes < sizeof(long))
            throw new ArgumentOutOfRangeException(nameof(maximumOffsetBytes));
        _maximumJournalBytes = maximumJournalBytes;
        _maximumOffsetBytes = maximumOffsetBytes;

        _events = OpenTemporaryJournal();
        try
        {
            _offsets = OpenTemporaryJournal();
        }
        catch
        {
            _events.Dispose();
            throw;
        }
    }

    /// <summary>Number of classified lines currently stored on disk.</summary>
    public long CachedLineCount => _scannedLines;

    /// <summary>Serialized event bytes currently stored on disk.</summary>
    public long JournalBytes => _eventBytes;

    /// <summary>
    /// Diagnostic count of validated source read requests. Internal so
    /// tests can verify I/O batching without introducing public API.
    /// This counts API requests, not the lower-level journal read calls.
    /// </summary>
    internal long ReadRequestCount { get; private set; }

    /// <summary>
    /// Diagnostic number of event-journal bulk stream reads. Each
    /// validated contiguous window uses one ReadExactlyAsync operation;
    /// this is not the number of physical disk I/O operations.
    /// </summary>
    internal long EventReadRequestCount { get; private set; }

    /// <summary>
    /// Count of actual event stream write-position assignments. Sequential
    /// appends already at the end do not require setting Position again.
    /// </summary>
    internal long EventWritePositionSetterCount { get; private set; }

    /// <summary>
    /// Count of actual offset stream write-position assignments. Sequential
    /// appends already at the end do not require setting Position again.
    /// </summary>
    internal long OffsetWritePositionSetterCount { get; private set; }

    /// <summary>
    /// Benchmark-only A/B comparison with the pre-optimization write path.
    /// When true, restores unconditional Position assignments before every
    /// journal append. Never enabled by the production viewer.
    /// </summary>
    internal bool ForceJournalAppendPositionSetters { get; set; }

    /// <summary>
    /// Returns at most 128 source-preserving classified physical lines,
    /// without replaying already cached scanner events.
    /// </summary>
    /// <param name="firstLine">Zero-based physical source line.</param>
    /// <param name="maxLines">Page size from 1 to 128 physical lines.</param>
    /// <param name="cancellationToken">Cancels scanning and disk access.</param>
    /// <returns>Line metadata from the validated copy.</returns>
    public async Task<IReadOnlyList<MarkdownContainerLine>> ReadAsync(
        long firstLine, int maxLines, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_faulted)
            throw new InvalidOperationException("The container event journal is unusable.");
        if (firstLine < 0)
            throw new ArgumentOutOfRangeException(nameof(firstLine));
        if (maxLines is < 1 or > 128)
            throw new ArgumentOutOfRangeException(nameof(maxLines));
        cancellationToken.ThrowIfCancellationRequested();
        ReadRequestCount++;

        var requestedEnd = firstLine > long.MaxValue - maxLines
            ? long.MaxValue : firstLine + maxLines;
        if (requestedEnd > _scannedLines)
            await ScanUntilAsync(requestedEnd, cancellationToken).ConfigureAwait(false);

        if (firstLine >= _scannedLines)
            return [];

        var count = (int)Math.Min(maxLines, _scannedLines - firstLine);

        // One offset-table read also includes the following record's
        // offset, when available. At the journal's current high-water
        // mark, _eventBytes is the authoritative exclusive end offset.
        // This makes each event's exact stored byte span known before
        // touching the event stream, including the final requested row.
        var offsetCount = count +
            (firstLine + count < _scannedLines ? 1 : 0);
        var offsetsBuffer = new byte[checked(offsetCount * sizeof(long))];
        var readTimings = ScanTimings;
        var offsetPositionStartedAt = readTimings is null
            ? 0 : Stopwatch.GetTimestamp();
        _offsets.Position = checked(firstLine * sizeof(long));
        readTimings?.Record(MdViewJournalScanStage.OffsetReadPosition,
            offsetPositionStartedAt, firstLine, -1L);
        var offsetReadStartedAt = readTimings is null
            ? 0 : Stopwatch.GetTimestamp();
        await _offsets.ReadExactlyAsync(offsetsBuffer, cancellationToken)
            .ConfigureAwait(false);
        // At this point the first actual journal offset is not known.
        readTimings?.Record(MdViewJournalScanStage.OffsetTableRead,
            offsetReadStartedAt, firstLine, -1L);

        var offsets = new long[count + 1];
        for (var i = 0; i < offsetCount; i++)
            offsets[i] = BinaryPrimitives.ReadInt64LittleEndian(
                offsetsBuffer.AsSpan(i * sizeof(long), sizeof(long)));
        if (offsetCount == count)
            offsets[count] = _eventBytes;

        // Fail before allocating a large buffer if even one offset
        // crosses the journal boundary, overlaps the next record, or
        // describes an impossible serialized source event.
        for (var i = 0; i < count; i++)
        {
            // Check monotonic bounds *before* subtraction, including
            // corrupted long offsets that could overflow a difference.
            if (offsets[i] < 0 ||
                offsets[i + 1] <= offsets[i] ||
                offsets[i + 1] > _eventBytes)
                throw new InvalidDataException(
                    "Container journal record offset or span is invalid.");
            var size = offsets[i + 1] - offsets[i];
            if (size < sizeof(int) + 1 ||
                size > MaximumSerializedEventBytes + sizeof(int))
                throw new InvalidDataException(
                    "Container journal record offset or span is invalid.");
        }

        var lines = new List<MarkdownContainerLine>(count);
        var first = 0;
        while (first < count)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Coalesce neighboring complete records into at most 256 KiB
            // of journal data. A single valid larger record remains one
            // bounded (at most 8 MiB + header) read, rather than
            // allocating an extra buffer or splitting JSON mid-record.
            var lastExclusive = first + 1;
            while (lastExclusive < count &&
                offsets[lastExclusive + 1] - offsets[first] <=
                    MaximumContiguousEventReadBytes)
                lastExclusive++;

            var readLength = checked((int)(
                offsets[lastExclusive] - offsets[first]));
            var serializedWindow = new byte[readLength];
            var eventReadPositionStartedAt = readTimings is null
                ? 0 : Stopwatch.GetTimestamp();
            _events.Position = offsets[first];
            readTimings?.Record(MdViewJournalScanStage.EventReadPosition,
                eventReadPositionStartedAt, firstLine + first, offsets[first]);
            EventReadRequestCount++;
            var eventReadStartedAt = readTimings is null
                ? 0 : Stopwatch.GetTimestamp();
            await _events.ReadExactlyAsync(
                serializedWindow, cancellationToken).ConfigureAwait(false);
            readTimings?.Record(MdViewJournalScanStage.EventWindowRead,
                eventReadStartedAt, firstLine + first, offsets[first]);

            for (var i = first; i < lastExclusive; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var entryOffset = checked((int)(offsets[i] - offsets[first]));
                var storedBytes = checked((int)(offsets[i + 1] - offsets[i]));
                var record = serializedWindow.AsSpan(entryOffset, storedBytes);
                var length = BinaryPrimitives.ReadInt32LittleEndian(
                    record[..sizeof(int)]);
                if (length < 1 ||
                    length > MaximumSerializedEventBytes ||
                    length != storedBytes - sizeof(int))
                    throw new InvalidDataException(
                        "Container journal record length is invalid.");

                MarkdownContainerLine? value;
                var deserializeStartedAt = readTimings is null
                    ? 0 : Stopwatch.GetTimestamp();
                try
                {
                    value = JsonSerializer.Deserialize<MarkdownContainerLine>(
                        record[sizeof(int)..]);
                }
                catch (JsonException ex)
                {
                    throw new InvalidDataException(
                        "Container journal JSON is invalid.", ex);
                }
                readTimings?.Record(MdViewJournalScanStage.DeserializeEvent,
                    deserializeStartedAt, firstLine + i, offsets[i]);
                if (value is null || value.Source.LineNumber != firstLine + i)
                    throw new InvalidDataException(
                        "Container journal line numbering is invalid.");
                lines.Add(value);
            }
            first = lastExclusive;
        }
        return lines;
    }

    private async Task ScanUntilAsync(long requestedEnd, CancellationToken cancellationToken)
    {
        try
        {
            if (_scanner is null)
            {
                _reader = _snapshot.OpenReader();
                // An async enumerator outlives a single ReadAsync request.
                // Capturing its first caller's token would cancel unrelated
                // future navigation when that caller later disposes/cancels it.
                // Keep iterator lifetime independent and check each request's
                // token around MoveNextAsync and all journal IO.
                _scanner = MarkdownIncrementalContainerScanner.ScanAsync(
                    _reader, cancellationToken: CancellationToken.None)
                    .GetAsyncEnumerator(CancellationToken.None);
            }

            while (_scannedLines < requestedEnd)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var timing = ScanTimings;
                var scannerStart = timing is null ? 0 : Stopwatch.GetTimestamp();
                var hasNext = await _scanner.MoveNextAsync().ConfigureAwait(false);
                timing?.Record(MdViewJournalScanStage.ScannerAdvance,
                    scannerStart, _scannedLines, _eventBytes);
                if (!hasNext)
                    break;
                cancellationToken.ThrowIfCancellationRequested();

                var line = _scanner.Current;
                if (line.Source.LineNumber != _scannedLines)
                    throw new InvalidDataException("Scanner line numbering is inconsistent.");

                var serializeStart = timing is null ? 0 : Stopwatch.GetTimestamp();
                var record = JsonSerializer.SerializeToUtf8Bytes(line);
                timing?.Record(MdViewJournalScanStage.SerializeEvent,
                    serializeStart, _scannedLines, _eventBytes);
                if (record.Length > MaximumSerializedEventBytes)
                    throw new InvalidDataException("Container event exceeds the record limit.");
                if (record.Length + sizeof(int) > _maximumJournalBytes - _eventBytes ||
                    _scannedLines >= _maximumOffsetBytes / sizeof(long))
                    throw new InvalidDataException("Container journal exceeds its disk budget.");

                var position = _eventBytes;
                var eventWritePositionStartedAt = timing is null
                    ? 0 : Stopwatch.GetTimestamp();
                // FileStream.Position may synchronously drain its write
                // buffer even when asked to seek to the current position.
                // Avoid re-seeking on every sequential journal record.
                // Readback may have moved this stream: in that case restore
                // the append position exactly once before the next write.
                if (ForceJournalAppendPositionSetters || _events.Position != position)
                {
                    _events.Position = position;
                    EventWritePositionSetterCount++;
                }
                timing?.Record(MdViewJournalScanStage.EventWritePosition,
                    eventWritePositionStartedAt, _scannedLines, position);
                var size = new byte[sizeof(int)];
                BinaryPrimitives.WriteInt32LittleEndian(size, record.Length);
                var lengthWriteStart = timing is null ? 0 : Stopwatch.GetTimestamp();
                await _events.WriteAsync(size, cancellationToken).ConfigureAwait(false);
                timing?.Record(MdViewJournalScanStage.EventLengthWrite,
                    lengthWriteStart, _scannedLines, position);

                var payloadWriteStart = timing is null ? 0 : Stopwatch.GetTimestamp();
                await _events.WriteAsync(record, cancellationToken).ConfigureAwait(false);
                timing?.Record(MdViewJournalScanStage.EventPayloadWrite,
                    payloadWriteStart, _scannedLines, position);

                var offsetWritePositionStartedAt = timing is null
                    ? 0 : Stopwatch.GetTimestamp();
                var offsetAppendPosition = checked(_scannedLines * sizeof(long));
                if (ForceJournalAppendPositionSetters ||
                    _offsets.Position != offsetAppendPosition)
                {
                    _offsets.Position = offsetAppendPosition;
                    OffsetWritePositionSetterCount++;
                }
                timing?.Record(MdViewJournalScanStage.OffsetWritePosition,
                    offsetWritePositionStartedAt, _scannedLines, position);
                var positionBytes = new byte[sizeof(long)];
                BinaryPrimitives.WriteInt64LittleEndian(positionBytes, position);
                var offsetWriteStart = timing is null ? 0 : Stopwatch.GetTimestamp();
                await _offsets.WriteAsync(positionBytes, cancellationToken)
                    .ConfigureAwait(false);
                timing?.Record(MdViewJournalScanStage.OffsetWrite,
                    offsetWriteStart, _scannedLines, position);
                _eventBytes = checked(position + sizeof(int) + record.Length);
                _scannedLines++;
            }
        }
        catch
        {
            // A cancelled/partial write or failed scan cannot be resumed from
            // an indeterminate iterator/file position. Fail closed.
            _faulted = true;
            throw;
        }
    }

    private static FileStream OpenTemporaryJournal()
    {
        var path = Path.Combine(Path.GetTempPath(),
            "afrowave-mdview-journal-" + Guid.NewGuid().ToString("N") + ".tmp");
        return new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite,
            FileShare.None, 65536, FileOptions.Asynchronous | FileOptions.DeleteOnClose);
    }

    /// <summary>Disposes scanner and both temporary disk journals, not the snapshot.</summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
        _disposed = true;
        try
        {
            if (_scanner is not null)
                await _scanner.DisposeAsync().ConfigureAwait(false);
            _reader?.Dispose();
        }
        finally
        {
            await _offsets.DisposeAsync().ConfigureAwait(false);
            await _events.DisposeAsync().ConfigureAwait(false);
        }
    }
}
