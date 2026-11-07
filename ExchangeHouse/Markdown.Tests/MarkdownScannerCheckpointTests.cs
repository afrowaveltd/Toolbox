using System.Text;
using Afrowave.Toolbox.ExchangeHouse.Markdown;
using Xunit;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown.Tests;

public sealed class MarkdownScannerCheckpointTests
{
    private static MarkdownScannerCheckpoint Example() => new(
        MarkdownScannerCheckpointCodec.CurrentVersion,
        43,
        715,
        [new MarkdownScannerListFrame(0, 2, 24, "-"),
         new MarkdownScannerListFrame(2, 4, 80, "1.")],
        [new MarkdownScannerListFrame(0, 2, 140, "*")],
        1,
        24,
        false,
        true,
        new MarkdownScannerFenceState(
            '`', 3, 1, 512, "csharp", 0, 140, 2, 24, 0));

    [Fact]
    public void CheckpointRoundTripsAllNestedOwnershipAndFenceFields()
    {
        var source = Example();
        var bytes = MarkdownScannerCheckpointCodec.Encode(source);
        var recovered = MarkdownScannerCheckpointCodec.Decode(bytes);
        Assert.Equal(source.Version, recovered.Version);
        Assert.Equal(source.NextLineNumber, recovered.NextLineNumber);
        Assert.Equal(source.NextUtf16Offset, recovered.NextUtf16Offset);
        Assert.Equal(source.OrdinaryLists, recovered.OrdinaryLists);
        Assert.Equal(source.QuoteLists, recovered.QuoteLists);
        Assert.Equal(source.CurrentQuoteDepth, recovered.CurrentQuoteDepth);
        Assert.Equal(source.CurrentOuterQuoteOwnerUtf16Offset,
            recovered.CurrentOuterQuoteOwnerUtf16Offset);
        Assert.Equal(source.OrdinaryBlankPending, recovered.OrdinaryBlankPending);
        Assert.Equal(source.QuoteBlankPending, recovered.QuoteBlankPending);
        Assert.Equal(source.Fence, recovered.Fence);
        Assert.InRange(bytes.Length, 1, MarkdownScannerCheckpointCodec.MaximumEncodedBytes);
    }

    [Fact]
    public void EmptyInitialCheckpointRoundTrips()
    {
        var checkpoint = new MarkdownScannerCheckpoint(1, 0, 0,
            [], [], 0, null, false, false, null);
        var restored = MarkdownScannerCheckpointCodec.Decode(
            MarkdownScannerCheckpointCodec.Encode(checkpoint));
        Assert.Equal(0, restored.NextLineNumber);
        Assert.Empty(restored.OrdinaryLists);
        Assert.Null(restored.Fence);
    }

    [Fact]
    public void UnknownVersionAndImpossibleOwnerAreRejected()
    {
        var checkpoint = Example();
        Assert.Throws<InvalidDataException>(() =>
            MarkdownScannerCheckpointCodec.Encode(checkpoint with { Version = 2 }));
        Assert.Throws<InvalidDataException>(() =>
            MarkdownScannerCheckpointCodec.Encode(
                checkpoint with { CurrentOuterQuoteOwnerUtf16Offset = 9999 }));
        Assert.Throws<InvalidDataException>(() =>
            MarkdownScannerCheckpointCodec.Encode(
                checkpoint with { OrdinaryLists = new[] {
                    new MarkdownScannerListFrame(0, 2, 10000, "-") } }));
    }

    [Fact]
    public void InvalidFenceAndExcessiveNestingAreRejected()
    {
        var checkpoint = Example();
        Assert.Throws<InvalidDataException>(() =>
            MarkdownScannerCheckpointCodec.Encode(checkpoint with {
                Fence = checkpoint.Fence! with { Marker = '#'} }));
        Assert.Throws<InvalidDataException>(() =>
            MarkdownScannerCheckpointCodec.Encode(checkpoint with {
                OrdinaryLists = Enumerable.Range(0, 33)
                    .Select(i => new MarkdownScannerListFrame(0, 2, i, "-")).ToArray() }));
    }

    [Fact]
    public void InvalidJsonAndOversizedPayloadAreRejected()
    {
        Assert.Throws<InvalidDataException>(() =>
            MarkdownScannerCheckpointCodec.Decode(Encoding.UTF8.GetBytes("{bad json")));
        Assert.Throws<InvalidDataException>(() =>
            MarkdownScannerCheckpointCodec.Decode(
                new byte[MarkdownScannerCheckpointCodec.MaximumEncodedBytes + 1]));
        Assert.Throws<InvalidDataException>(() =>
            MarkdownScannerCheckpointCodec.Decode([]));
    }
}
