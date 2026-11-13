using System.Globalization;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Afrowave.Toolbox.Blackboard.TextFiles;
using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Experimental local Markdown pager with a viewport-specific disk page index,
/// source-aware code classification and bounded visual row rendering.
/// </summary>
/// <remarks>
/// Recomputes page boundaries on the next drawing after terminal resizing.
/// Ordinary prose wraps near sentence, clause or word breaks. Fenced source
/// code is not wrapped and shows a right-arrow if clipped to the viewport.
/// Complex Markdown list/quote continuity and exact proportional-font
/// terminal rendering remain experimental.
/// </remarks>
public sealed class MdViewFormattedPreviewPager
{
    private readonly IAnsiConsole _console;

    /// <summary>Creates a pager that renders to the supplied terminal.</summary>
    /// <param name="console">Interactive terminal for formatted preview.</param>
    public MdViewFormattedPreviewPager(IAnsiConsole console) =>
        _console = console ?? throw new ArgumentNullException(nameof(console));

    /// <summary>
    /// Displays a source-bound visual page fitted to the terminal dimensions,
    /// with lazy disk-indexed heading and link source-position navigation.
    /// </summary>
    /// <param name="path">Local UTF-8 Markdown document.</param>
    /// <param name="linesPerPage">Zero for automatic viewport height, or an explicit 1–128 visual row cap.</param>
    /// <param name="cancellationToken">Cancels validation, indexing or navigation.</param>
    public async Task RunAsync(string path, int linesPerPage = 0,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (linesPerPage is < 0 or > 128)
            throw new ArgumentOutOfRangeException(nameof(linesPerPage));
        if (!_console.Profile.Capabilities.Interactive)
            throw new InvalidOperationException(
                "Formatted preview paging requires an interactive terminal.");

        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(
            path, cancellationToken: cancellationToken);
        await using var containerCursor = new MdViewContainerPageCursor(snapshot);
        var lines = snapshot.Lines;

        MdViewTerminalPageIndex? pageIndex = null;
        MdViewDiskNavigationIndex? navigationIndex = null;
        var picker = new MdViewDiskNavigationPicker(_console);
        var theme = MdViewHighlighting.CreateTheme();
        var gray = new Style(foreground: Color.Grey);

        var position = new MdViewVisualPosition(0, 0);
        var previousPages = new Stack<(MdViewVisualPosition Position, long Number, bool Custom)>();
        long pageNumber = 0;
        var customStart = false;
        string? feedback = null;

        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                // Spectre.Console queries terminal dimensions at this point.
                // A resize while a prompt is visible does not interrupt it;
                // the next draw will get a fresh source-bound page index.
                var width = Math.Max(1, _console.Profile.Width);
                var height = Math.Max(1, _console.Profile.Height);
                if (pageIndex is null ||
                    pageIndex.TerminalWidth != width ||
                    pageIndex.TerminalHeight != height)
                {
                    if (pageIndex is not null)
                    {
                        await pageIndex.DisposeAsync().ConfigureAwait(false);
                        // Soft-wrap offsets from the prior width are invalid.
                        position = new MdViewVisualPosition(position.Line, 0);
                        previousPages.Clear();
                    }

                    _console.Write(new Text(
                        "Measuring Markdown pages for current terminal size..." +
                        Environment.NewLine, gray));
                    pageIndex = await MdViewTerminalPageIndex.CreateAsync(
                        snapshot, width, height, linesPerPage,
                        cancellationToken: cancellationToken).ConfigureAwait(false);
                    pageNumber = lines.LineCount == 0 ? 0 :
                        await pageIndex.FindPageAsync(position, cancellationToken)
                            .ConfigureAwait(false);
                    customStart = position !=
                        await pageIndex.GetStartAsync(pageNumber, cancellationToken)
                            .ConfigureAwait(false);
                }

                _console.Clear();
                WriteGrayHeader(path, snapshot, pageIndex, gray);
                if (feedback is not null)
                {
                    _console.Write(new Text(feedback + Environment.NewLine, gray));
                    feedback = null;
                }

                var rendered = await MdViewTerminalPageRenderer.RenderAsync(
                    _console, containerCursor, position, pageIndex.Columns,
                    pageIndex.RowsPerPage, theme, cancellationToken).ConfigureAwait(false);

                if (rendered.Rows == 0)
                    _console.Write(new Text("(Empty page)" + Environment.NewLine));

                // The source line may continue through several visual pages.
                // The displayed range includes that partially shown last line.
                var lastLine = rendered.Next.WrapRow > 0
                    ? rendered.Next.Line + 1 : rendered.Next.Line;
                _console.WriteLine();
                _console.Write(new Text(
                    "Page " + (pageNumber + 1).ToString(CultureInfo.InvariantCulture) +
                    " of " + pageIndex.PageCount.ToString(CultureInfo.InvariantCulture) +
                    (customStart ? " (jump view)" : "") +
                    " | Source lines " +
                    (rendered.Rows == 0 ? "0" :
                        (position.Line + 1).ToString(CultureInfo.InvariantCulture)) +
                    "–" + lastLine.ToString(CultureInfo.InvariantCulture) +
                    " of " + lines.LineCount.ToString(CultureInfo.InvariantCulture) +
                    Environment.NewLine, gray));
                if (rendered.ContainsClippedCode)
                {
                    // A narrow terminal must not wrap the status warning
                    // into additional rows outside the viewport budget.
                    // The arrow on the code line and this note both mean
                    // horizontal clipping, never a source line break.
                    var codeNote = pageIndex.Columns >= 56
                        ? "→ Code continues beyond the visible width (not wrapped)."
                        : pageIndex.Columns >= 25
                            ? "→ Code clipped (no wrap)."
                            : pageIndex.Columns >= 14
                                ? "→ Code clipped"
                                : "→ code";
                    _console.Write(new Text(codeNote + Environment.NewLine, gray));
                }

                const string next = "Next page";
                const string previous = "Previous page";
                const string jump = "Go to source line";
                const string headings = "Find heading (disk index)";
                const string links = "Find link location (disk index)";
                const string nativeTable = "View table (Spectre)";
                const string exit = "Exit preview";

                var choices = new List<string>();
                if (rendered.Next.Line < lines.LineCount)
                    choices.Add(next);
                if (previousPages.Count != 0)
                    choices.Add(previous);
                if (lines.LineCount != 0)
                {
                    choices.Add(jump);
                    choices.Add(headings);
                    choices.Add(links);
                }
                // Inspect only this visible page (up to 128 physical lines).
                // No whole-table state is allocated or stored in RAM.
                long? visibleTable = null;
                if (rendered.Rows > 0)
                    visibleTable = await MdViewSpectreTableViewer.FindFirstTableAsync(
                        containerCursor, position.Line,
                        rendered.Next.Line + (rendered.Next.WrapRow > 0 ? 1 : 0),
                        cancellationToken).ConfigureAwait(false);
                if (visibleTable is not null && width >= 28)
                    choices.Add(nativeTable);
                choices.Add(exit);

                var chosen = await new SelectionPrompt<string>()
                    .Title("Preview navigation")
                    .PageSize(Math.Clamp(choices.Count, 3, 8))
                    .AddChoices(choices)
                    .AddCancelResult(exit)
                    .ShowAsync(_console, cancellationToken).ConfigureAwait(false);

                if (chosen == exit)
                    return;

                if (chosen == next)
                {
                    previousPages.Push((position, pageNumber, customStart));
                    if (!customStart && pageNumber + 1 < pageIndex.PageCount)
                    {
                        pageNumber++;
                        position = await pageIndex.GetStartAsync(
                            pageNumber, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        position = rendered.Next;
                        pageNumber = await pageIndex.FindPageAsync(
                            position, cancellationToken).ConfigureAwait(false);
                    }
                    customStart = position !=
                        await pageIndex.GetStartAsync(pageNumber, cancellationToken)
                            .ConfigureAwait(false);
                }
                else if (chosen == previous)
                {
                    var prior = previousPages.Pop();
                    position = prior.Position;
                    pageNumber = prior.Number;
                    customStart = prior.Custom;
                }
                else if (chosen == jump)
                {
                    var input = await new TextPrompt<string>(
                        "Go to physical source line (1-based, empty = cancel): ")
                        .AllowEmpty().ShowAsync(_console, cancellationToken)
                        .ConfigureAwait(false);
                    if (string.IsNullOrWhiteSpace(input))
                        continue;
                    if (!long.TryParse(input, NumberStyles.None,
                            CultureInfo.InvariantCulture, out var number) ||
                        number < 1 || number > lines.LineCount)
                    {
                        feedback = "Source line is outside this document.";
                        continue;
                    }

                    previousPages.Clear();
                    position = new MdViewVisualPosition(number - 1, 0);
                    pageNumber = await pageIndex.FindPageAsync(
                        position, cancellationToken).ConfigureAwait(false);
                    customStart = position !=
                        await pageIndex.GetStartAsync(pageNumber, cancellationToken)
                            .ConfigureAwait(false);
                }
                else if (chosen == nativeTable && visibleTable is not null)
                {
                    await new MdViewSpectreTableViewer(_console).RunAsync(
                        containerCursor, visibleTable.Value, cancellationToken)
                        .ConfigureAwait(false);
                    // The source pager retains its canonical page and rechecks
                    // the actual terminal size on the next normal draw.
                }
                else if (chosen == headings || chosen == links)
                {
                    try
                    {
                        if (navigationIndex is null)
                        {
                            _console.Write(new Text(
                                "Indexing validated Markdown source once; results stay on disk..." +
                                Environment.NewLine));
                            navigationIndex = await MdViewDiskNavigationIndex.CreateAsync(
                                snapshot, cancellationToken: cancellationToken)
                                .ConfigureAwait(false);
                        }

                        var kind = chosen == headings
                            ? MdViewIndexedItemKind.Heading
                            : MdViewIndexedItemKind.Link;
                        var selected = await picker.PickAsync(
                            navigationIndex, kind, cancellationToken).ConfigureAwait(false);
                        if (selected is not null)
                        {
                            previousPages.Clear();
                            position = new MdViewVisualPosition(selected.LineNumber, 0);
                            pageNumber = await pageIndex.FindPageAsync(
                                position, cancellationToken).ConfigureAwait(false);
                            customStart = position !=
                                await pageIndex.GetStartAsync(pageNumber, cancellationToken)
                                    .ConfigureAwait(false);
                            feedback = "Jumped to source line " +
                                (position.Line + 1).ToString(CultureInfo.InvariantCulture) +
                                (kind == MdViewIndexedItemKind.Link
                                    ? " containing the selected Markdown link."
                                    : " containing the selected heading.");
                        }
                    }
                    catch (Exception ex) when (ex is IOException or InvalidDataException or
                        UnauthorizedAccessException)
                    {
                        feedback = "Navigation index unavailable: " + ex.Message;
                    }
                }
            }
        }
        finally
        {
            if (navigationIndex is not null)
                await navigationIndex.DisposeAsync().ConfigureAwait(false);
            if (pageIndex is not null)
                await pageIndex.DisposeAsync().ConfigureAwait(false);
        }
    }

    private void WriteGrayHeader(
        string path, Utf8ValidatedSnapshot snapshot,
        MdViewTerminalPageIndex layout, Style gray)
    {
        var columns = layout.Columns;
        void Write(string text)
        {
            // Keep each gray header line within one terminal row.
            _console.Write(new Text(
                MdViewTerminalWordWrap.Wrap(text, columns, code: true)[0] +
                Environment.NewLine, gray));
        }

        Write("mdview — experimental Markdown page preview");
        Write(Path.GetFileName(path) + " | " +
            snapshot.ByteLength.ToString(CultureInfo.InvariantCulture) +
            " bytes | " + snapshot.Lines.LineCount.ToString(CultureInfo.InvariantCulture) +
            " physical lines");
        Write("Smart prose wrapping | code stays unwrapped | viewport " +
            layout.TerminalWidth + "×" + layout.TerminalHeight);
        _console.WriteLine(); // One blank line between header and document.
    }
}
