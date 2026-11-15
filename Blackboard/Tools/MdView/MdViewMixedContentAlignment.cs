using Spectre.Console;
using Spectre.Console.Rendering;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Optional horizontal positioning of the mixed Markdown reading column
/// while preserving its exact content width and visual row count.
/// </summary>
public static class MdViewMixedContentAlignment
{
    /// <summary>
    /// Returns the left gutter needed to center the existing reading column
    /// in the physical terminal. Zero also preserves the default layout.
    /// </summary>
    /// <param name="terminalWidth">Actual terminal width in columns.</param>
    /// <param name="maximumContentColumns">Optional Markdown content width cap; zero means automatic.</param>
    /// <param name="centerContent">True to center the reading column within available free space.</param>
    /// <returns>Non-negative number of columns to indent each visible content row.</returns>
    public static int LeftGutter(
        int terminalWidth, int maximumContentColumns, bool centerContent)
    {
        var effectiveWidth = MdViewMixedContentWidth.EffectiveTerminalWidth(
            terminalWidth, maximumContentColumns);
        return centerContent
            ? Math.Max(0, (terminalWidth - effectiveWidth) / 2)
            : 0;
    }

    /// <summary>
    /// Wraps a native Spectre table with only horizontal left padding.
    /// Top and bottom padding are deliberately zero to preserve page height.
    /// </summary>
    /// <param name="table">Measured native table widget.</param>
    /// <param name="leftGutter">Calculated non-negative left indentation.</param>
    /// <returns>The original widget when no gutter is needed, or a padded widget.</returns>
    public static IRenderable IndentTable(Table table, int leftGutter)
    {
        ArgumentNullException.ThrowIfNull(table);
        if (leftGutter < 0)
            throw new ArgumentOutOfRangeException(nameof(leftGutter));

        return leftGutter == 0
            ? table
            : new Padder(table, new Padding(leftGutter, 0, 0, 0));
    }
}
