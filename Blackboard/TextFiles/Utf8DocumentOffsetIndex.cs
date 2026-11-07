using System.Buffers.Binary;
using System.Text;

namespace Afrowave.Toolbox.Blackboard.TextFiles;

/// <summary>
/// Disk-backed mapping of document-wide decoded UTF-16 offsets into UTF-8
/// byte offsets. The initial UTF-8 BOM is excluded from UTF-16 coordinates.
/// </summary>
/// <remarks>
/// Builds an independent, temporary 16-byte-per-line index from a validated
/// <see cref="Utf8DiskLineIndex"/>. Its caller retains ownership of that source.
/// Construction is bounded-memory but currently performs random source seeks
/// per line and may be slow on rotational disks. This is not an immutable
/// snapshot of a concurrently modified file.
/// </remarks>
public sealed class Utf8DocumentOffsetIndex : IAsyncDisposable
{
    private const int EntrySize = sizeof(long) + sizeof(int) + sizeof(int);
    /// <summary>Default limit for the temporary UTF-16 line table (256 MiB).</summary>
    public const long DefaultMaximumIndexBytes = 256L * 1024 * 1024;

    private readonly Utf8DiskLineIndex _source;
    private readonly FileStream _entries;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _disposed;

    private Utf8DocumentOffsetIndex(
        Utf8DiskLineIndex source, FileStream entries, long totalUtf16Length)
    {
        _source = source;
        _entries = entries;
        Utf16Length = totalUtf16Length;
    }

    /// <summary>Total number of decoded UTF-16 units, including line terminators but excluding BOM.</summary>
    public long Utf16Length { get; }

    /// <summary>Temporary index storage size in bytes.</summary>
    public long IndexBytes => checked(_source.LineCount * EntrySize);

    /// <summary>
    /// Builds a bounded disk-backed lookup table using the validated physical
    /// line reader. Existing UTF-8 source data is never written to.
    /// </summary>
    /// <param name="source">Previously validated physical-line index, owned by caller.</param>
    /// <param name="maximumIndexBytes">Temporary index byte budget.</param>
    /// <param name="cancellationToken">Cancels index construction.</param>
    /// <returns>Independent UTF-16 position index; dispose after use.</returns>
    public static async Task<Utf8DocumentOffsetIndex> CreateAsync(
        Utf8DiskLineIndex source,
        long maximumIndexBytes = DefaultMaximumIndexBytes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (maximumIndexBytes < EntrySize)
            throw new ArgumentOutOfRangeException(nameof(maximumIndexBytes));
        if (source.LineCount > maximumIndexBytes / EntrySize)
            throw new InvalidDataException("UTF-16 index exceeds its disk budget.");

        var path = Path.Combine(Path.GetTempPath(),
            "afrowave-utf16-index-" + Guid.NewGuid().ToString("N") + ".idx");
        var entries = new FileStream(path, FileMode.CreateNew,
            FileAccess.ReadWrite, FileShare.None, 65536,
            FileOptions.Asynchronous | FileOptions.DeleteOnClose);
        try
        {
            var entry = new byte[EntrySize];
            long total = 0;
            for (long lineNumber = 0; lineNumber < source.LineCount;)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var page = await source.ReadPageAsync(lineNumber, 128, cancellationToken)
                    .ConfigureAwait(false);
                if (page.Lines.Count == 0)
                    throw new InvalidDataException("Line index did not make progress.");

                foreach (var line in page.Lines)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    // UTF-16 length of the content is known without materializing
                    // the document. The byte gap to the following start is the
                    // terminator: zero, LF/CR (one), or CRLF (two).
                    var endByte = await source.GetByteOffsetAsync(
                        lineNumber, line.Length, cancellationToken).ConfigureAwait(false);
                    var nextByte = lineNumber + 1 < source.LineCount
                        ? await source.GetByteOffsetAsync(
                            lineNumber + 1, 0, cancellationToken).ConfigureAwait(false)
                        : source.ByteLength;
                    var gap = nextByte - endByte;
                    if (gap is < 0 or > 2)
                        throw new InvalidDataException("Invalid line terminator width.");

                    BinaryPrimitives.WriteInt64LittleEndian(entry.AsSpan(0, 8), total);
                    BinaryPrimitives.WriteInt32LittleEndian(entry.AsSpan(8, 4), line.Length);
                    BinaryPrimitives.WriteInt32LittleEndian(entry.AsSpan(12, 4), (int)gap);
                    await entries.WriteAsync(entry, cancellationToken).ConfigureAwait(false);
                    total = checked(total + line.Length + gap);
                    lineNumber++;
                }
            }

            await entries.FlushAsync(cancellationToken).ConfigureAwait(false);
            entries.Position = 0;
            return new Utf8DocumentOffsetIndex(source, entries, total);
        }
        catch
        {
            await entries.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>Maps an absolute decoded UTF-16 offset (including newline units) to a source byte position.</summary>
    /// <param name="utf16Offset">Document-wide zero-based UTF-16 offset, or EOF.</param>
    /// <param name="cancellationToken">Cancels index access.</param>
    /// <returns>Absolute position in the original UTF-8 source bytes.</returns>
    public async Task<long> GetByteOffsetAsync(
        long utf16Offset, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (utf16Offset < 0 || utf16Offset > Utf16Length)
            throw new ArgumentOutOfRangeException(nameof(utf16Offset));
        if (utf16Offset == Utf16Length)
            return _source.ByteLength;

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            long low = 0;
            long high = _source.LineCount - 1;
            var entry = new byte[EntrySize];
            while (low <= high)
            {
                var middle = low + (high - low) / 2;
                _entries.Position = checked(middle * EntrySize);
                await _entries.ReadExactlyAsync(entry, cancellationToken)
                    .ConfigureAwait(false);
                var start = BinaryPrimitives.ReadInt64LittleEndian(entry.AsSpan(0, 8));
                var contentLength = BinaryPrimitives.ReadInt32LittleEndian(entry.AsSpan(8, 4));
                var terminatorLength = BinaryPrimitives.ReadInt32LittleEndian(entry.AsSpan(12, 4));
                var end = checked(start + contentLength + terminatorLength);
                if (utf16Offset < start)
                    high = middle - 1;
                else if (utf16Offset >= end)
                    low = middle + 1;
                else
                {
                    var column = checked((int)(utf16Offset - start));
                    if (column <= contentLength)
                        return await _source.GetByteOffsetAsync(
                            middle, column, cancellationToken).ConfigureAwait(false);
                    var terminatorStart = await _source.GetByteOffsetAsync(
                        middle, contentLength, cancellationToken).ConfigureAwait(false);
                    return checked(terminatorStart + column - contentLength);
                }
            }
            throw new InvalidDataException("UTF-16 index contains inconsistent ranges.");
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Disposes the temporary UTF-16 index, not its source line index.</summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_disposed)
                return;
            _disposed = true;
            await _entries.DisposeAsync().ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }
}
