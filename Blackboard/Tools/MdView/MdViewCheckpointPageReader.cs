using Afrowave.Toolbox.Blackboard.TextFiles;
using Afrowave.Toolbox.ExchangeHouse.Markdown;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Opaque resume point owned by a single validated-snapshot checkpoint reader.
/// It binds a scanner checkpoint to its source without exposing a reusable
/// untrusted checkpoint-to-file association.
/// </summary>
public sealed class MdViewCheckpointPosition
{
    internal MdViewCheckpointPosition(
        MdViewCheckpointPageReader owner, MarkdownScannerCheckpoint checkpoint)
    {
        Owner = owner;
        Checkpoint = checkpoint;
    }

    internal MdViewCheckpointPageReader Owner { get; }
    internal MarkdownScannerCheckpoint Checkpoint { get; }

    /// <summary>Next zero-based physical source line to classify.</summary>
    public long NextLineNumber => Checkpoint.NextLineNumber;

    /// <summary>Next decoded document UTF-16 source offset, excluding the BOM.</summary>
    public long NextUtf16Offset => Checkpoint.NextUtf16Offset;
}

/// <summary>
/// One independently resumed, bounded Markdown container page and its next
/// checkpoint. This opt-in reader does not replace the existing disk journal.
/// </summary>
/// <param name="Lines">Classified source lines, up to the requested page size.</param>
/// <param name="Next">Snapshot-bound position following the returned lines.</param>
public sealed record MdViewCheckpointPage(
    IReadOnlyList<MarkdownContainerLine> Lines, MdViewCheckpointPosition Next);

/// <summary>
/// Restarts Markdown container classification directly from a validated snapshot
/// and a snapshot-bound scanner checkpoint, without replaying earlier lines.
/// </summary>
/// <remarks>
/// The caller owns and must outlive this reader's validated snapshot and
/// UTF-16 index. Checkpoints are created only by this instance. No claim is
/// made that an arbitrary unseen line is reachable without first processing
/// the preceding context or obtaining a trusted checkpoint for it.
/// </remarks>
public sealed class MdViewCheckpointPageReader
{
    private readonly Utf8ValidatedSnapshot _snapshot;
    private readonly Utf8DocumentOffsetIndex _positions;

    /// <summary>Creates a reader on a snapshot and its corresponding UTF-16 index.</summary>
    /// <param name="snapshot">Validated immutable-by-convention source copy.</param>
    /// <param name="positions">UTF-16 index created from snapshot.Lines.</param>
    public MdViewCheckpointPageReader(
        Utf8ValidatedSnapshot snapshot, Utf8DocumentOffsetIndex positions)
    {
        _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        _positions = positions ?? throw new ArgumentNullException(nameof(positions));
        Start = new MdViewCheckpointPosition(
            this, new MarkdownIncrementalContainerScanner.StateMachine().Capture());
    }

    /// <summary>Initial source-bound checkpoint before the first physical line.</summary>
    public MdViewCheckpointPosition Start { get; }

    // Only trusted checkpoint stores constructed for this exact reader may
    // rebind serialized parser state. ReadAsync independently verifies its
    // UTF-16 and byte alignment against the validated snapshot.
    internal MdViewCheckpointPosition BindCheckpoint(MarkdownScannerCheckpoint checkpoint)
    {
        MarkdownScannerCheckpointCodec.Validate(checkpoint);
        return new MdViewCheckpointPosition(this, checkpoint);
    }

    /// <summary>
    /// Classifies up to 128 physical lines from a checkpoint without replaying
    /// previous lines. The next checkpoint can be reused for future navigation.
    /// </summary>
    /// <param name="position">A checkpoint returned by this reader.</param>
    /// <param name="maxLines">Maximum number of source lines, from 1 to 128.</param>
    /// <param name="cancellationToken">Cancels source mapping and classification.</param>
    /// <returns>Classified lines and the next snapshot-bound checkpoint.</returns>
    public async Task<MdViewCheckpointPage> ReadAsync(
        MdViewCheckpointPosition position, int maxLines,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(position);
        if (!ReferenceEquals(position.Owner, this))
            throw new ArgumentException(
                "Checkpoint belongs to a different source reader.", nameof(position));
        if (maxLines is < 1 or > 128)
            throw new ArgumentOutOfRangeException(nameof(maxLines));
        cancellationToken.ThrowIfCancellationRequested();

        var checkpoint = position.Checkpoint;
        var lineNumber = checkpoint.NextLineNumber;
        if (lineNumber > _snapshot.Lines.LineCount ||
            checkpoint.NextUtf16Offset > _positions.Utf16Length)
            throw new InvalidDataException("Checkpoint exceeds the validated source.");

        var byteOffset = await _positions.GetByteOffsetAsync(
            checkpoint.NextUtf16Offset, cancellationToken).ConfigureAwait(false);
        var expectedByteOffset = lineNumber == _snapshot.Lines.LineCount
            ? _snapshot.ByteLength
            : await _snapshot.Lines.GetByteOffsetAsync(
                lineNumber, 0, cancellationToken).ConfigureAwait(false);

        if (byteOffset != expectedByteOffset)
            throw new InvalidDataException(
                "Checkpoint does not align with a validated physical line.");

        var state = MarkdownIncrementalContainerScanner.StateMachine.Restore(checkpoint);
        var output = new List<MarkdownContainerLine>(maxLines);
        if (lineNumber < _snapshot.Lines.LineCount)
        {
            using var reader = _snapshot.OpenReaderAtByteOffset(byteOffset);
            await foreach (var line in MarkdownIncrementalLineScanner.ScanAsync(
                reader, cancellationToken: cancellationToken)
                .WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var absolute = line with
                {
                    LineNumber = checked(line.LineNumber + checkpoint.NextLineNumber),
                    Utf16Offset = checked(line.Utf16Offset + checkpoint.NextUtf16Offset)
                };
                output.Add(state.Process(absolute));
                if (output.Count == maxLines)
                    break;
            }
        }

        var next = new MdViewCheckpointPosition(this, state.Capture());
        return new MdViewCheckpointPage(output, next);
    }
}
