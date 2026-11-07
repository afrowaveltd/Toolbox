using Afrowave.Toolbox.ExchangeHouse.Markdown;
using Xunit;

namespace Afrowave.Toolbox.ExchangeHouse.Markdown.Tests;

public sealed class MarkdownContainerStateMachineTests
{
    [Theory]
    [InlineData("- first\n  continuation\n  > quote\n  > - child\n  >   continuation\nend")]
    [InlineData("start\r\n```csharp\r\n# literal\r\n- literal\r\n```\r\n# real")]
    [InlineData("- parent\n  > quoted\n  > ```json\n  > {\"hello\": true}\n  > ```\n- next")]
    [InlineData("> ```text\n> code\noutside\n# heading\n\n- list")]
    [InlineData("- first\r\n\r\n  continuation\r\n  - child\r\n\r\nend")]
    public async Task CaptureAndRestoreAfterEveryLineMatchesUninterruptedScan(string source)
    {
        var baseline = new List<MarkdownContainerLine>();
        await foreach (var line in MarkdownIncrementalContainerScanner.ScanAsync(
            new StringReader(source)))
            baseline.Add(line);

        var physical = new List<MarkdownStreamLine>();
        await foreach (var line in MarkdownIncrementalLineScanner.ScanAsync(
            new StringReader(source)))
            physical.Add(line);

        var actual = new List<MarkdownContainerLine>();
        var machine = new MarkdownIncrementalContainerScanner.StateMachine();
        foreach (var line in physical)
        {
            var checkpoint = MarkdownScannerCheckpointCodec.Decode(
                MarkdownScannerCheckpointCodec.Encode(machine.Capture()));
            machine = MarkdownIncrementalContainerScanner.StateMachine.Restore(checkpoint);
            actual.Add(machine.Process(line));
        }

        Assert.Equal(baseline, actual);
        Assert.Equal(physical.Count, machine.NextLineNumber);
        Assert.Equal(source.Length, machine.NextUtf16Offset);
    }

    [Fact]
    public async Task RestoredCheckpointPreservesActiveFenceAndQuotedListOwner()
    {
        const string source = "- parent\n  > quote\n  > ```cs\n  > # literal\n  > ```\nend";
        var physical = new List<MarkdownStreamLine>();
        await foreach (var line in MarkdownIncrementalLineScanner.ScanAsync(
            new StringReader(source)))
            physical.Add(line);

        var machine = new MarkdownIncrementalContainerScanner.StateMachine();
        for (var i = 0; i < 3; i++)
            machine.Process(physical[i]);

        var checkpoint = machine.Capture();
        Assert.NotNull(checkpoint.Fence);
        Assert.Equal('`', checkpoint.Fence.Marker);
        Assert.NotNull(checkpoint.Fence.OuterListOwnerUtf16Offset);
        Assert.Equal(1, checkpoint.Fence.QuoteDepth);

        var restored = MarkdownIncrementalContainerScanner.StateMachine.Restore(
            MarkdownScannerCheckpointCodec.Decode(
                MarkdownScannerCheckpointCodec.Encode(checkpoint)));
        Assert.Equal(MarkdownContainerLineKind.CodeLine,
            restored.Process(physical[3]).Kind);
        Assert.Equal(MarkdownContainerLineKind.FenceClose,
            restored.Process(physical[4]).Kind);
    }

    [Fact]
    public void StateMachineRejectsMisalignedPhysicalSourceLines()
    {
        var machine = new MarkdownIncrementalContainerScanner.StateMachine();
        Assert.Throws<ArgumentException>(() => machine.Process(
            new MarkdownStreamLine(1, 0, MarkdownStreamLineKind.ParagraphLine, "test", "\n")));
        Assert.Throws<ArgumentException>(() => machine.Process(
            new MarkdownStreamLine(0, 4, MarkdownStreamLineKind.ParagraphLine, "test", "\n")));
        Assert.Equal(0, machine.NextLineNumber);
        Assert.Equal(0, machine.NextUtf16Offset);
    }

    [Fact]
    public void StateMachineRejectsRestoredFenceWithUnresolvableOuterOwner()
    {
        var checkpoint = new MarkdownScannerCheckpoint(1, 5, 80,
            [new MarkdownScannerListFrame(0, 2, 10, "-")],
            [], 1, 10, false, false,
            new MarkdownScannerFenceState('`', 3, 1, 50, "cs",
                -1, null, 0, 25, 0));
        Assert.Throws<InvalidDataException>(() =>
            MarkdownIncrementalContainerScanner.StateMachine.Restore(checkpoint));
    }
}
