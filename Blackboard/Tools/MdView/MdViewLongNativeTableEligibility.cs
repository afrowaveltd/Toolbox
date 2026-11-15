using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Streaming, bounded-memory eligibility preflight for displaying every
/// source row of a long Markdown table as native Spectre fragments.
/// </summary>
/// <remarks>
/// This does not store a table or any fragment widgets. It scans a validated
/// source table through at most six-row native plans, rejecting the entire
/// native mode if even one later fragment cannot fit a fresh visual page.
/// The fixed fragment limit prevents expensive full-source scans on extremely
/// large tables; those use the existing compact layout instead.
/// </remarks>
internal static class MdViewLongNativeTableEligibility
{
    /// <summary>
    /// Maximum bounded fragments checked before choosing compact fallback.
    /// Supports up to 12,288 short source rows per table (six per fragment).
    /// </summary>
    internal const int MaximumFragments = 2048;

    /// <summary>
    /// Verifies complete source-table native viability without drawing or
    /// retaining previous fragments in memory.
    /// </summary>
    /// <param name="cursor">Previously validated, disk-backed source cursor.</param>
    /// <param name="header">Confirmed table header and alignment delimiters.</param>
    /// <param name="console">Physical Spectre profile used for measurement.</param>
    /// <param name="contentRows">Visual rows available to one fresh page.</param>
    /// <param name="maximumContentColumns">Optional content-column width cap.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="maximumFragments">Bounded fragment scan limit, 1–2048; lowered only in test scenarios.</param>
    /// <returns>True only if every source row fits a native fresh-page fragment.</returns>
    internal static async Task<bool> CanRenderEntireTableAsync(
        MdViewContainerPageCursor cursor,
        MdViewSpectreTableHeader header,
        IAnsiConsole console,
        int contentRows,
        int maximumContentColumns = 0,
        CancellationToken cancellationToken = default,
        int maximumFragments = MaximumFragments)
    {
        if (maximumFragments is < 1 or > MaximumFragments)
            throw new ArgumentOutOfRangeException(nameof(maximumFragments));
        ArgumentNullException.ThrowIfNull(cursor);
        ArgumentNullException.ThrowIfNull(header);
        ArgumentNullException.ThrowIfNull(console);
        MdViewMixedContentWidth.Validate(maximumContentColumns);
        if (contentRows is < 1 or > 128 ||
            contentRows > Math.Max(1, console.Profile.Height))
            throw new ArgumentOutOfRangeException(nameof(contentRows));
        cancellationToken.ThrowIfCancellationRequested();

        if (!MdViewNativeTablePagePlanner.SupportsNativeGrid(
            MdViewMixedContentWidth.EffectiveTerminalWidth(
                Math.Max(1, console.Profile.Width), maximumContentColumns),
            header.Columns.Count))
            return false;

        var nextLine = checked(header.SourceLine + 2);
        for (var fragment = 0; fragment < maximumFragments; fragment++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var planned = await MdViewNativeTablePagePlanner.PlanAsync(
                cursor, header, nextLine, console, contentRows,
                cancellationToken: cancellationToken,
                maximumContentColumns: maximumContentColumns)
                .ConfigureAwait(false);
            if (!planned.IsNative ||
                planned.FirstDataLine != nextLine ||
                planned.NextDataLine - nextLine != planned.DataRows ||
                planned.VisualRows is < 1 or > 128 ||
                planned.VisualRows > contentRows ||
                (planned.HasMore && planned.DataRows == 0))
                return false;
            if (!planned.HasMore)
                return true;

            nextLine = planned.NextDataLine;
        }

        // No partial promotion of unbounded source tables. Reuse compact
        // source layout when the conservative scan limit is exhausted.
        return false;
    }
}
