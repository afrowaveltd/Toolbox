using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Renders canonical pages from a disk-backed mixed-document index using
/// exactly the same source-bound planner as index construction.
/// </summary>
/// <remarks>
/// The full planned page is checked against its disk record before any
/// output is written. A resize or substituted source cursor invalidates
/// the index; the caller must rebuild it for the new document/viewport.
/// </remarks>
public static class MdViewMixedVisualIndexedRenderer
{
    /// <summary>
    /// Reconstructs and displays one indexed mixed Markdown/native-table
    /// page after checking the complete source and visual-row contract.
    /// </summary>
    /// <param name="index">Caller-owned mixed visual page index.</param>
    /// <param name="cursor">The same validated cursor used to build the index.</param>
    /// <param name="console">Target terminal with unchanged geometry.</param>
    /// <param name="pageNumber">Zero-based canonical document page number.</param>
    /// <param name="theme">Existing Markdown styles shared with the compact reader.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="centerContent">Whether to center the planned reading column without changing its indexed rows.</param>
    /// <returns>Verified source-bound index record for the displayed page.</returns>
    public static async Task<MdViewMixedIndexedPage> RenderPageAsync(
        MdViewMixedVisualPageIndex index,
        MdViewContainerPageCursor cursor,
        IAnsiConsole console,
        long pageNumber,
        MarkdownSpectreTheme theme,
        CancellationToken cancellationToken = default,
        bool centerContent = false)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(cursor);
        ArgumentNullException.ThrowIfNull(console);
        ArgumentNullException.ThrowIfNull(theme);
        cancellationToken.ThrowIfCancellationRequested();

        if (!index.UsesSourceCursor(cursor))
            throw new InvalidDataException(
                "The mixed page source differs from the indexed source.");
        if (console.Profile.Width != index.TerminalWidth ||
            console.Profile.Height != index.TerminalHeight ||
            console.Profile.Capabilities.Unicode != index.Unicode)
            throw new InvalidOperationException(
                "Terminal geometry changed; rebuild the mixed visual page index.");

        var expected = await index.GetPageAsync(pageNumber, cancellationToken)
            .ConfigureAwait(false);
        var page = await MdViewMixedVisualPagePlanner.PlanAsync(
            cursor, expected.Start, console, index.ContentRows,
            cancellationToken,
            maximumContentColumns: index.MaximumContentColumns,
            longNativeTables: index.LongNativeTables)
            .ConfigureAwait(false);

        if (page.LongNativeTables != index.LongNativeTables ||
            page.Start != expected.Start ||
            page.Next != expected.Next ||
            page.Rows != expected.VisualRows ||
            page.AtEnd != expected.AtEnd ||
            page.ContainsClippedCode != expected.ContainsClippedCode)
            throw new InvalidDataException(
                "Mixed Markdown page source or visual geometry no longer matches its disk index.");

        cancellationToken.ThrowIfCancellationRequested();
        var resume = MdViewMixedVisualPageRenderer.Render(
            console, page, theme, centerContent);
        if (resume != expected.Next)
            throw new InvalidDataException(
                "Mixed page renderer returned an unexpected source position.");

        return expected;
    }
}
