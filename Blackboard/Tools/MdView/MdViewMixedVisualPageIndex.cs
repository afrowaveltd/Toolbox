using System.Buffers.Binary;
using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Persistent disk record describing one measured mixed Markdown visual page.
/// Both source positions carry the compact-table continuation state.
/// </summary>
/// <param name="Start">Exact source position at the beginning of this page.</param>
/// <param name="Next">First unconsumed source/visual position.</param>
/// <param name="VisualRows">Number of measured content rows on this page.</param>
/// <param name="AtEnd">True only on the final indexed page.</param>
/// <param name="ContainsClippedCode">Whether this page contains horizontally clipped code.</param>
public sealed record MdViewMixedIndexedPage(
    MdViewMixedVisualPosition Start,
    MdViewMixedVisualPosition Next,
    int VisualRows, bool AtEnd, bool ContainsClippedCode);

/// <summary>
/// Disk-backed page-start index for an entire validated Markdown document,
/// combining the existing styled text layout and bounded native table blocks.
/// </summary>
/// <remarks>
/// Each record stores source/wrap/compact-table state at both ends of one
/// visual page, plus measured content rows and flags. The page planner
/// itself is the only source of layout geometry; no second table-height
/// approximation or whole-document in-memory list is maintained.
/// The index is a temporary file deleted when disposed. It does not replace
/// the established compact preview until its separate UI is verified.
/// </remarks>
public sealed class MdViewMixedVisualPageIndex : IAsyncDisposable
{
    /// <summary>Default disk budget for mixed-document page records (64 MiB).</summary>
    public const long DefaultMaximumIndexBytes = 64L * 1024 * 1024;

    /// <summary>
    /// Two source positions (24 bytes each), a 32-bit row count and
    /// a 32-bit flag field, stored in little-endian format.
    /// </summary>
    public const int RecordBytes = 56;

    private const int AtEndFlag = 1;
    private const int ClippedCodeFlag = 2;
    private readonly FileStream _stream;
    private readonly MdViewContainerPageCursor _sourceCursor;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _disposed;

    private MdViewMixedVisualPageIndex(
        FileStream stream, MdViewContainerPageCursor cursor,
        long pageCount, long sourceLineCount, int width, int height,
        int contentRows, bool unicode)
    {
        _stream = stream;
        _sourceCursor = cursor;
        PageCount = pageCount;
        SourceLineCount = sourceLineCount;
        TerminalWidth = width;
        TerminalHeight = height;
        ContentRows = contentRows;
        Unicode = unicode;
    }

    /// <summary>Number of indexed pages; at least one, including an empty file.</summary>
    public long PageCount { get; }

    /// <summary>Total physical source lines in the validated document.</summary>
    public long SourceLineCount { get; }

    /// <summary>Terminal width used for planning and index construction.</summary>
    public int TerminalWidth { get; }

    /// <summary>Terminal height used for planning and index construction.</summary>
    public int TerminalHeight { get; }

    /// <summary>Maximum content display rows allocated to each page.</summary>
    public int ContentRows { get; }

    /// <summary>Whether the measured terminal supported Unicode.</summary>
    public bool Unicode { get; }

    /// <summary>
    /// Builds a complete disk index using the same bounded planner that
    /// will later be used to render individual pages.
    /// </summary>
    /// <param name="cursor">Caller-owned validated and disk-backed source cursor.</param>
    /// <param name="console">Terminal with the viewport and Spectre capabilities.</param>
    /// <param name="contentRows">Content row budget, from 1 to 128.</param>
    /// <param name="maximumIndexBytes">Maximum temporary index bytes.</param>
    /// <param name="cancellationToken">Cancels the scan and index writes.</param>
    /// <returns>Owned disk-backed index that must be disposed.</returns>
    public static async Task<MdViewMixedVisualPageIndex> CreateAsync(
        MdViewContainerPageCursor cursor, IAnsiConsole console,
        int contentRows,
        long maximumIndexBytes = DefaultMaximumIndexBytes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cursor);
        ArgumentNullException.ThrowIfNull(console);
        if (contentRows is < 1 or > 128 ||
            contentRows > Math.Max(1, console.Profile.Height))
            throw new ArgumentOutOfRangeException(nameof(contentRows));
        if (maximumIndexBytes < RecordBytes)
            throw new ArgumentOutOfRangeException(nameof(maximumIndexBytes));
        cancellationToken.ThrowIfCancellationRequested();

        var width = console.Profile.Width;
        var height = console.Profile.Height;
        var unicode = console.Profile.Capabilities.Unicode;
        var path = Path.Combine(Path.GetTempPath(),
            "afrowave-mdview-mixed-pages-" + Guid.NewGuid().ToString("N") + ".idx");
        var stream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite,
            FileShare.None, 65536, FileOptions.Asynchronous | FileOptions.DeleteOnClose);
        try
        {
            using var writer = new BinaryWriter(stream,
                System.Text.Encoding.UTF8, leaveOpen: true);
            long pageCount = 0;
            var position = MdViewMixedVisualPosition.Beginning;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (console.Profile.Width != width ||
                    console.Profile.Height != height ||
                    console.Profile.Capabilities.Unicode != unicode)
                    throw new InvalidOperationException(
                        "Terminal geometry changed during mixed-page indexing.");

                var page = await MdViewMixedVisualPagePlanner.PlanAsync(
                    cursor, position, console, contentRows, cancellationToken)
                    .ConfigureAwait(false);

                if (page.Start != position ||
                    page.TerminalWidth != width ||
                    page.TerminalHeight != height ||
                    page.Unicode != unicode ||
                    page.Rows is < 0 or > 128 ||
                    page.Rows > contentRows ||
                    (page.Rows == 0 && (!page.AtEnd || pageCount != 0)) ||
                    (!page.AtEnd && Compare(page.Next, position) <= 0) ||
                    (page.AtEnd && page.Next.WrapRow != 0))
                    throw new InvalidDataException(
                        "Mixed visual page planner returned invalid progress.");

                if (pageCount >= maximumIndexBytes / RecordBytes)
                    throw new InvalidDataException(
                        "Mixed visual page index exceeds its disk budget.");

                WritePosition(writer, page.Start);
                WritePosition(writer, page.Next);
                writer.Write(page.Rows);
                writer.Write((page.AtEnd ? AtEndFlag : 0) |
                    (page.ContainsClippedCode ? ClippedCodeFlag : 0));
                pageCount++;

                position = page.Next;
                if (page.AtEnd)
                {
                    writer.Flush();
                    await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                    return new MdViewMixedVisualPageIndex(
                        stream, cursor, pageCount, position.Line,
                        width, height, contentRows, unicode);
                }
            }
        }
        catch
        {
            await stream.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>Reads a page record without loading other page records.</summary>
    /// <param name="pageNumber">Zero-based page number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Exact start/end source state and measured page height.</returns>
    public async Task<MdViewMixedIndexedPage> GetPageAsync(
        long pageNumber, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (pageNumber < 0 || pageNumber >= PageCount)
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return await ReadAtAsync(pageNumber, cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Finds the canonical visual page containing the start of one physical
    /// source line, including lines inside a native table or compact block.
    /// </summary>
    /// <param name="sourceLine">Zero-based physical source line.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Zero-based page number.</returns>
    public Task<long> FindPageAsync(
        long sourceLine, CancellationToken cancellationToken = default) =>
        FindPageAsync(new MdViewVisualPosition(sourceLine, 0),
            cancellationToken);

    /// <summary>
    /// Finds the canonical visual page containing the supplied physical
    /// line/soft-wrap row position by binary search on disk page starts.
    /// </summary>
    /// <param name="position">Source line and within-line visual row.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Zero-based canonical page containing the position.</returns>
    public async Task<long> FindPageAsync(
        MdViewVisualPosition position, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (position.Line < 0 || position.Line >= SourceLineCount ||
            position.WrapRow < 0)
            throw new ArgumentOutOfRangeException(nameof(position));

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var low = 0L;
            var high = PageCount;
            while (low < high)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var middle = low + (high - low) / 2;
                var page = await ReadAtAsync(middle, cancellationToken)
                    .ConfigureAwait(false);
                var beginning = new MdViewVisualPosition(
                    page.Start.Line, page.Start.WrapRow);
                if (beginning.Line < position.Line ||
                    (beginning.Line == position.Line &&
                     beginning.WrapRow <= position.WrapRow))
                    low = middle + 1;
                else
                    high = middle;
            }
            return Math.Max(0, low - 1);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Deletes the temporary disk index and releases its resources.</summary>
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
            await _stream.DisposeAsync().ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>True only for the validated source cursor used to build this index.</summary>
    /// <param name="cursor">Source cursor proposed for rendering.</param>
    internal bool UsesSourceCursor(MdViewContainerPageCursor cursor) =>
        ReferenceEquals(_sourceCursor, cursor);

    private static void WritePosition(BinaryWriter writer, MdViewMixedVisualPosition position)
    {
        writer.Write(position.Line);
        writer.Write(position.WrapRow);
        writer.Write(position.CompactHeaderLine);
        writer.Write(position.CompactColumns);
    }

    private static MdViewMixedVisualPosition ReadPosition(ReadOnlySpan<byte> record)
    {
        return new MdViewMixedVisualPosition(
            BinaryPrimitives.ReadInt64LittleEndian(record[..8]),
            BinaryPrimitives.ReadInt32LittleEndian(record.Slice(8, 4)),
            BinaryPrimitives.ReadInt64LittleEndian(record.Slice(12, 8)),
            BinaryPrimitives.ReadInt32LittleEndian(record.Slice(20, 4)));
    }

    private static int Compare(
        MdViewMixedVisualPosition left, MdViewMixedVisualPosition right) =>
        left.Line == right.Line
            ? left.WrapRow.CompareTo(right.WrapRow)
            : left.Line.CompareTo(right.Line);

    private async Task<MdViewMixedIndexedPage> ReadAtAsync(
        long pageNumber, CancellationToken cancellationToken)
    {
        _stream.Position = checked(pageNumber * RecordBytes);
        var buffer = new byte[RecordBytes];
        await _stream.ReadExactlyAsync(buffer, cancellationToken)
            .ConfigureAwait(false);
        var start = ReadPosition(buffer.AsSpan(0, 24));
        var next = ReadPosition(buffer.AsSpan(24, 24));
        var visualRows = BinaryPrimitives.ReadInt32LittleEndian(
            buffer.AsSpan(48, 4));
        var flags = BinaryPrimitives.ReadInt32LittleEndian(
            buffer.AsSpan(52, 4));

        if (start.Line < 0 || start.Line > SourceLineCount ||
            start.WrapRow < 0 ||
            next.Line < 0 || next.Line > SourceLineCount ||
            next.WrapRow < 0 || Compare(next, start) < 0 ||
            (Compare(next, start) == 0 &&
             (flags & AtEndFlag) == 0) ||
            visualRows < 0 || visualRows > ContentRows ||
            (visualRows == 0 && (flags & AtEndFlag) == 0) ||
            (flags & ~(AtEndFlag | ClippedCodeFlag)) != 0 ||
            (((flags & AtEndFlag) != 0) != (pageNumber == PageCount - 1)) ||
            ((flags & AtEndFlag) != 0 &&
             (next.Line != SourceLineCount || next.WrapRow != 0)) ||
            !ValidCompactState(start) || !ValidCompactState(next))
            throw new InvalidDataException(
                "Mixed visual page index contains invalid source positions or row counts.");

        return new MdViewMixedIndexedPage(
            start, next, visualRows,
            (flags & AtEndFlag) != 0, (flags & ClippedCodeFlag) != 0);
    }

    private static bool ValidCompactState(MdViewMixedVisualPosition position) =>
        position.CompactHeaderLine == -1
            ? position.CompactColumns == 0
            : position.CompactHeaderLine >= 0 &&
              position.CompactHeaderLine <= position.Line &&
              position.CompactColumns is >= 2 and <=
                  MdViewTerminalPipeTableLayout.MaximumColumns;
}
