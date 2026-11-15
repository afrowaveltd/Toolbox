using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Renders a bounded mixed visual-page plan, preserving the exact source
/// continuation and the Markdown styles of the existing compact renderer.
/// </summary>
/// <remarks>
/// Before writing, the renderer verifies terminal geometry and all native
/// table heights against the actual Spectre render-segment pipeline. It
/// never parses untrusted Markdown as Spectre markup. This separate
/// experimental component is not yet connected to the default pager.
/// </remarks>
public static class MdViewMixedVisualPageRenderer
{
    /// <summary>
    /// Writes a previously planned mixed page after checking viewport and
    /// measured-widget parity. Source files and disk indexes are unchanged.
    /// </summary>
    /// <param name="console">Target Spectre console.</param>
    /// <param name="plan">Bounded, source-aware page plan.</param>
    /// <param name="theme">Existing Markdown presentation theme.</param>
    /// <param name="centerContent">Whether to center the reading column inside the physical terminal.</param>
    /// <returns>Exact resume position recorded by the page planner.</returns>
    public static MdViewMixedVisualPosition Render(
        IAnsiConsole console, MdViewMixedVisualPage plan,
        MarkdownSpectreTheme theme, bool centerContent = false)
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(theme);

        if (console.Profile.Width != plan.TerminalWidth ||
            console.Profile.Height != plan.TerminalHeight ||
            console.Profile.Capabilities.Unicode != plan.Unicode)
            throw new InvalidOperationException(
                "Terminal geometry changed; rebuild the mixed visual page plan.");

        var gutter = MdViewMixedContentAlignment.LeftGutter(
            plan.TerminalWidth, plan.MaximumContentColumns, centerContent);
        var prefix = gutter == 0 ? null : new Text(new string(' ', gutter));

        var counted = 0;
        foreach (var block in plan.Blocks)
        {
            if (block.VisualRows is < 1 or > 128)
                throw new InvalidDataException(
                    "Mixed visual page contains an invalid block height.");
            counted = checked(counted + block.VisualRows);
            if (counted > 128)
                throw new InvalidDataException(
                    "Mixed visual page exceeds the maximum visual row budget.");

            if (block is MdViewMixedVisualTableBlock native)
            {
                if (native.Widget is null ||
                    MdViewSpectreTableHeight.CountRows(
                        MdViewMixedContentAlignment.IndentTable(
                            native.Widget, gutter),
                        console, 128) != native.VisualRows)
                    throw new InvalidDataException(
                        "Native table height differs from the mixed page plan.");
            }
            else if (block is MdViewMixedVisualTextBlock text)
            {
                if (text.Rows.Count != block.VisualRows)
                    throw new InvalidDataException(
                        "Text row count differs from the mixed page plan.");
            }
            else
            {
                throw new InvalidDataException(
                    "Mixed page contains an unsupported visual block.");
            }
        }

        if (counted != plan.Rows)
            throw new InvalidDataException(
                "Mixed page visual block heights do not match its total.");

        foreach (var block in plan.Blocks)
        {
            switch (block)
            {
                case MdViewMixedVisualTextBlock text:
                    foreach (var row in text.Rows)
                    {
                        if (prefix is not null)
                            console.Write(prefix);
                        foreach (var span in row.Spans)
                            console.Write(new Text(span.Text,
                                MdViewTerminalPageRenderer.ResolveStyle(
                                    span.Role, text.Source, theme)));
                        console.WriteLine();
                    }
                    break;
                case MdViewMixedVisualTableBlock native:
                    console.Write(MdViewMixedContentAlignment.IndentTable(
                        native.Widget, gutter));
                    break;
            }
        }
        return plan.Next;
    }
}
