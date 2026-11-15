using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Renders a single indexed native Spectre table fragment after verifying
/// that its measured source coordinates still match the disk record.
/// </summary>
/// <remarks>
/// This table-only renderer is not yet connected to the ordinary Markdown
/// page UI. It intentionally uses the same bounded planner as indexing;
/// a resized console must rebuild the index before any widget is written.
/// </remarks>
public static class MdViewNativeTableFragmentRenderer
{
    /// <summary>
    /// Reconstructs the requested native table fragment and writes it only
    /// after index/renderer geometry and source-resume parity are checked.
    /// </summary>
    /// <param name="index">Complete, caller-owned native table fragment index.</param>
    /// <param name="cursor">Validated Markdown source cursor.</param>
    /// <param name="header">Previously validated table source header.</param>
    /// <param name="console">Destination terminal with unchanged geometry.</param>
    /// <param name="fragmentNumber">Zero-based indexed fragment number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Source and visual geometry of the displayed fragment.</returns>
    public static async Task<MdViewNativeTableFragment> RenderAsync(
        MdViewNativeTableFragmentIndex index,
        MdViewContainerPageCursor cursor,
        MdViewSpectreTableHeader header,
        IAnsiConsole console,
        long fragmentNumber,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(cursor);
        ArgumentNullException.ThrowIfNull(header);
        ArgumentNullException.ThrowIfNull(console);
        cancellationToken.ThrowIfCancellationRequested();
        if (!index.IsNative)
            throw new InvalidOperationException(
                "Native table index requested compact source fallback.");
        if (index.HeaderLine != header.SourceLine)
            throw new InvalidDataException(
                "The requested table header differs from the indexed source.");
        if (index.TerminalWidth != console.Profile.Width ||
            index.TerminalHeight != console.Profile.Height ||
            index.Unicode != console.Profile.Capabilities.Unicode)
            throw new InvalidOperationException(
                "The terminal geometry changed; rebuild the native table index.");

        var expected = await index.GetFragmentAsync(
            fragmentNumber, cancellationToken).ConfigureAwait(false);
        var actual = await MdViewNativeTablePagePlanner.PlanAsync(
            cursor, header, expected.FirstDataLine, console,
            index.ContentRows, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (!actual.IsNative || actual.Widget is null ||
            actual.NextDataLine != expected.NextDataLine ||
            actual.DataRows != expected.DataRows ||
            actual.VisualRows != expected.VisualRows ||
            actual.HasMore != expected.HasMore)
            throw new InvalidDataException(
                "Native table source or visual geometry no longer matches its disk index.");

        cancellationToken.ThrowIfCancellationRequested();
        console.Write(actual.Widget);
        return expected;
    }
}
