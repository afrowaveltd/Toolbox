using System.Buffers.Binary;
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

        var requestedEnd = firstLine > long.MaxValue - maxLines
            ? long.MaxValue : firstLine + maxLines;
        if (requestedEnd > _scannedLines)
            await ScanUntilAsync(requestedEnd, cancellationToken).ConfigureAwait(false);

        if (firstLine >= _scannedLines)
            return [];

        var count = (int)Math.Min(maxLines, _scannedLines - firstLine);
        var offsetsBuffer = new byte[checked(count * sizeof(long))];
        _offsets.Position = checked(firstLine * sizeof(long));
        await _offsets.ReadExactlyAsync(offsetsBuffer, cancellationToken)
            .ConfigureAwait(false);

        var lines = new List<MarkdownContainerLine>(count);
        for (var i = 0; i < count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var entryOffset = BinaryPrimitives.ReadInt64LittleEndian(
                offsetsBuffer.AsSpan(i * sizeof(long), sizeof(long)));
            if (entryOffset < 0 || entryOffset > _eventBytes - sizeof(int))
                throw new InvalidDataException("Container journal offset is invalid.");

            _events.Position = entryOffset;
            var sizeBytes = new byte[sizeof(int)];
            await _events.ReadExactlyAsync(sizeBytes, cancellationToken)
                .ConfigureAwait(false);
            var length = BinaryPrimitives.ReadInt32LittleEndian(sizeBytes);
            if (length < 1 || length > 8 * 1024 * 1024 ||
                length > _eventBytes - entryOffset - sizeof(int))
                throw new InvalidDataException("Container journal record is invalid.");

            var serialized = new byte[length];
            await _events.ReadExactlyAsync(serialized, cancellationToken)
                .ConfigureAwait(false);
            MarkdownContainerLine? value;
            try
            {
                value = JsonSerializer.Deserialize<MarkdownContainerLine>(serialized);
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException("Container journal JSON is invalid.", ex);
            }
            if (value is null || value.Source.LineNumber != firstLine + i)
                throw new InvalidDataException("Container journal line numbering is invalid.");
            lines.Add(value);
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
                _scanner = MarkdownIncrementalContainerScanner.ScanAsync(
                    _reader, cancellationToken: cancellationToken)
                    .GetAsyncEnumerator(cancellationToken);
            }

            while (_scannedLines < requestedEnd)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!await _scanner.MoveNextAsync().ConfigureAwait(false))
                    break;

                var line = _scanner.Current;
                if (line.Source.LineNumber != _scannedLines)
                    throw new InvalidDataException("Scanner line numbering is inconsistent.");

                var record = JsonSerializer.SerializeToUtf8Bytes(line);
                if (record.Length > 8 * 1024 * 1024)
                    throw new InvalidDataException("Container event exceeds the record limit.");
                if (record.Length + sizeof(int) > _maximumJournalBytes - _eventBytes ||
                    _scannedLines >= _maximumOffsetBytes / sizeof(long))
                    throw new InvalidDataException("Container journal exceeds its disk budget.");

                var position = _eventBytes;
                _events.Position = position;
                var size = new byte[sizeof(int)];
                BinaryPrimitives.WriteInt32LittleEndian(size, record.Length);
                await _events.WriteAsync(size, cancellationToken).ConfigureAwait(false);
                await _events.WriteAsync(record, cancellationToken).ConfigureAwait(false);

                _offsets.Position = checked(_scannedLines * sizeof(long));
                var positionBytes = new byte[sizeof(long)];
                BinaryPrimitives.WriteInt64LittleEndian(positionBytes, position);
                await _offsets.WriteAsync(positionBytes, cancellationToken)
                    .ConfigureAwait(false);
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
