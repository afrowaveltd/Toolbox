using System.Buffers.Binary;
using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// One measured native table fragment stored as a fixed-size disk record.
/// Its source interval contains data rows only; the source header is
/// repeated visually and included in the measured visual row count.
/// </summary>
/// <param name="FirstDataLine">Inclusive zero-based source data line.</param>
/// <param name="NextDataLine">Exclusive first unconsumed source data line.</param>
/// <param name="DataRows">Count of consumed physical data lines (0–6).</param>
/// <param name="VisualRows">Measured native Spectre table height, including its frame and header.</param>
/// <param name="HasMore">Whether a matching source data line immediately follows.</param>
public sealed record MdViewNativeTableFragment(
    long FirstDataLine, long NextDataLine,
    int DataRows, int VisualRows, bool HasMore);

/// <summary>
/// Disk-backed, source-bound index for one Markdown table's measured native
/// fragments. The validated source snapshot and its cursor remain caller-owned.
/// </summary>
/// <remarks>
/// This indexes only a confirmed single table, not a complete mixed Markdown
/// document. Each fixed-size record is stored in a temporary file that is
/// deleted on disposal. A fragment is reconstructed from the same planner
/// when eventually rendered; no native widget is kept in the index's RAM.
/// On any unsafe fragment geometry, the entire table requests the existing
/// compact fallback, rather than returning a partially indexed native table.
/// </remarks>
public sealed class MdViewNativeTableFragmentIndex : IAsyncDisposable
{
    /// <summary>Maximum temporary bytes allocated for fragment records.</summary>
    public const long DefaultMaximumIndexBytes = 64L * 1024 * 1024;

    /// <summary>Bytes in one indexed fragment: two longs and three ints.</summary>
    public const int RecordBytes = 2 * sizeof(long) + 3 * sizeof(int);

    private readonly FileStream _stream;
    private readonly MdViewContainerPageCursor _sourceCursor;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _disposed;

    private MdViewNativeTableFragmentIndex(
        FileStream stream, MdViewContainerPageCursor cursor,
        long headerLine, long count, long sourceRows,
        long nextDataLine, int terminalWidth, int terminalHeight,
        bool unicode, int contentRows,
        MdViewNativeTableFallbackReason fallbackReason)
    {
        _stream = stream;
        _sourceCursor = cursor;
        HeaderLine = headerLine;
        FragmentCount = count;
        SourceDataRows = sourceRows;
        NextDataLine = nextDataLine;
        TerminalWidth = terminalWidth;
        TerminalHeight = terminalHeight;
        Unicode = unicode;
        ContentRows = contentRows;
        FallbackReason = fallbackReason;
    }

    /// <summary>Physical source line containing the validated table header.</summary>
    public long HeaderLine { get; }

    /// <summary>Number of native fragments indexed on disk, zero on fallback.</summary>
    public long FragmentCount { get; }

    /// <summary>Number of data source lines covered by the native fragments.</summary>
    public long SourceDataRows { get; }

    /// <summary>
    /// First unconsumed source data line; header + 2 if compact fallback
    /// was requested before any table data was accepted.
    /// </summary>
    public long NextDataLine { get; }

    /// <summary>Terminal columns used for exact Spectre measurement.</summary>
    public int TerminalWidth { get; }

    /// <summary>Terminal height used when establishing the index geometry.</summary>
    public int TerminalHeight { get; }

    /// <summary>Whether the source Spectre terminal supported Unicode.</summary>
    public bool Unicode { get; }

    /// <summary>Fixed available visual row budget for each fragment.</summary>
    public int ContentRows { get; }

    /// <summary>
    /// Entire-table compact fallback reason, or None when a complete native
    /// table fragment index was constructed.
    /// </summary>
    public MdViewNativeTableFallbackReason FallbackReason { get; }

    /// <summary>True only when every source data row is indexed natively.</summary>
    public bool IsNative => FallbackReason == MdViewNativeTableFallbackReason.None;

    /// <summary>
    /// Builds a bounded disk index by measuring successive source fragments.
    /// Source is always read through the previously validated cursor.
    /// </summary>
    /// <param name="cursor">Caller-owned disk-backed Markdown source cursor.</param>
    /// <param name="header">Previously validated source-bound table header.</param>
    /// <param name="console">Spectre console with the intended viewport profile.</param>
    /// <param name="contentRows">Page content height from 1 to 128 rows.</param>
    /// <param name="maximumIndexBytes">Disk budget, at least one record.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Disposable disk index or an explicit entire-table fallback.</returns>
    public static async Task<MdViewNativeTableFragmentIndex> CreateAsync(
        MdViewContainerPageCursor cursor,
        MdViewSpectreTableHeader header,
        IAnsiConsole console,
        int contentRows,
        long maximumIndexBytes = DefaultMaximumIndexBytes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cursor);
        ArgumentNullException.ThrowIfNull(header);
        ArgumentNullException.ThrowIfNull(console);
        if (contentRows is < 1 or > 128)
            throw new ArgumentOutOfRangeException(nameof(contentRows));
        if (maximumIndexBytes < RecordBytes)
            throw new ArgumentOutOfRangeException(nameof(maximumIndexBytes));
        if (contentRows > Math.Max(1, console.Profile.Height))
            throw new ArgumentOutOfRangeException(nameof(contentRows),
                "Visual content rows cannot exceed terminal height.");
        cancellationToken.ThrowIfCancellationRequested();

        // Do not rely on a caller-supplied header that has become stale or
        // was constructed with unrelated column labels/delimiters.
        var actual = await MdViewSpectreTableBlockReader.ReadHeaderAsync(
            cursor, header.SourceLine, cancellationToken).ConfigureAwait(false);
        if (actual is null ||
            !actual.Columns.SequenceEqual(header.Columns) ||
            !actual.Separators.SequenceEqual(header.Separators))
            throw new InvalidDataException(
                "Native table header no longer matches the validated source.");

        var path = Path.Combine(Path.GetTempPath(),
            "afrowave-mdview-native-table-" + Guid.NewGuid().ToString("N") + ".idx");
        var stream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite,
            FileShare.None, 65536, FileOptions.Asynchronous | FileOptions.DeleteOnClose);
        try
        {
            using var writer = new BinaryWriter(stream,
                System.Text.Encoding.UTF8, leaveOpen: true);
            var firstDataLine = checked(header.SourceLine + 2);
            var nextDataLine = firstDataLine;
            var totalDataRows = 0L;
            var fragmentCount = 0L;
            var fallback = MdViewNativeTableFallbackReason.None;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var chunk = await MdViewNativeTablePagePlanner.PlanAsync(
                    cursor, actual, nextDataLine, console, contentRows,
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                if (!chunk.IsNative)
                {
                    fallback = chunk.FallbackReason;
                    // Never expose a partially native table. Fall back the
                    // *whole* source block to the existing compact reader.
                    stream.SetLength(0);
                    fragmentCount = 0;
                    totalDataRows = 0;
                    nextDataLine = firstDataLine;
                    break;
                }
                if (chunk.NextDataLine < nextDataLine ||
                    chunk.NextDataLine - nextDataLine != chunk.DataRows ||
                    (chunk.HasMore && chunk.DataRows == 0) ||
                    chunk.VisualRows > contentRows || chunk.VisualRows < 1)
                    throw new InvalidDataException(
                        "Native table fragment has invalid source or visual geometry.");

                if (fragmentCount >= maximumIndexBytes / RecordBytes)
                    throw new InvalidDataException(
                        "Native table fragment index exceeds its disk budget.");

                writer.Write(nextDataLine);
                writer.Write(chunk.NextDataLine);
                writer.Write(chunk.DataRows);
                writer.Write(chunk.VisualRows);
                writer.Write(chunk.HasMore ? 1 : 0);

                fragmentCount++;
                totalDataRows = checked(totalDataRows + chunk.DataRows);
                nextDataLine = chunk.NextDataLine;
                if (!chunk.HasMore)
                    break;
            }

            writer.Flush();
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            return new MdViewNativeTableFragmentIndex(
                stream, cursor, header.SourceLine, fragmentCount, totalDataRows,
                nextDataLine, console.Profile.Width, console.Profile.Height,
                console.Profile.Capabilities.Unicode, contentRows, fallback);
        }
        catch
        {
            await stream.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>Reads exactly one indexed fragment without loading its neighbors.</summary>
    /// <param name="fragmentNumber">Zero-based fragment number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Measured native fragment source coordinates and visual height.</returns>
    public async Task<MdViewNativeTableFragment> GetFragmentAsync(
        long fragmentNumber, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (fragmentNumber < 0 || fragmentNumber >= FragmentCount)
            throw new ArgumentOutOfRangeException(nameof(fragmentNumber));

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return await ReadAtAsync(fragmentNumber, cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Finds the native fragment containing an original physical source
    /// line. The table header and delimiter map to fragment zero.
    /// </summary>
    /// <param name="sourceLine">Zero-based original Markdown source line.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Matching zero-based fragment index, or null beyond the table.</returns>
    public async Task<long?> FindFragmentAsync(
        long sourceLine, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (sourceLine < 0)
            throw new ArgumentOutOfRangeException(nameof(sourceLine));
        if (FragmentCount == 0 || sourceLine < HeaderLine ||
            sourceLine >= NextDataLine)
            return null;
        if (sourceLine < HeaderLine + 2)
            return 0;

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var low = 0L;
            var high = FragmentCount;
            while (low < high)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var mid = low + (high - low) / 2;
                var fragment = await ReadAtAsync(mid, cancellationToken)
                    .ConfigureAwait(false);
                if (fragment.NextDataLine <= sourceLine)
                    low = mid + 1;
                else
                    high = mid;
            }
            if (low >= FragmentCount)
                return null;
            var candidate = await ReadAtAsync(low, cancellationToken)
                .ConfigureAwait(false);
            return candidate.FirstDataLine <= sourceLine &&
                sourceLine < candidate.NextDataLine ? low : null;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Confirms that rendering is using the exact validated source cursor
    /// from which this temporary index was built.
    /// </summary>
    /// <param name="cursor">Cursor proposed for fragment reconstruction.</param>
    /// <returns>True only for the original caller-owned source cursor.</returns>
    internal bool UsesSourceCursor(MdViewContainerPageCursor cursor) =>
        ReferenceEquals(_sourceCursor, cursor);

    /// <summary>Releases and deletes the temporary fragment index.</summary>
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

    private async Task<MdViewNativeTableFragment> ReadAtAsync(
        long number, CancellationToken cancellationToken)
    {
        _stream.Position = checked(number * RecordBytes);
        var bytes = new byte[RecordBytes];
        await _stream.ReadExactlyAsync(bytes, cancellationToken)
            .ConfigureAwait(false);
        var first = BinaryPrimitives.ReadInt64LittleEndian(bytes.AsSpan(0, 8));
        var next = BinaryPrimitives.ReadInt64LittleEndian(bytes.AsSpan(8, 8));
        var dataRows = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(16, 4));
        var visualRows = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(20, 4));
        var hasMore = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(24, 4));
        if (first < HeaderLine + 2 || next < first ||
            next > NextDataLine || next - first != dataRows ||
            dataRows is < 0 or > MdViewNativeTablePagePlanner.MaximumSourceRows ||
            visualRows is < 1 or > 128 || visualRows > ContentRows ||
            hasMore is < 0 or > 1)
            throw new InvalidDataException(
                "Native table fragment index contains invalid coordinates.");
        return new MdViewNativeTableFragment(
            first, next, dataRows, visualRows, hasMore != 0);
    }
}
