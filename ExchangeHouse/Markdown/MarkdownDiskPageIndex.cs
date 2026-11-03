using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>Bounds for disk-backed raw Markdown source pages.</summary>
public sealed class MarkdownDiskPageOptions
{
    /// <summary>Number of physical lines indexed per page (1..128).</summary>
    public int LinesPerPage { get; init; } = 40;

    /// <summary>Maximum physical line size in UTF-8 bytes (1..1 MiB).</summary>
    public int MaxLineBytes { get; init; } = 64 * 1024;

    /// <summary>Maximum temporary index size (at least 8 bytes, default 256 MiB).</summary>
    public long MaxIndexBytes { get; init; } = 256L * 1024 * 1024;

    internal void Validate()
    {
        if (LinesPerPage is < 1 or > 128 || MaxLineBytes is < 1 or > 1024 * 1024 ||
            MaxIndexBytes < sizeof(long))
            throw new ArgumentOutOfRangeException(nameof(MarkdownDiskPageOptions),
                "LinesPerPage must be 1..128, MaxLineBytes 1..1 MiB and MaxIndexBytes >= 8.");
    }
}

/// <summary>
/// One bounded page of unmodified Markdown source. Byte offsets refer to the
/// original UTF-8 file (including an optional BOM), not UTF-16 characters.
/// This is source paging, NOT a contextual CommonMark document parse.
/// </summary>
public sealed record MarkdownSourcePage(
    long Number, long FirstLine, int LineCount,
    long StartByte, long EndByte, string Content);

/// <summary>
/// Builds an on-disk index of physical UTF-8 Markdown line starts, with one
/// 64-bit offset per page. Neither the source nor the index is loaded into RAM.
/// The index file is temporary and deleted on disposal; the source is never
/// modified. The caller must dispose the pager after use.
/// </summary>
/// <remarks>
/// Only UTF-8 (optionally with UTF-8 BOM) is supported. Strict Unicode and
/// terminal-control validation is performed during indexing. Raw source pages
/// may split a code fence or paragraph: consumers must not assume page-local
/// parsing produces the same AST as whole-document parsing. The source is
/// expected to remain unchanged until the pager is disposed.
/// </remarks>
public sealed class MarkdownDiskPageIndex : IDisposable, IAsyncDisposable
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private readonly string _indexPath;
    private readonly long _sourceSize;
    private readonly DateTime _sourceModifiedUtc;
    private readonly MarkdownDiskPageOptions _options;
    private int _disposed;

    private MarkdownDiskPageIndex(string sourcePath, string indexPath,
        long sourceSize, DateTime sourceModifiedUtc, long lineCount,
        MarkdownDiskPageOptions options)
    {
        SourcePath = sourcePath;
        _indexPath = indexPath;
        _sourceSize = sourceSize;
        _sourceModifiedUtc = sourceModifiedUtc;
        LineCount = lineCount;
        _options = options;
    }

    /// <summary>Absolute original file path; the index never owns the source.</summary>
    public string SourcePath { get; }

    /// <summary>Number of physical lines; a terminal newline does not add a blank line.</summary>
    public long LineCount { get; }

    /// <summary>Number of pages available to seek directly.</summary>
    public long PageCount => (LineCount + _options.LinesPerPage - 1) / _options.LinesPerPage;

    /// <summary>Number of source lines on a full page.</summary>
    public int LinesPerPage => _options.LinesPerPage;

    /// <summary>Maximum validated UTF-8 bytes per source physical line.</summary>
    public int MaxLineBytes => _options.MaxLineBytes;

    /// <summary>Create a bounded-memory page index and strictly validate the entire source.</summary>
    public static async Task<MarkdownDiskPageIndex> BuildAsync(
        string sourcePath,
        MarkdownDiskPageOptions? options = null,
        CancellationToken cancellationToken = default,
        string? tempDirectory = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        options ??= new MarkdownDiskPageOptions();
        options.Validate();

        var fullPath = Path.GetFullPath(sourcePath);
        var directory = tempDirectory ?? Path.GetTempPath();
        Directory.CreateDirectory(directory);
        var indexPath = Path.Combine(directory,
            "afrowave-markdown-index-" + Guid.NewGuid().ToString("N") + ".tmp");

        try
        {
            await using var source = new FileStream(fullPath, FileMode.Open,
                FileAccess.Read, FileShare.Read, 64 * 1024,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            var initialSize = source.Length;
            var modified = File.GetLastWriteTimeUtc(fullPath);

            await using var index = new FileStream(indexPath, FileMode.CreateNew,
                FileAccess.Write, FileShare.None, 8192, FileOptions.Asynchronous);
            if (!OperatingSystem.IsWindows())
                File.SetUnixFileMode(indexPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);

            var offsetBytes = new byte[sizeof(long)];
            async Task WriteOffsetAsync(long value)
            {
                if (index.Position > options.MaxIndexBytes - sizeof(long))
                    throw new InvalidDataException(
                        "The temporary Markdown page index exceeded MaxIndexBytes.");
                BinaryPrimitives.WriteInt64LittleEndian(offsetBytes, value);
                await index.WriteAsync(offsetBytes, cancellationToken);
            }

            if (initialSize > 0)
                await WriteOffsetAsync(0);

            var decoder = StrictUtf8.GetDecoder();
            var bytes = ArrayPool<byte>.Shared.Rent(64 * 1024);
            var chars = ArrayPool<char>.Shared.Rent(64 * 1024 + 4);

            long position = 0;
            long lineStart = 0;
            long completeLines = 0;
            bool pendingCr = false;
            try
            {
                async Task CompleteLineAsync(long nextStart)
                {
                    if (nextStart - lineStart > (long)options.MaxLineBytes + 2)
                        throw new InvalidDataException("A Markdown physical line exceeds MaxLineBytes.");
                    lineStart = nextStart;
                    completeLines = checked(completeLines + 1);
                    if (completeLines % options.LinesPerPage == 0)
                        await WriteOffsetAsync(nextStart);
                }

                int read;
                while ((read = await source.ReadAsync(
                    bytes.AsMemory(0, 64 * 1024), cancellationToken)) > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // The decoder remembers a multi-byte sequence crossing chunks,
                    // and rejects malformed or truncated UTF-8, including C1 controls.
                    var charCount = decoder.GetChars(bytes, 0, read, chars, 0, flush: false);
                    ValidateCharacters(chars.AsSpan(0, charCount));

                    for (var i = 0; i < read; i++)
                    {
                        var value = bytes[i];
                        if (pendingCr)
                        {
                            pendingCr = false;
                            if (value == (byte)'\n')
                            {
                                position++;
                                await CompleteLineAsync(position);
                                continue;
                            }
                            await CompleteLineAsync(position);
                        }

                        position++;
                        if (value == (byte)'\r')
                            pendingCr = true;
                        else if (value == (byte)'\n')
                            await CompleteLineAsync(position);
                        else if (position - lineStart > options.MaxLineBytes)
                            throw new InvalidDataException(
                                "A Markdown physical line exceeds MaxLineBytes.");
                    }
                }

                var finalCount = decoder.GetChars(
                    Array.Empty<byte>(), 0, 0, chars, 0, flush: true);
                ValidateCharacters(chars.AsSpan(0, finalCount));

                if (pendingCr)
                    await CompleteLineAsync(position);
                else if (position > lineStart)
                    completeLines = checked(completeLines + 1);

                // A UTF-8 BOM without content is an empty logical document,
                // just like the existing complete-file viewer. Do not create
                // a phantom page that the BOM-skipping scanner cannot replay.
                if (position == 3 && completeLines == 1)
                {
                    source.Position = 0;
                    var bom = new byte[3];
                    await source.ReadExactlyAsync(bom, cancellationToken);
                    if (bom.AsSpan().SequenceEqual(new byte[] { 0xEF, 0xBB, 0xBF }))
                        completeLines = 0;
                }

                // A page boundary coinciding with EOF is NOT an empty extra page.
                var actualPages = (completeLines + options.LinesPerPage - 1) /
                    options.LinesPerPage;
                index.SetLength(checked(actualPages * sizeof(long)));
                await index.FlushAsync(cancellationToken);

                if (position != initialSize ||
                    File.GetLastWriteTimeUtc(fullPath) != modified)
                    throw new IOException("Source Markdown changed during indexing.");

                return new MarkdownDiskPageIndex(fullPath, indexPath,
                    position, modified, completeLines, options);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(bytes);
                ArrayPool<char>.Shared.Return(chars);
            }
        }
        catch
        {
            try { File.Delete(indexPath); }
            catch (IOException) { /* Preserve the original failure. */ }
            catch (UnauthorizedAccessException) { /* Preserve the original failure. */ }
            throw;
        }
    }

    /// <summary>
    /// Read an arbitrary source page using at most LinesPerPage * (MaxLineBytes+2)
    /// bytes. Reading pages does not change a shared file cursor.
    /// </summary>
    public async Task<MarkdownSourcePage> ReadPageAsync(
        long number, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
        if (number < 0 || number >= PageCount)
            throw new ArgumentOutOfRangeException(nameof(number));

        var offsetsToRead = number + 1 < PageCount ? 16 : 8;
        var offsetData = new byte[offsetsToRead];
        await using (var index = new FileStream(_indexPath, FileMode.Open,
            FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous))
        {
            index.Seek(checked(number * sizeof(long)), SeekOrigin.Begin);
            await index.ReadExactlyAsync(offsetData, cancellationToken);
        }

        var start = BinaryPrimitives.ReadInt64LittleEndian(offsetData);
        var end = offsetsToRead == 16 ?
            BinaryPrimitives.ReadInt64LittleEndian(offsetData.AsSpan(8)) :
            _sourceSize;

        if (start < 0 || end < start || end > _sourceSize ||
            end - start > (long)_options.LinesPerPage * (_options.MaxLineBytes + 2))
            throw new InvalidDataException("Invalid Markdown page index.");

        await using var source = new FileStream(SourcePath, FileMode.Open,
            FileAccess.Read, FileShare.Read, 64 * 1024,
            FileOptions.Asynchronous | FileOptions.RandomAccess);

        if (source.Length != _sourceSize ||
            File.GetLastWriteTimeUtc(SourcePath) != _sourceModifiedUtc)
            throw new IOException("Source Markdown changed since indexing.");

        source.Seek(start, SeekOrigin.Begin);
        var contentBytes = new byte[checked((int)(end - start))];
        await source.ReadExactlyAsync(contentBytes, cancellationToken);
        var text = StrictUtf8.GetString(contentBytes);
        if (number == 0 && text.StartsWith('\uFEFF'))
            text = text[1..];

        return new MarkdownSourcePage(number, checked(number * _options.LinesPerPage + 1),
            checked((int)Math.Min(_options.LinesPerPage,
                LineCount - number * _options.LinesPerPage)),
            start, end, text);
    }

    private static void ValidateCharacters(ReadOnlySpan<char> characters)
    {
        foreach (var character in characters)
            if (char.IsControl(character) && character is not '\r' and not '\n' and not '\t')
                throw new InvalidDataException(
                    "Markdown source contains forbidden terminal control characters.");
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
            File.Delete(_indexPath);
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}
