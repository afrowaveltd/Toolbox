using System.Buffers.Binary;
using Afrowave.Toolbox.Blackboard.TextFiles;
using Afrowave.Toolbox.ExchangeHouse.Markdown;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>Source position within a physical Markdown line and its wrapped display rows.</summary>
/// <param name="Line">Zero-based physical source line.</param>
/// <param name="WrapRow">Zero-based visual row within that source line.</param>
public readonly record struct MdViewVisualPosition(long Line, int WrapRow);

/// <summary>
/// Disk-backed exact page starts for a specific terminal geometry.
/// Scans a validated snapshot once and never materializes all page positions.
/// </summary>
/// <remarks>
/// The page count is exact for this deterministic source-line wrapping
/// model, not for arbitrary proportional terminal fonts or the full
/// CommonMark renderer. A changed terminal geometry requires rebuilding
/// these temporary offsets; no stale page geometry is reused.
/// </remarks>
public sealed class MdViewTerminalPageIndex : IAsyncDisposable
{
    /// <summary>Default temporary byte budget for viewport-dependent page starts.</summary>
    public const long DefaultMaximumIndexBytes = 64L * 1024 * 1024;

    /// <summary>Space reserved for gray header, footer and navigation prompt.</summary>
    public const int ReservedTerminalRows = 16;

    private const int RecordBytes = sizeof(long) + sizeof(int);
    private readonly FileStream _stream;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _disposed;

    private MdViewTerminalPageIndex(FileStream stream, long pages, long sourceLines,
        int columns, int rowsPerPage, int terminalWidth, int terminalHeight)
    {
        _stream = stream;
        PageCount = pages;
        SourceLineCount = sourceLines;
        Columns = columns;
        RowsPerPage = rowsPerPage;
        TerminalWidth = terminalWidth;
        TerminalHeight = terminalHeight;
    }

    /// <summary>Number of source-display pages for this viewport.</summary>
    public long PageCount { get; }

    /// <summary>Total physical source lines used to construct the index.</summary>
    public long SourceLineCount { get; }

    /// <summary>Available content columns, excluding a two-column margin.</summary>
    public int Columns { get; }

    /// <summary>Maximum content display rows per page.</summary>
    public int RowsPerPage { get; }

    /// <summary>Raw detected terminal width used for this index.</summary>
    public int TerminalWidth { get; }

    /// <summary>Raw detected terminal height used for this index.</summary>
    public int TerminalHeight { get; }

    /// <summary>Creates page boundaries for the provided current viewport.</summary>
    /// <param name="snapshot">Caller-owned validated immutable snapshot.</param>
    /// <param name="width">Detected terminal width.</param>
    /// <param name="height">Detected terminal height.</param>
    /// <param name="maximumSourceLinesPerPage">Legacy explicit upper bound (1–128).</param>
    /// <param name="maximumIndexBytes">Maximum temporary page-index storage.</param>
    /// <param name="cancellationToken">Cancels the sequential scan.</param>
    /// <returns>Owned disk index to dispose before the source snapshot.</returns>
    public static async Task<MdViewTerminalPageIndex> CreateAsync(
        Utf8ValidatedSnapshot snapshot, int width, int height,
        int maximumSourceLinesPerPage = 24,
        long maximumIndexBytes = DefaultMaximumIndexBytes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));
        if (maximumSourceLinesPerPage is < 1 or > 128)
            throw new ArgumentOutOfRangeException(nameof(maximumSourceLinesPerPage));
        if (maximumIndexBytes < RecordBytes)
            throw new ArgumentOutOfRangeException(nameof(maximumIndexBytes));

        var columns = Math.Clamp(width - 2, 8, 4096);
        var rowsPerPage = Math.Min(maximumSourceLinesPerPage,
            Math.Max(1, height - ReservedTerminalRows));
        var path = Path.Combine(Path.GetTempPath(),
            "afrowave-mdview-visual-pages-" + Guid.NewGuid().ToString("N") + ".idx");
        var stream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite,
            FileShare.None, 65536, FileOptions.Asynchronous | FileOptions.DeleteOnClose);

        try
        {
            using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);
            long pages = 0;
            long lines = 0;
            var usedRows = 0;

            void AddStart(long line, int row)
            {
                if (pages >= maximumIndexBytes / RecordBytes)
                    throw new InvalidDataException(
                        "Terminal page index exceeds its disk budget.");
                writer.Write(line);
                writer.Write(row);
                pages++;
            }

            AddStart(0, 0);
            using var reader = snapshot.OpenReader();
            await foreach (var line in MarkdownIncrementalContainerScanner.ScanAsync(
                reader, cancellationToken: cancellationToken)
                .WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (line.Source.LineNumber != lines)
                    throw new InvalidDataException("Page scanner source line is inconsistent.");
                var code = IsLiteralCode(line);
                var rowCount = MdViewTerminalWordWrap.Wrap(
                    line.Source.Text, columns, code).Count;
                var consumed = 0;
                while (consumed < rowCount)
                {
                    if (usedRows == rowsPerPage)
                    {
                        AddStart(lines, consumed);
                        usedRows = 0;
                    }
                    var take = Math.Min(rowsPerPage - usedRows, rowCount - consumed);
                    consumed += take;
                    usedRows += take;
                }
                lines++;
            }
            if (lines != snapshot.Lines.LineCount)
                throw new InvalidDataException(
                    "Terminal page index differs from validated physical line count.");
            writer.Flush();
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            return new MdViewTerminalPageIndex(
                stream, pages, lines, columns, rowsPerPage, width, height);
        }
        catch
        {
            await stream.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>Gets the exact start of a numbered page without loading other pages.</summary>
    /// <param name="pageNumber">Zero-based page number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Physical line plus within-line visual-row offset.</returns>
    public async Task<MdViewVisualPosition> GetStartAsync(
        long pageNumber, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (pageNumber < 0 || pageNumber >= PageCount)
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return await ReadAtAsync(pageNumber, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Finds the first canonical page containing a source line.</summary>
    /// <param name="lineNumber">Zero-based physical source line.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Zero-based canonical page where that physical line begins.</returns>
    public Task<long> FindPageAsync(
        long lineNumber, CancellationToken cancellationToken = default) =>
        FindPageAsync(new MdViewVisualPosition(lineNumber, 0), cancellationToken);

    /// <summary>Finds the canonical page containing a source line and wrap-row offset.</summary>
    /// <param name="position">Source position within the validated document.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Zero-based canonical page number.</returns>
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
            var low = 0L;
            var high = PageCount;
            while (low < high)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var middle = low + (high - low) / 2;
                var start = await ReadAtAsync(middle, cancellationToken)
                    .ConfigureAwait(false);
                if (start.Line < position.Line ||
                    (start.Line == position.Line &&
                     start.WrapRow <= position.WrapRow))
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

    /// <summary>Releases and deletes the temporary viewport-dependent index.</summary>
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

    /// <summary>True only for source lines that must retain code whitespace.</summary>
    /// <param name="line">Container-classified Markdown source line.</param>
    public static bool IsLiteralCode(MarkdownContainerLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        return line.Kind is MarkdownContainerLineKind.FenceOpen or
            MarkdownContainerLineKind.CodeLine or
            MarkdownContainerLineKind.FenceClose ||
            // Conservative unowned four-space/tab-indented source also
            // represents literal code; explicit list continuations
            // are classified separately and retain their own layout.
            (line.Kind == MarkdownContainerLineKind.Plain &&
             (line.Source.Text.StartsWith("    ", StringComparison.Ordinal) ||
              line.Source.Text.StartsWith('\t')));
    }

    private async Task<MdViewVisualPosition> ReadAtAsync(
        long pageNumber, CancellationToken cancellationToken)
    {
        _stream.Position = checked(pageNumber * RecordBytes);
        var buffer = new byte[RecordBytes];
        await _stream.ReadExactlyAsync(buffer, cancellationToken).ConfigureAwait(false);
        var line = BinaryPrimitives.ReadInt64LittleEndian(buffer.AsSpan(0, sizeof(long)));
        var row = BinaryPrimitives.ReadInt32LittleEndian(
            buffer.AsSpan(sizeof(long), sizeof(int)));
        if (line < 0 || line > SourceLineCount || row < 0)
            throw new InvalidDataException("Viewport page index contains an invalid position.");
        return new MdViewVisualPosition(line, row);
    }
}
