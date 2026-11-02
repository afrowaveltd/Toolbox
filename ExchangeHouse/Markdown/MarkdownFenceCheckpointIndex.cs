using System.Buffers.Binary;
using System.Text;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>
/// A bounded source page, together with its saved fence state and conservatively
/// classified physical lines. No complete Markdown AST is constructed.
/// </summary>
public sealed record MarkdownContextPage(
    MarkdownSourcePage Source,
    MarkdownStreamCheckpoint Checkpoint,
    IReadOnlyList<MarkdownStreamLine> Lines);

/// <summary>
/// Disposable, fixed-record on-disk fence checkpoints for a validated
/// MarkdownDiskPageIndex. Stores no source text, only the state needed to
/// resume recognition of fenced code on an arbitrary physical-line page.
/// </summary>
/// <remarks>
/// This deliberately does not claim CommonMark-conformant full page rendering:
/// list/quote ownership beyond fenced code, paragraph joins and reference links
/// still need additional checkpoint state.
/// </remarks>
public sealed class MarkdownFenceCheckpointIndex : IDisposable, IAsyncDisposable
{
    private const int RecordBytes = 512;
    private const int LanguageMaxBytes = 64;
    private const long DefaultMaxIndexBytes = 256L * 1024 * 1024;
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private readonly MarkdownDiskPageIndex _pages;
    private readonly string _path;
    private readonly int _checkpointStride;
    private int _disposed;

    private MarkdownFenceCheckpointIndex(
        MarkdownDiskPageIndex pages, string path, int checkpointStride)
    {
        _pages = pages;
        _path = path;
        _checkpointStride = checkpointStride;
    }

    /// <summary>Number of pages with independently reconstructable states.</summary>
    public long PageCount => _pages.PageCount;

    /// <summary>Store one checkpoint for every this-many pages, default 16.</summary>
    public int CheckpointStride => _checkpointStride;

    /// <summary>Number of fixed-size checkpoint records on disk.</summary>
    public long SavedCheckpointCount =>
        PageCount == 0 ? 0 : 1 + (PageCount - 1) / _checkpointStride;

    /// <summary>
    /// Scan the source a second time with bounded state and save one 512-byte
    /// checkpoint every checkpointStride pages. Other pages reconstruct their
    /// state by replaying at most checkpointStride-1 source pages.
    /// </summary>
    public static async Task<MarkdownFenceCheckpointIndex> BuildAsync(
        MarkdownDiskPageIndex pages,
        CancellationToken cancellationToken = default,
        string? tempDirectory = null,
        long maxIndexBytes = DefaultMaxIndexBytes,
        int checkpointStride = 16)
    {
        ArgumentNullException.ThrowIfNull(pages);
        if (checkpointStride is < 1 or > 256)
            throw new ArgumentOutOfRangeException(nameof(checkpointStride),
                "Checkpoint stride must be 1..256 pages.");
        if (maxIndexBytes < RecordBytes)
            throw new ArgumentOutOfRangeException(nameof(maxIndexBytes));
        var savedCount = pages.PageCount == 0 ? 0 :
            1 + (pages.PageCount - 1) / checkpointStride;
        if (savedCount > maxIndexBytes / RecordBytes)
            throw new InvalidDataException("Fence checkpoint index exceeds disk quota.");

        var directory = tempDirectory ?? Path.GetTempPath();
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "afrowave-markdown-fences-" +
            Guid.NewGuid().ToString("N") + ".tmp");

        try
        {
            // Verify that the dependent source index is still usable before
            // creating the companion index.
            if (pages.PageCount > 0)
                _ = await pages.ReadPageAsync(0, cancellationToken);

            await using var source = new FileStream(pages.SourcePath,
                FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            var sourceSize = source.Length;
            var lastWrite = File.GetLastWriteTimeUtc(pages.SourcePath);

            await using var output = new FileStream(path, FileMode.CreateNew,
                FileAccess.Write, FileShare.None, 8192, FileOptions.Asynchronous);
            if (!OperatingSystem.IsWindows())
                File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);

            // The source index already proved that the entire document is strict
            // UTF-8. StreamReader consumes only an optional UTF-8 BOM.
            using var reader = new StreamReader(source, StrictUtf8,
                detectEncodingFromByteOrderMarks: true, bufferSize: 8192, leaveOpen: true);

            var state = new MarkdownStreamCheckpoint(0);
            var record = new byte[RecordBytes];
            long lines = 0;
            var options = new MarkdownStreamOptions
            {
                MaxLineChars = checked(pages.MaxLineBytes + 2)
            };

            await foreach (var line in MarkdownStreamingReader.ScanAsync(
                reader, options, cancellationToken))
            {
                if (lines % pages.LinesPerPage == 0 &&
                    (lines / pages.LinesPerPage) % checkpointStride == 0)
                {
                    state = state with { Start = line.Start };
                    Encode(record, state);
                    await output.WriteAsync(record, cancellationToken);
                }

                // Use the SAME bounded state-transition logic as the
                // public line scanner: no independently implemented grammar.
                state = MarkdownStreamingReader.AdvanceCheckpoint(state, line);

                lines = checked(lines + 1);
            }

            if (lines != pages.LineCount ||
                output.Length != checked(savedCount * RecordBytes) ||
                source.Length != sourceSize ||
                File.GetLastWriteTimeUtc(pages.SourcePath) != lastWrite)
                throw new IOException("Source Markdown changed during checkpoint indexing.");

            await output.FlushAsync(cancellationToken);
            return new MarkdownFenceCheckpointIndex(pages, path, checkpointStride);
        }
        catch
        {
            try { File.Delete(path); }
            catch (IOException) { /* Original failure takes precedence. */ }
            catch (UnauthorizedAccessException) { /* Original failure takes precedence. */ }
            throw;
        }
    }

    /// <summary>
    /// Reconstruct the state at a requested page by loading the closest
    /// preceding disk checkpoint, then replaying at most stride-1 pages.
    /// Only one page's text is materialized at a time.
    /// </summary>
    public async Task<MarkdownStreamCheckpoint> ReadCheckpointAsync(
        long pageNumber, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
        if (pageNumber < 0 || pageNumber >= PageCount)
            throw new ArgumentOutOfRangeException(nameof(pageNumber));

        var anchorNumber = pageNumber / _checkpointStride;
        var anchorPage = anchorNumber * _checkpointStride;
        var data = new byte[RecordBytes];
        await using (var index = new FileStream(_path, FileMode.Open,
            FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous))
        {
            index.Seek(checked(anchorNumber * RecordBytes), SeekOrigin.Begin);
            await index.ReadExactlyAsync(data, cancellationToken);
        }

        var state = Decode(data);
        var options = new MarkdownStreamOptions
        {
            MaxLineChars = checked(_pages.MaxLineBytes + 2)
        };
        for (var page = anchorPage; page < pageNumber; page++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var source = await _pages.ReadPageAsync(page, cancellationToken);
            using var reader = new StringReader(source.Content);
            var encountered = 0;
            await foreach (var line in MarkdownStreamingReader.ScanAsync(
                reader, state, options, cancellationToken))
            {
                state = MarkdownStreamingReader.AdvanceCheckpoint(state, line);
                encountered++;
            }
            if (encountered != source.LineCount)
                throw new InvalidDataException(
                    "Sparse checkpoint replay disagrees with source page line count.");
        }

        return state;
    }

    /// <summary>
    /// Load and classify a single source page with its actual opening fence
    /// state, rather than mistakenly treating page-local code as Markdown.
    /// </summary>
    public async Task<MarkdownContextPage> ReadPageAsync(
        long pageNumber, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
        var checkpoint = await ReadCheckpointAsync(pageNumber, cancellationToken);
        var source = await _pages.ReadPageAsync(pageNumber, cancellationToken);
        var lines = new List<MarkdownStreamLine>(source.LineCount);

        using var reader = new StringReader(source.Content);
        await foreach (var line in MarkdownStreamingReader.ScanAsync(reader,
            checkpoint,
            new MarkdownStreamOptions
            {
                MaxLineChars = checked(_pages.MaxLineBytes + 2)
            }, cancellationToken))
        {
            lines.Add(line);
        }

        if (lines.Count != source.LineCount)
            throw new InvalidDataException("Page and fence checkpoint disagree on line boundaries.");

        return new MarkdownContextPage(source, checkpoint, lines);
    }

    private static void Encode(byte[] destination, MarkdownStreamCheckpoint state)
    {
        Array.Clear(destination);
        BinaryPrimitives.WriteInt64LittleEndian(destination.AsSpan(0, 8), state.Start);
        BinaryPrimitives.WriteInt64LittleEndian(destination.AsSpan(8, 8), state.FenceStart);
        BinaryPrimitives.WriteInt32LittleEndian(destination.AsSpan(16, 4), state.FenceLength);
        destination[20] = state.FenceMarker switch
        {
            '\0' => 0,
            '`' => 1,
            '~' => 2,
            _ => throw new InvalidDataException("Invalid checkpoint fence marker.")
        };
        destination[21] = checked((byte)state.FenceQuoteDepth);
        // Preserve a short language hint. Extremely long/unusual info strings
        // intentionally have no hint; code-vs-Markdown classification remains.
        if (state.CodeLanguage is { Length: <= LanguageMaxBytes } language)
        {
            var bytes = StrictUtf8.GetBytes(language);
            if (bytes.Length <= LanguageMaxBytes)
            {
                destination[22] = (byte)bytes.Length;
                bytes.CopyTo(destination.AsSpan(24, LanguageMaxBytes));
            }
        }

        var lists = state.ListIndents;
        if (lists.Count > 32 || state.PreviousQuoteDepth is < 0 or > 32)
            throw new InvalidDataException("Invalid bounded container checkpoint.");
        destination[88] = (byte)lists.Count;
        destination[89] = (byte)state.PreviousQuoteDepth;
        destination[90] = state.PreviousParagraphCandidate ? (byte)1 : (byte)0;
        var owners = state.ListOwners;
        if (owners.Count > 32 || owners.Count != lists.Count)
            throw new InvalidDataException("Invalid list owner checkpoint.");
        destination[91] = (byte)owners.Count;
        destination[92] = state.PreviousOwnedText ? (byte)1 : (byte)0;
        destination[93] = state.PreviousWasBlank ? (byte)1 : (byte)0;
        if (state.PreviousParagraphCandidate != state.OpenParagraphStart.HasValue ||
            (state.OpenParagraphStart is long paragraph &&
                (paragraph < 0 || paragraph >= state.Start)) ||
            (state.ParagraphListOwnerStart is long paragraphOwner &&
                (paragraphOwner < 0 || paragraphOwner >= state.Start ||
                 state.OpenParagraphStart is null)))
            throw new InvalidDataException("Invalid paragraph ownership checkpoint.");
        BinaryPrimitives.WriteInt64LittleEndian(destination.AsSpan(96, 8),
            state.OpenParagraphStart ?? -1);
        BinaryPrimitives.WriteInt64LittleEndian(destination.AsSpan(104, 8),
            state.ParagraphListOwnerStart ?? -1);
        for (var i = 0; i < owners.Count; i++)
        {
            var owner = owners[i];
            if (owner.Start < 0 || owner.Start > state.Start ||
                owner.Indent is < 0 or > 32 ||
                owner.ContentIndent < owner.Indent || owner.ContentIndent > 128 ||
                lists[i] != owner.Indent)
                throw new InvalidDataException("Invalid list owner state.");
            var offset = 128 + 12 * i;
            BinaryPrimitives.WriteInt64LittleEndian(destination.AsSpan(offset, 8),
                owner.Start);
            destination[offset + 8] = (byte)owner.Indent;
            destination[offset + 9] = (byte)owner.ContentIndent;
        }
    }

    private static MarkdownStreamCheckpoint Decode(byte[] record)
    {
        var marker = record[20] switch
        {
            0 => '\0',
            1 => '`',
            2 => '~',
            _ => throw new InvalidDataException("Invalid fence checkpoint marker.")
        };
        var length = record[22];
        if (length > LanguageMaxBytes || record[88] > 32 ||
            record[89] > 32 || record[90] > 1 || record[91] > 32 ||
            record[88] != record[91] || record[92] > 1 || record[93] > 1 ||
            (marker == '\0' && (length != 0 || record[21] != 0 ||
                BinaryPrimitives.ReadInt32LittleEndian(record.AsSpan(16, 4)) != 0)))
            throw new InvalidDataException("Invalid fenced-code checkpoint.");

        var language = length == 0 ? null :
            StrictUtf8.GetString(record, 24, length);
        var paragraphStart = BinaryPrimitives.ReadInt64LittleEndian(
            record.AsSpan(96, 8));
        var paragraphOwner = BinaryPrimitives.ReadInt64LittleEndian(
            record.AsSpan(104, 8));
        var lineStart = BinaryPrimitives.ReadInt64LittleEndian(
            record.AsSpan(0, 8));
        if (paragraphStart < -1 || paragraphStart >= lineStart ||
            paragraphOwner < -1 || paragraphOwner >= lineStart ||
            (paragraphStart == -1 && paragraphOwner != -1) ||
            ((paragraphStart != -1) != (record[90] == 1)))
            throw new InvalidDataException("Invalid paragraph checkpoint.");

        var owners = new MarkdownListOwner[record[91]];
        var lists = new int[record[88]];
        var globalStart = BinaryPrimitives.ReadInt64LittleEndian(record.AsSpan(0, 8));
        for (var i = 0; i < owners.Length; i++)
        {
            var offset = 128 + 12 * i;
            var start = BinaryPrimitives.ReadInt64LittleEndian(record.AsSpan(offset, 8));
            var indent = record[offset + 8];
            var contentIndent = record[offset + 9];
            if (start < 0 || start > globalStart || indent > 32 ||
                contentIndent < indent || contentIndent > 128)
                throw new InvalidDataException("Invalid list owner record.");
            owners[i] = new MarkdownListOwner(start, indent, contentIndent);
            lists[i] = indent;
        }

        return new MarkdownStreamCheckpoint(
            BinaryPrimitives.ReadInt64LittleEndian(record.AsSpan(0, 8)),
            marker,
            BinaryPrimitives.ReadInt32LittleEndian(record.AsSpan(16, 4)),
            BinaryPrimitives.ReadInt64LittleEndian(record.AsSpan(8, 8)),
            record[21], language)
        {
            ListIndents = lists,
            ListOwners = owners,
            PreviousOwnedText = record[92] == 1,
            PreviousWasBlank = record[93] == 1,
            PreviousQuoteDepth = record[89],
            PreviousParagraphCandidate = record[90] == 1,
            OpenParagraphStart = paragraphStart == -1 ? null : paragraphStart,
            ParagraphListOwnerStart = paragraphOwner == -1 ? null : paragraphOwner
        };
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
            File.Delete(_path);
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}
