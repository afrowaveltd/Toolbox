using System.Text.Json;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>
/// One immutable list stack frame stored in a scanner checkpoint.
/// </summary>
/// <param name="MarkerIndent">Indentation of the explicit list marker, in columns.</param>
/// <param name="ContentIndent">Required content indentation, in columns.</param>
/// <param name="StartUtf16Offset">Document-absolute start offset of the list marker.</param>
/// <param name="Marker">Exact list marker text.</param>
public sealed record MarkdownScannerListFrame(
    int MarkerIndent, int ContentIndent, long StartUtf16Offset, string Marker);

/// <summary>
/// Active fenced-code block context at a scanner checkpoint.
/// </summary>
/// <param name="Marker">Backtick or tilde opening character.</param>
/// <param name="Length">Minimum delimiter length.</param>
/// <param name="QuoteDepth">Owning quote depth.</param>
/// <param name="StartUtf16Offset">Opening delimiter's document offset.</param>
/// <param name="Language">Optional code language.</param>
/// <param name="ListDepth">Owning list depth, or -1.</param>
/// <param name="ListOwnerUtf16Offset">Owning list start, if any.</param>
/// <param name="ContentIndent">Content indentation of the owning list.</param>
/// <param name="OuterListOwnerUtf16Offset">Ordinary outer-list owner of a quote fence.</param>
/// <param name="OuterListDepth">Depth of that outer-list owner, or -1.</param>
public sealed record MarkdownScannerFenceState(
    char Marker, int Length, int QuoteDepth, long StartUtf16Offset,
    string? Language, int ListDepth, long? ListOwnerUtf16Offset,
    int ContentIndent, long? OuterListOwnerUtf16Offset, int OuterListDepth);

/// <summary>
/// Serializable, bounded checkpoint contract for the incremental container scanner.
/// This is a data contract only: scanner export/restore hooks are a separate stage.
/// </summary>
/// <param name="Version">Checkpoint schema version; currently 1.</param>
/// <param name="NextLineNumber">Next zero-based physical line to process.</param>
/// <param name="NextUtf16Offset">Next decoded document UTF-16 offset (BOM excluded).</param>
/// <param name="OrdinaryLists">Ordered list ownership stack.</param>
/// <param name="QuoteLists">Quoted list ownership stack.</param>
/// <param name="CurrentQuoteDepth">Current explicit quote depth.</param>
/// <param name="CurrentOuterQuoteOwnerUtf16Offset">Current outer ordinary-list owner.</param>
/// <param name="OrdinaryBlankPending">Whether an ordinary list blank separator is unresolved.</param>
/// <param name="QuoteBlankPending">Whether a quoted list blank separator is unresolved.</param>
/// <param name="Fence">Active fenced-code block context, if any.</param>
public sealed record MarkdownScannerCheckpoint(
    int Version,
    long NextLineNumber,
    long NextUtf16Offset,
    MarkdownScannerListFrame[] OrdinaryLists,
    MarkdownScannerListFrame[] QuoteLists,
    int CurrentQuoteDepth,
    long? CurrentOuterQuoteOwnerUtf16Offset,
    bool OrdinaryBlankPending,
    bool QuoteBlankPending,
    MarkdownScannerFenceState? Fence);

/// <summary>
/// Strict, versioned JSON encoding for a future restorable scanner state.
/// Rejects malformed or unbounded external checkpoint data before use.
/// </summary>
public static class MarkdownScannerCheckpointCodec
{
    /// <summary>The only currently supported checkpoint schema version.</summary>
    public const int CurrentVersion = 1;
    /// <summary>Maximum encoded checkpoint size, 32 KiB.</summary>
    public const int MaximumEncodedBytes = 32 * 1024;
    private const int MaximumDepth = 32;

    /// <summary>Serializes a validated checkpoint to bounded UTF-8 JSON.</summary>
    /// <param name="checkpoint">Complete scanner state.</param>
    /// <returns>Encoded JSON bytes.</returns>
    public static byte[] Encode(MarkdownScannerCheckpoint checkpoint)
    {
        Validate(checkpoint);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(checkpoint);
        if (bytes.Length > MaximumEncodedBytes)
            throw new InvalidDataException("Scanner checkpoint exceeds the encoded size limit.");
        return bytes;
    }

    /// <summary>Deserializes and validates untrusted UTF-8 JSON checkpoint data.</summary>
    /// <param name="data">JSON document bytes.</param>
    /// <returns>Validated checkpoint value.</returns>
    public static MarkdownScannerCheckpoint Decode(ReadOnlySpan<byte> data)
    {
        if (data.Length is < 1 or > MaximumEncodedBytes)
            throw new InvalidDataException("Scanner checkpoint has invalid byte length.");
        try
        {
            var checkpoint = JsonSerializer.Deserialize<MarkdownScannerCheckpoint>(data);
            if (checkpoint is null)
                throw new InvalidDataException("Scanner checkpoint is null.");
            Validate(checkpoint);
            return checkpoint;
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("Scanner checkpoint JSON is invalid.", ex);
        }
    }

    /// <summary>Validates all bounded fields and internal owner references.</summary>
    /// <param name="value">Checkpoint to validate.</param>
    public static void Validate(MarkdownScannerCheckpoint value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Version != CurrentVersion ||
            value.NextLineNumber < 0 || value.NextUtf16Offset < 0 ||
            value.CurrentQuoteDepth is < 0 or > MaximumDepth ||
            !ValidOwner(value.CurrentOuterQuoteOwnerUtf16Offset, value.NextUtf16Offset))
            throw new InvalidDataException("Scanner checkpoint header is invalid.");

        ValidateStack(value.OrdinaryLists, value.NextUtf16Offset);
        ValidateStack(value.QuoteLists, value.NextUtf16Offset);
        if (value.CurrentQuoteDepth == 0 &&
            (value.QuoteLists.Length != 0 || value.CurrentOuterQuoteOwnerUtf16Offset is not null))
            throw new InvalidDataException("Unquoted checkpoint contains quoted ownership.");
        if (value.Fence is not { } fence)
            return;

        if (fence.Marker is not ('`' or '~') || fence.Length is < 3 or > 1024 ||
            fence.QuoteDepth is < 0 or > MaximumDepth ||
            fence.StartUtf16Offset < 0 || fence.StartUtf16Offset > value.NextUtf16Offset ||
            fence.ListDepth is < -1 or >= MaximumDepth ||
            fence.OuterListDepth is < -1 or >= MaximumDepth ||
            fence.ContentIndent is < 0 or > 256 ||
            !ValidOwner(fence.ListOwnerUtf16Offset, value.NextUtf16Offset) ||
            !ValidOwner(fence.OuterListOwnerUtf16Offset, value.NextUtf16Offset) ||
            (fence.ListOwnerUtf16Offset is null) != (fence.ListDepth == -1) ||
            (fence.OuterListOwnerUtf16Offset is null) != (fence.OuterListDepth == -1) ||
            (fence.Language is not null && (fence.Language.Length > 64 ||
                fence.Language.Any(char.IsControl))))
            throw new InvalidDataException("Scanner checkpoint fence state is invalid.");
    }

    private static bool ValidOwner(long? offset, long nextOffset) =>
        offset is null || offset >= 0 && offset <= nextOffset;

    private static void ValidateStack(MarkdownScannerListFrame[]? stack, long nextOffset)
    {
        if (stack is null || stack.Length > MaximumDepth)
            throw new InvalidDataException("Scanner checkpoint stack depth is invalid.");
        foreach (var frame in stack)
        {
            if (frame is null || frame.Marker is null ||
                frame.Marker.Length is < 1 or > 12 ||
                frame.Marker.Any(char.IsControl) ||
                frame.MarkerIndent is < 0 or > 256 ||
                frame.ContentIndent is < 0 or > 256 ||
                frame.ContentIndent < frame.MarkerIndent ||
                frame.StartUtf16Offset < 0 || frame.StartUtf16Offset > nextOffset)
                throw new InvalidDataException("Scanner checkpoint list frame is invalid.");
        }
    }
}
