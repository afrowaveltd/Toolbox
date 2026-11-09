using System.Buffers.Binary;
using Afrowave.Toolbox.Blackboard.TextFiles;
using Afrowave.Toolbox.ExchangeHouse.Markdown;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Optional disk-backed, snapshot-local sparse Markdown scanner checkpoints.
/// Enables bounded replay from the nearest indexed context, without retaining
/// one classified event for every physical source line.
/// </summary>
/// <remarks>
/// Building the index must classify the full validated document once.
/// This is a context index, not a rendering journal or full CommonMark parser.
/// The caller owns the source snapshot and the UTF-16 index.
/// </remarks>
public sealed class MdViewDiskCheckpointIndex : IAsyncDisposable
{
    /// <summary>Default interval between persisted scanner contexts (1,024 lines).</summary>
    public const int DefaultStride = 1024;
    /// <summary>Default combined disk budget for checkpoint data and offsets (256 MiB).</summary>
    public const long DefaultMaximumIndexBytes = 256L * 1024 * 1024;

    private readonly MdViewCheckpointPageReader _pages;
    private readonly FileStream _records;
    private readonly FileStream _offsets;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _disposed;

    private MdViewDiskCheckpointIndex(MdViewCheckpointPageReader pages,
        FileStream records, FileStream offsets, int stride, long checkpointCount,
        long indexedLineCount)
    {
        _pages = pages;
        _records = records;
        _offsets = offsets;
        Stride = stride;
        CheckpointCount = checkpointCount;
        IndexedLineCount = indexedLineCount;
    }

    /// <summary>Source lines between adjacent stored contexts.</summary>
    public int Stride { get; }

    /// <summary>Number of checkpoints stored on disk, including the initial one.</summary>
    public long CheckpointCount { get; }

    /// <summary>Total physical source lines in the validated snapshot.</summary>
    public long IndexedLineCount { get; }

    /// <summary>Combined current checkpoint data and offset bytes.</summary>
    public long IndexBytes => checked(_records.Length + _offsets.Length);

    /// <summary>
    /// Classifies a validated source once and stores bounded, versioned
    /// scanner contexts every <paramref name="stride"/> physical lines.
    /// </summary>
    /// <param name="snapshot">Validated source; ownership remains with caller.</param>
    /// <param name="positions">Document UTF-16 index for this snapshot.</param>
    /// <param name="stride">Checkpoint interval: 1 through 65,536 lines.</param>
    /// <param name="maximumIndexBytes">Combined temporary disk-space budget.</param>
    /// <param name="cancellationToken">Cancels construction and deletes temporary files.</param>
    /// <returns>Independent disk-backed checkpoint index.</returns>
    public static async Task<MdViewDiskCheckpointIndex> CreateAsync(
        Utf8ValidatedSnapshot snapshot, Utf8DocumentOffsetIndex positions,
        int stride = DefaultStride,
        long maximumIndexBytes = DefaultMaximumIndexBytes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(positions);
        if (stride is < 1 or > 65536)
            throw new ArgumentOutOfRangeException(nameof(stride));
        if (maximumIndexBytes < 16)
            throw new ArgumentOutOfRangeException(nameof(maximumIndexBytes));
        cancellationToken.ThrowIfCancellationRequested();

        var records = OpenIndexFile();
        FileStream? offsets = null;
        try
        {
            offsets = OpenIndexFile();
            var pages = new MdViewCheckpointPageReader(snapshot, positions);
            long count = 0;
            long bytes = 0;
            var next = pages.Start;

            async Task StoreAsync(MdViewCheckpointPosition position)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var encoded = MarkdownScannerCheckpointCodec.Encode(position.Checkpoint);
                var cost = checked((long)encoded.Length + sizeof(int) + sizeof(long));
                if (cost > maximumIndexBytes - bytes)
                    throw new InvalidDataException("Scanner checkpoint index exceeds its disk budget.");

                var recordOffset = records.Position;
                var lengthBytes = new byte[sizeof(int)];
                BinaryPrimitives.WriteInt32LittleEndian(lengthBytes, encoded.Length);
                await records.WriteAsync(lengthBytes, cancellationToken).ConfigureAwait(false);
                await records.WriteAsync(encoded, cancellationToken).ConfigureAwait(false);

                var offsetBytes = new byte[sizeof(long)];
                BinaryPrimitives.WriteInt64LittleEndian(offsetBytes, recordOffset);
                await offsets!.WriteAsync(offsetBytes, cancellationToken).ConfigureAwait(false);
                bytes = checked(bytes + cost);
                count++;
            }

            await StoreAsync(next).ConfigureAwait(false);
            var totalLines = snapshot.Lines.LineCount;
            while (next.NextLineNumber < totalLines)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var nextBoundary = Math.Min(
                    totalLines, checked(((next.NextLineNumber / stride) + 1L) * stride));
                while (next.NextLineNumber < nextBoundary)
                {
                    var take = (int)Math.Min(128, nextBoundary - next.NextLineNumber);
                    var page = await pages.ReadAsync(next, take, cancellationToken)
                        .ConfigureAwait(false);
                    if (page.Lines.Count == 0 || page.Next.NextLineNumber <= next.NextLineNumber)
                        throw new InvalidDataException("Checkpoint source did not advance.");
                    next = page.Next;
                }
                if (next.NextLineNumber % stride == 0)
                    await StoreAsync(next).ConfigureAwait(false);
            }

            await records.FlushAsync(cancellationToken).ConfigureAwait(false);
            await offsets.FlushAsync(cancellationToken).ConfigureAwait(false);
            return new MdViewDiskCheckpointIndex(
                pages, records, offsets, stride, count, totalLines);
        }
        catch
        {
            if (offsets is not null)
                await offsets.DisposeAsync().ConfigureAwait(false);
            await records.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// Reads a requested page after restoring the nearest preceding stored
    /// scanner context. Replays at most <see cref="Stride"/> preceding lines.
    /// </summary>
    /// <param name="firstLine">Zero-based physical source line, including EOF.</param>
    /// <param name="maxLines">Requested page size, from 1 to 128 lines.</param>
    /// <param name="cancellationToken">Cancels index access or replay.</param>
    /// <returns>Container-classified page and its next snapshot-bound position.</returns>
    public async Task<MdViewCheckpointPage> ReadAsync(
        long firstLine, int maxLines, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (firstLine < 0 || firstLine > IndexedLineCount)
            throw new ArgumentOutOfRangeException(nameof(firstLine));
        if (maxLines is < 1 or > 128)
            throw new ArgumentOutOfRangeException(nameof(maxLines));

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var index = firstLine / Stride;
            if (index >= CheckpointCount)
                index = CheckpointCount - 1;
            var offsetBuffer = new byte[sizeof(long)];
            _offsets.Position = checked(index * sizeof(long));
            await _offsets.ReadExactlyAsync(offsetBuffer, cancellationToken)
                .ConfigureAwait(false);
            var recordOffset = BinaryPrimitives.ReadInt64LittleEndian(offsetBuffer);
            if (recordOffset < 0 || recordOffset > _records.Length - sizeof(int))
                throw new InvalidDataException("Scanner checkpoint offset is invalid.");
            _records.Position = recordOffset;
            var lengthBuffer = new byte[sizeof(int)];
            await _records.ReadExactlyAsync(lengthBuffer, cancellationToken)
                .ConfigureAwait(false);
            var length = BinaryPrimitives.ReadInt32LittleEndian(lengthBuffer);
            if (length is < 1 or > MarkdownScannerCheckpointCodec.MaximumEncodedBytes ||
                length > _records.Length - recordOffset - sizeof(int))
                throw new InvalidDataException("Scanner checkpoint record size is invalid.");
            var bytes = new byte[length];
            await _records.ReadExactlyAsync(bytes, cancellationToken).ConfigureAwait(false);
            var restored = MarkdownScannerCheckpointCodec.Decode(bytes);
            if (restored.NextLineNumber != index * Stride)
                throw new InvalidDataException("Scanner checkpoint line does not match its slot.");
            var position = _pages.BindCheckpoint(restored);

            while (position.NextLineNumber < firstLine)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var step = (int)Math.Min(128, firstLine - position.NextLineNumber);
                var replay = await _pages.ReadAsync(position, step, cancellationToken)
                    .ConfigureAwait(false);
                if (replay.Lines.Count != step)
                    throw new InvalidDataException("Scanner checkpoint replay ended unexpectedly.");
                position = replay.Next;
            }

            return await _pages.ReadAsync(position, maxLines, cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Closes and deletes both temporary index files; does not dispose the source.</summary>
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
            await _offsets.DisposeAsync().ConfigureAwait(false);
            await _records.DisposeAsync().ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    private static FileStream OpenIndexFile()
    {
        var path = Path.Combine(Path.GetTempPath(),
            "afrowave-mdview-checkpoints-" + Guid.NewGuid().ToString("N") + ".idx");
        return new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite,
            FileShare.None, 65536, FileOptions.Asynchronous | FileOptions.DeleteOnClose);
    }
}
