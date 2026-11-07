using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace Afrowave.Toolbox.Blackboard.TextFiles;

/// <summary>
/// One bounded page of physical UTF-8 lines. Line numbers are zero-based; this
/// is a source index, not a visual Markdown layout or a CommonMark parser.
/// </summary>
/// <param name="FirstLine">Requested first physical line (zero-based).</param>
/// <param name="NextLine">Position to request for the next page.</param>
/// <param name="TotalLines">Total physical lines in the indexed file.</param>
/// <param name="Lines">Complete decoded lines, excluding line terminators.</param>
public sealed record Utf8LinePage(
    long FirstLine, long NextLine, long TotalLines, IReadOnlyList<string> Lines)
{
    /// <summary>Whether there are more lines after this page.</summary>
    public bool HasMore => NextLine < TotalLines;
}

/// <summary>
/// Strict UTF-8 physical-line index backed by an on-disk temporary offset table.
/// The entire file is validated in a sequential pass before any page is exposed.
/// Only a fixed-size read buffer and one bounded page are kept in memory.
/// </summary>
/// <remarks>
/// Supports optional UTF-8 BOM and LF, CRLF or CR terminators. UTF-16/32 files
/// should use the existing TextFilePreviewReader until a dedicated encoding-aware
/// index is added. Does not parse Markdown, wrap terminal columns or highlight code.
/// A fresh temporary index is created for this reader and deleted on disposal.
/// </remarks>
public sealed class Utf8DiskLineIndex : IAsyncDisposable
{
    private const int BufferSize = 64 * 1024;
    private const int MaximumPageLines = 128;
    private const int MaximumPageBytes = 4 * 1024 * 1024;
    private const int DefaultMaximumLineBytes = 1024 * 1024;
    /// <summary>Default maximum temporary offset index (256 MiB).</summary>
    public const long DefaultMaximumIndexBytes = 256L * 1024 * 1024;
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    private readonly FileStream _source;
    private readonly FileStream _offsets;
    private readonly int _maximumLineBytes;
    private readonly SemaphoreSlim _readGate = new(1, 1);
    private bool _disposed;

    private Utf8DiskLineIndex(FileStream source, FileStream offsets,
        long byteLength, long lineCount, int maximumLineBytes)
    {
        _source = source;
        _offsets = offsets;
        _maximumLineBytes = maximumLineBytes;
        ByteLength = byteLength;
        LineCount = lineCount;
    }

    /// <summary>Source file size when indexing completed.</summary>
    public long ByteLength { get; }

    /// <summary>Number of physical lines; no phantom line after a final newline.</summary>
    public long LineCount { get; }

    /// <summary>Number of bytes in the disk-backed offset table.</summary>
    public long IndexBytes => checked((LineCount + 1) * sizeof(long));

    /// <summary>
    /// Validates a complete UTF-8 file and creates a disk-backed offset table.
    /// The file is opened read-only and is never modified. A malformed UTF-8
    /// sequence or terminal control anywhere in the file rejects indexing.
    /// </summary>
    /// <param name="path">Local UTF-8 text file, with or without UTF-8 BOM.</param>
    /// <param name="maximumLineBytes">Per-line read limit (4 KiB to 4 MiB).</param>
    /// <param name="maximumIndexBytes">Maximum temporary index size; default 256 MiB.</param>
    /// <param name="cancellationToken">Cancels indexing and deletes the temporary table.</param>
    public static async Task<Utf8DiskLineIndex> OpenAsync(
        string path, int maximumLineBytes = DefaultMaximumLineBytes,
        long maximumIndexBytes = DefaultMaximumIndexBytes,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (maximumLineBytes is < 4096 or > 4 * 1024 * 1024)
            throw new ArgumentOutOfRangeException(nameof(maximumLineBytes));
        if (maximumIndexBytes < 16)
            throw new ArgumentOutOfRangeException(nameof(maximumIndexBytes));

        var source = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            BufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
        FileStream? offsets = null;
        try
        {
            var indexPath = Path.Combine(Path.GetTempPath(),
                "afrowave-text-index-" + Guid.NewGuid().ToString("N") + ".idx");
            offsets = new FileStream(indexPath, FileMode.CreateNew, FileAccess.ReadWrite,
                FileShare.None, BufferSize, FileOptions.Asynchronous | FileOptions.DeleteOnClose);

            var length = source.Length;
            var lineCount = length == 0 ? 0L : 1L;
            using (var writer = new BinaryWriter(offsets, Encoding.UTF8, leaveOpen: true))
            {
                if (length > 0)
                    writer.Write(0L);

                var bytes = new byte[BufferSize];
                var chars = new char[BufferSize + 4];
                var decoder = StrictUtf8.GetDecoder();
                long processed = 0;
                var pendingCr = false;

                void AddStart(long position)
                {
                    if (position >= length)
                        return;
                    // Include the final sentinel in the on-disk budget.
                    if (checked((lineCount + 2) * sizeof(long)) > maximumIndexBytes)
                        throw new InvalidDataException(
                            "The source contains more lines than the temporary index budget allows.");
                    writer.Write(position);
                    lineCount = checked(lineCount + 1);
                }

                while (processed < length)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var count = await source.ReadAsync(bytes.AsMemory(0,
                        (int)Math.Min(bytes.Length, length - processed)), cancellationToken)
                        .ConfigureAwait(false);
                    if (count == 0)
                        throw new IOException("The source file changed during indexing.");

                    int decoded;
                    try
                    {
                        decoded = decoder.GetChars(bytes, 0, count, chars, 0, flush: false);
                    }
                    catch (DecoderFallbackException ex)
                    {
                        throw new InvalidDataException("Input is not valid UTF-8.", ex);
                    }
                    ValidateControls(chars.AsSpan(0, decoded));

                    for (var i = 0; i < count; i++)
                    {
                        var position = processed + i;
                        var value = bytes[i];
                        if (pendingCr)
                        {
                            pendingCr = false;
                            if (value == 0x0A)
                            {
                                AddStart(position + 1); // CRLF is one newline.
                                continue;
                            }
                            AddStart(position); // Lone CR is one newline.
                        }
                        if (value == 0x0D)
                            pendingCr = true;
                        else if (value == 0x0A)
                            AddStart(position + 1);
                    }

                    processed += count;
                }

                try
                {
                    var endChars = decoder.GetChars(Array.Empty<byte>(), 0, 0,
                        chars, 0, flush: true);
                    ValidateControls(chars.AsSpan(0, endChars));
                }
                catch (DecoderFallbackException ex)
                {
                    throw new InvalidDataException("UTF-8 ends in an incomplete character.", ex);
                }

                if (source.Length != length)
                    throw new IOException("Source size changed while indexing.");

                // Final offset is a sentinel; it is not another line start.
                writer.Write(length);
                writer.Flush();
            }
            await offsets.FlushAsync(cancellationToken).ConfigureAwait(false);
            source.Position = 0;
            offsets.Position = 0;
            return new Utf8DiskLineIndex(source, offsets, length,
                lineCount, maximumLineBytes);
        }
        catch
        {
            if (offsets is not null)
                await offsets.DisposeAsync().ConfigureAwait(false);
            await source.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// Reads up to 128 complete source lines with at most 4 MiB of source bytes
    /// in one page. The page may contain fewer lines than requested if the byte
    /// budget is reached; NextLine makes continuation lossless.
    /// </summary>
    public async Task<Utf8LinePage> ReadPageAsync(
        long firstLine, int maxLines = 40, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (firstLine < 0)
            throw new ArgumentOutOfRangeException(nameof(firstLine));
        if (maxLines is < 1 or > MaximumPageLines)
            throw new ArgumentOutOfRangeException(nameof(maxLines));

        await _readGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (firstLine >= LineCount)
                return new Utf8LinePage(firstLine, LineCount, LineCount, []);

            var count = (int)Math.Min(maxLines, LineCount - firstLine);
            var table = new byte[(count + 1) * sizeof(long)];
            _offsets.Position = checked(firstLine * sizeof(long));
            await _offsets.ReadExactlyAsync(table, cancellationToken).ConfigureAwait(false);

            // Select the largest prefix permitted by the same per-line and
            // page byte budgets before touching the source file. The offset
            // table is already read as one small contiguous range.
            var selected = 0;
            long startByte = -1;
            long pageBytes = 0;
            for (var i = 0; i < count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var start = BinaryPrimitives.ReadInt64LittleEndian(
                    table.AsSpan(i * sizeof(long), sizeof(long)));
                var end = BinaryPrimitives.ReadInt64LittleEndian(
                    table.AsSpan((i + 1) * sizeof(long), sizeof(long)));
                var size = end - start;
                if (start < 0 || size < 0 || end > ByteLength)
                    throw new InvalidDataException("The line index is corrupt.");
                if (size > _maximumLineBytes)
                    throw new InvalidDataException(
                        "A source line exceeds the configured per-line byte limit.");
                if (selected > 0 && pageBytes + size > MaximumPageBytes)
                    break;
                if (selected == 0)
                    startByte = start;
                pageBytes += size;
                selected++;
            }

            // One seek and one contiguous read per page instead of one per
            // physical line. Keep the pool rental below the 4 MiB page cap.
            var lines = new List<string>(selected);
            var buffer = ArrayPool<byte>.Shared.Rent(checked((int)pageBytes));
            try
            {
                _source.Position = startByte;
                await _source.ReadExactlyAsync(
                    buffer.AsMemory(0, checked((int)pageBytes)),
                    cancellationToken).ConfigureAwait(false);

                for (var i = 0; i < selected; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var start = BinaryPrimitives.ReadInt64LittleEndian(
                        table.AsSpan(i * sizeof(long), sizeof(long)));
                    var end = BinaryPrimitives.ReadInt64LittleEndian(
                        table.AsSpan((i + 1) * sizeof(long), sizeof(long)));
                    string line;
                    try
                    {
                        line = StrictUtf8.GetString(buffer,
                            checked((int)(start - startByte)),
                            checked((int)(end - start)));
                    }
                    catch (DecoderFallbackException ex)
                    {
                        throw new InvalidDataException(
                            "Source contents changed or contain invalid UTF-8.", ex);
                    }
                    ValidateControls(line.AsSpan());
                    if (line.EndsWith("\r\n", StringComparison.Ordinal))
                        line = line[..^2];
                    else if (line.EndsWith("\n", StringComparison.Ordinal) ||
                             line.EndsWith("\r", StringComparison.Ordinal))
                        line = line[..^1];
                    if (firstLine == 0 && i == 0 && line.StartsWith('\uFEFF'))
                        line = line[1..];
                    lines.Add(line);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
            if (_source.Length != ByteLength)
                throw new IOException("Source size changed after indexing.");

            return new Utf8LinePage(firstLine, firstLine + lines.Count,
                LineCount, lines);
        }
        finally
        {
            _readGate.Release();
        }
    }

    // Internal batch lookup for coordinated source/UTF-16 index construction.
    // A single offset-table read replaces two random source/index lookups per line.
    internal async Task<long[]> ReadLineStartsAsync(
        long firstLine, int count, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (firstLine < 0 || firstLine > LineCount)
            throw new ArgumentOutOfRangeException(nameof(firstLine));
        if (count < 1 || count > MaximumPageLines ||
            count > LineCount - firstLine)
            throw new ArgumentOutOfRangeException(nameof(count));

        await _readGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var bytes = new byte[checked((count + 1) * sizeof(long))];
            _offsets.Position = checked(firstLine * sizeof(long));
            await _offsets.ReadExactlyAsync(bytes, cancellationToken)
                .ConfigureAwait(false);

            var starts = new long[count + 1];
            for (var i = 0; i < starts.Length; i++)
            {
                starts[i] = BinaryPrimitives.ReadInt64LittleEndian(
                    bytes.AsSpan(i * sizeof(long), sizeof(long)));
                if (starts[i] < 0 || starts[i] > ByteLength ||
                    (i > 0 && starts[i] < starts[i - 1]))
                    throw new InvalidDataException("The line index is corrupt.");
            }
            return starts;
        }
        finally
        {
            _readGate.Release();
        }
    }

    /// <summary>
    /// Maps a zero-based UTF-16 column within a physical source line to its
    /// absolute UTF-8 byte offset. A column must not split a surrogate pair.
    /// The line terminator is excluded; the column at the end of its content
    /// maps to the first byte of the terminator (or end of file).
    /// </summary>
    /// <param name="lineNumber">Zero-based physical line number.</param>
    /// <param name="utf16Column">Zero-based UTF-16 column excluding BOM and terminator.</param>
    /// <param name="cancellationToken">Cancels reading the bounded line and index entry.</param>
    /// <returns>Absolute byte offset in the validated source file.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Line or column is out of range.</exception>
    /// <exception cref="ArgumentException">Column splits a UTF-16 surrogate pair.</exception>
    public async Task<long> GetByteOffsetAsync(
        long lineNumber, int utf16Column, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (lineNumber < 0 || lineNumber >= LineCount)
            throw new ArgumentOutOfRangeException(nameof(lineNumber));
        if (utf16Column < 0)
            throw new ArgumentOutOfRangeException(nameof(utf16Column));

        // Reuse the same bounded decoder and validation as ordinary source pages.
        var page = await ReadPageAsync(lineNumber, 1, cancellationToken)
            .ConfigureAwait(false);
        var line = page.Lines[0];
        if (utf16Column > line.Length)
            throw new ArgumentOutOfRangeException(nameof(utf16Column));
        if (utf16Column > 0 && utf16Column < line.Length &&
            char.IsHighSurrogate(line[utf16Column - 1]) &&
            char.IsLowSurrogate(line[utf16Column]))
            throw new ArgumentException("Column splits a UTF-16 surrogate pair.",
                nameof(utf16Column));

        var prefixBytes = StrictUtf8.GetByteCount(line.AsSpan(0, utf16Column));
        await _readGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var offsetBytes = new byte[sizeof(long)];
            _offsets.Position = checked(lineNumber * sizeof(long));
            await _offsets.ReadExactlyAsync(offsetBytes, cancellationToken)
                .ConfigureAwait(false);
            var start = BinaryPrimitives.ReadInt64LittleEndian(offsetBytes);

            // The first line returned by ReadPageAsync hides the UTF-8 BOM.
            // Account for its three source bytes in the physical position.
            if (lineNumber == 0 && ByteLength >= 3)
            {
                var signature = new byte[3];
                _source.Position = 0;
                await _source.ReadExactlyAsync(signature, cancellationToken)
                    .ConfigureAwait(false);
                if (signature[0] == 0xEF && signature[1] == 0xBB &&
                    signature[2] == 0xBF)
                    start += 3;
            }
            var absolute = checked(start + prefixBytes);
            if (absolute > ByteLength)
                throw new IOException("Source contents changed after indexing.");
            return absolute;
        }
        finally
        {
            _readGate.Release();
        }
    }

    /// <summary>Deletes the temporary index and closes the read-only source.</summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
        await _readGate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_disposed)
                return;
            _disposed = true;
            await _offsets.DisposeAsync().ConfigureAwait(false);
            await _source.DisposeAsync().ConfigureAwait(false);
        }
        finally
        {
            _readGate.Release();
        }
    }

    private static void ValidateControls(ReadOnlySpan<char> text)
    {
        foreach (var character in text)
        {
            if (char.IsControl(character) &&
                character is not '\r' and not '\n' and not '\t')
                throw new InvalidDataException(
                    "File contains unsafe terminal control characters.");
        }
    }
}
