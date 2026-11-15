namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Shared deterministic content-width policy for the opt-in mixed Markdown
/// preview. The physical terminal is never resized or misreported.
/// </summary>
/// <remarks>
/// The width limit concerns visible Markdown content columns, not physical
/// terminal columns. Native Spectre tables use the identical effective width,
/// including the usual two-column terminal margin.
/// </remarks>
public static class MdViewMixedContentWidth
{
    /// <summary>Minimum explicitly requested content width.</summary>
    public const int MinimumColumns = 40;

    /// <summary>Maximum explicitly requested content width.</summary>
    public const int MaximumColumns = 4096;

    /// <summary>
    /// Validates the requested maximum; zero retains the full terminal width.
    /// </summary>
    /// <param name="maximumColumns">Zero or 40–4096 visible content columns.</param>
    public static void Validate(int maximumColumns)
    {
        if (maximumColumns != 0 &&
            maximumColumns is < MinimumColumns or > MaximumColumns)
            throw new ArgumentOutOfRangeException(nameof(maximumColumns),
                "Maximum content width must be 0 (automatic) or 40–4096 columns.");
    }

    /// <summary>
    /// Returns physical columns available to the shared visual planner
    /// including its existing two-column outer margin.
    /// </summary>
    /// <param name="terminalWidth">Actual terminal width in columns.</param>
    /// <param name="maximumColumns">Zero, or requested maximum content width.</param>
    /// <returns>Effective terminal columns for rendering and native table measurement.</returns>
    public static int EffectiveTerminalWidth(int terminalWidth, int maximumColumns)
    {
        if (terminalWidth < 1)
            throw new ArgumentOutOfRangeException(nameof(terminalWidth));
        Validate(maximumColumns);
        return maximumColumns == 0
            ? terminalWidth
            : Math.Min(terminalWidth, maximumColumns + 2);
    }

    /// <summary>
    /// Gets the precise text layout width, preserving the existing
    /// minimum-eight-column safe layout contract.
    /// </summary>
    /// <param name="terminalWidth">Actual terminal columns.</param>
    /// <param name="maximumColumns">Optional content column limit.</param>
    /// <returns>Markdown layout columns, from 8 to 4096.</returns>
    public static int ContentColumns(int terminalWidth, int maximumColumns) =>
        Math.Clamp(EffectiveTerminalWidth(terminalWidth, maximumColumns) - 2,
            8, MaximumColumns);
}
