using Spectre.Console;
using Spectre.Console.Rendering;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Measures the actual visual row count of a bounded native Spectre renderable
/// using the same rendering pipeline and terminal profile as console output.
/// </summary>
/// <remarks>
/// <see cref="IRenderable"/> measurement reports width, not height. Native
/// table borders, headers and wrapped cells must be counted from rendered
/// segments. Only bounded chunk widgets are passed here; callers must never
/// render a complete unbounded document to measure its height.
/// </remarks>
public static class MdViewSpectreTableHeight
{
    /// <summary>Upper bound on inspected rendered rows for a single widget.</summary>
    public const int MaximumMeasuredRows = 512;

    /// <summary>
    /// Counts actual Spectre output rows at the supplied console's width.
    /// </summary>
    /// <param name="renderable">Small, caller-owned native Spectre widget.</param>
    /// <param name="console">Target terminal with current width and capabilities.</param>
    /// <param name="maximumRows">Early stop budget from 1 to 512.</param>
    /// <returns>Rendered rows, or maximumRows + 1 when the budget is exceeded.</returns>
    public static int CountRows(
        IRenderable renderable, IAnsiConsole console,
        int maximumRows = MaximumMeasuredRows)
    {
        ArgumentNullException.ThrowIfNull(renderable);
        ArgumentNullException.ThrowIfNull(console);
        if (maximumRows is < 1 or > MaximumMeasuredRows)
            throw new ArgumentOutOfRangeException(nameof(maximumRows));

        var rows = 0;
        var currentRowHasContent = false;
        foreach (var segment in renderable.GetSegments(console))
        {
            if (segment.IsControlCode)
                continue;

            if (segment.IsLineBreak)
            {
                rows++;
                currentRowHasContent = false;
            }
            else
            {
                // A renderable may emit ordinary segments containing
                // embedded newlines. Normalize those as additional rows.
                var text = segment.Text.AsSpan();
                if (!text.IsEmpty)
                {
                    for (var i = 0; i < text.Length; i++)
                    {
                        if (text[i] == '\n')
                        {
                            rows++;
                            currentRowHasContent = false;
                        }
                        else if (text[i] != '\r')
                        {
                            currentRowHasContent = true;
                        }
                    }
                }
            }

            if (rows > maximumRows)
                return maximumRows + 1;
        }

        if (currentRowHasContent)
            rows++;
        return Math.Min(rows, maximumRows + 1);
    }
}
