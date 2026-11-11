using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Afrowave.Toolbox.ExchangeHouse.Markdown;
using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>Visual page rendered from a source-bound start, including its next resume point.</summary>
/// <param name="Start">Source-and-wrap position at the start of the page.</param>
/// <param name="Next">The first source visual row after the rendered page.</param>
/// <param name="Rows">Number of displayed visual content rows.</param>
/// <param name="ContainsClippedCode">True if any code line extends beyond the viewport.</param>
public sealed record MdViewTerminalRenderedPage(
    MdViewVisualPosition Start, MdViewVisualPosition Next,
    int Rows, bool ContainsClippedCode);

/// <summary>
/// Renders a bounded number of visual Markdown lines from the existing
/// disk-backed container cursor, using the same layout as the page index.
/// </summary>
public static class MdViewTerminalPageRenderer
{
    /// <summary>Renders at most the requested visual rows.</summary>
    /// <param name="console">Target Spectre terminal.</param>
    /// <param name="cursor">Validated snapshot's persistent source-aware cursor.</param>
    /// <param name="start">Start physical line and optional soft-wrap row.</param>
    /// <param name="columns">Available text columns.</param>
    /// <param name="contentRows">Visual row budget.</param>
    /// <param name="theme">Existing Markdown presentation theme.</param>
    /// <param name="cancellationToken">Cancels source reading and output.</param>
    /// <returns>Exact next source/row location for a following page.</returns>
    public static async Task<MdViewTerminalRenderedPage> RenderAsync(
        IAnsiConsole console, MdViewContainerPageCursor cursor,
        MdViewVisualPosition start, int columns, int contentRows,
        MarkdownSpectreTheme theme, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentNullException.ThrowIfNull(cursor);
        ArgumentNullException.ThrowIfNull(theme);
        if (start.Line < 0 || start.WrapRow < 0)
            throw new ArgumentOutOfRangeException(nameof(start));
        if (columns is < 8 or > 4096)
            throw new ArgumentOutOfRangeException(nameof(columns));
        if (contentRows is < 1 or > 128)
            throw new ArgumentOutOfRangeException(nameof(contentRows));

        var position = start;
        var emitted = 0;
        var clipped = false;

        while (emitted < contentRows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var batch = await cursor.ReadAsync(
                position.Line, Math.Min(128, contentRows - emitted),
                cancellationToken).ConfigureAwait(false);
            if (batch.Count == 0)
                break;

            foreach (var classified in batch)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (classified.Source.LineNumber != position.Line)
                    throw new InvalidDataException("Preview source cursor lost line alignment.");

                var code = MdViewTerminalPageIndex.IsLiteralCode(classified);
                var source = classified.Source.Text;
                var rows = MdViewTerminalWordWrap.Wrap(source, columns, code);
                if (position.WrapRow >= rows.Count)
                    throw new InvalidDataException(
                        "Visual page position exceeds the wrapped source line.");

                if (code && MdViewTerminalWordWrap.MeasureColumns(
                    source.Replace("\t", "    ", StringComparison.Ordinal)) > columns)
                    clipped = true;

                for (var row = position.WrapRow; row < rows.Count; row++)
                {
                    var rendered = rows[row];
                    if (code)
                    {
                        console.Write(new Text(rendered + Environment.NewLine, theme.Code));
                    }
                    else if (classified.Source.Kind == MarkdownStreamLineKind.Heading &&
                             row > 0 &&
                             classified.Source.HeadingLevel is >= 1 and <= 6)
                    {
                        // Wrapped heading continuation retains heading styling;
                        // Markdown syntax markers are not repeated on soft rows.
                        console.Write(new Text(
                            rendered + Environment.NewLine,
                            theme.GetHeadingStyle(classified.Source.HeadingLevel)));
                    }
                    else if (rendered.Length == 0)
                    {
                        // The Markdown parser has no block for an empty
                        // string. Reserve its actual blank terminal row so
                        // the visual page index and emitted rows agree.
                        console.WriteLine();
                    }
                    else
                    {
                        MarkdownSpectreRenderer.Render(console, rendered, theme);
                    }

                    emitted++;
                    if (emitted == contentRows && row + 1 < rows.Count)
                        return new MdViewTerminalRenderedPage(start,
                            new MdViewVisualPosition(position.Line, row + 1),
                            emitted, clipped);
                    if (emitted == contentRows)
                        return new MdViewTerminalRenderedPage(start,
                            new MdViewVisualPosition(position.Line + 1, 0),
                            emitted, clipped);
                }
                position = new MdViewVisualPosition(position.Line + 1, 0);
            }
        }

        return new MdViewTerminalRenderedPage(start, position, emitted, clipped);
    }
}
