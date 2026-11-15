using System.Globalization;
using Afrowave.Toolbox.Blackboard.TextFiles;
using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Opt-in interactive local Markdown pager using disk-indexed mixed visual
/// pages with small native Spectre tables between ordinary styled text.
/// </summary>
/// <remarks>
/// The established "preview" command remains compact by default. This
/// pager uses the validated source snapshot, disk-backed mixed page
/// index and source-navigation index. A resize rebuilds visual page
/// boundaries from the current physical source-line anchor.
/// </remarks>
public sealed class MdViewMixedPreviewPager
{
    private readonly IAnsiConsole _console;

    /// <summary>Creates a mixed-page viewer for an interactive terminal.</summary>
    /// <param name="console">Console used for all rendering and navigation.</param>
    public MdViewMixedPreviewPager(IAnsiConsole console) =>
        _console = console ?? throw new ArgumentNullException(nameof(console));

    /// <summary>
    /// Opens a validated local Markdown file in an explicitly selected mixed
    /// visual preview, including native tables and canonical source jumps.
    /// </summary>
    /// <param name="path">Local UTF-8 Markdown source file.</param>
    /// <param name="linesPerPage">Zero for terminal height minus controls, else 1–128 rows.</param>
    /// <param name="cancellationToken">Cancels reading, indexing or interaction.</param>
    public async Task RunAsync(string path, int linesPerPage = 0,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (linesPerPage is < 0 or > 128)
            throw new ArgumentOutOfRangeException(nameof(linesPerPage));
        if (!_console.Profile.Capabilities.Interactive)
            throw new InvalidOperationException(
                "Mixed Markdown preview requires an interactive terminal.");

        // A smaller window cannot reliably fit even the bounded navigation
        // menu and status. Reject before opening or scanning the source.
        if (!CanDisplay(_console.Profile.Width, _console.Profile.Height))
        {
            _console.Write(new Text(
                "Mixed preview requires at least 28 columns and 18 rows. " +
                "Use 'mdview preview PATH' for compact viewing." +
                Environment.NewLine));
            return;
        }

        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(
            path, cancellationToken: cancellationToken);
        await using var cursor = new MdViewContainerPageCursor(snapshot);
        MdViewMixedVisualPageIndex? index = null;
        MdViewDiskNavigationIndex? navigation = null;
        var picker = new MdViewDiskNavigationPicker(_console);
        var theme = MdViewHighlighting.CreateTheme();
        var gray = new Style(foreground: Color.Grey);
        long pageNumber = 0;
        long anchorLine = 0;
        string? feedback = null;

        const string next = "Next page";
        const string previous = "Previous page";
        const string jump = "Go to source line";
        const string headings = "Find heading (disk index)";
        const string links = "Find link location (disk index)";
        const string exit = "Exit mixed preview";

        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var width = _console.Profile.Width;
                var height = _console.Profile.Height;
                if (!CanDisplay(width, height))
                {
                    WriteGrayLine("Mixed preview needs at least 28×18. " +
                        "Enlarge the terminal and reopen the preview.", gray, width);
                    return;
                }

                if (index is null ||
                    index.TerminalWidth != width ||
                    index.TerminalHeight != height ||
                    index.Unicode != _console.Profile.Capabilities.Unicode)
                {
                    if (index is not null)
                    {
                        // Wrapped rows are invalid after a width change.
                        // Anchor on the original physical source line, then
                        // find the canonical new page containing that line.
                        var oldPage = await index.GetPageAsync(
                            pageNumber, cancellationToken).ConfigureAwait(false);
                        anchorLine = oldPage.Start.Line;
                        await index.DisposeAsync().ConfigureAwait(false);
                        index = null;
                    }

                    WriteGrayLine(
                        "Measuring mixed Markdown pages for terminal size...",
                        gray, width);
                    index = await MdViewMixedVisualPageIndex.CreateAsync(
                        cursor, _console, ContentRows(height, linesPerPage),
                        cancellationToken: cancellationToken).ConfigureAwait(false);
                    pageNumber = snapshot.Lines.LineCount == 0 ? 0 :
                        await index.FindPageAsync(
                            Math.Min(anchorLine, snapshot.Lines.LineCount - 1),
                            cancellationToken).ConfigureAwait(false);
                }

                _console.Clear();
                WriteGrayLine("mdview — mixed Markdown / native Spectre preview",
                    gray, width);
                WriteGrayLine(Path.GetFileName(path) + " | " +
                    snapshot.ByteLength.ToString(CultureInfo.InvariantCulture) +
                    " bytes | " +
                    snapshot.Lines.LineCount.ToString(CultureInfo.InvariantCulture) +
                    " physical lines", gray, width);
                _console.WriteLine();

                // The indexed renderer validates the source, layout and
                // native-widget heights before writing page content.
                MdViewMixedIndexedPage displayed;
                try
                {
                    displayed = await MdViewMixedVisualIndexedRenderer.RenderPageAsync(
                        index, cursor, _console, pageNumber, theme,
                        cancellationToken).ConfigureAwait(false);
                }
                catch (InvalidOperationException) when (
                    index.TerminalWidth != _console.Profile.Width ||
                    index.TerminalHeight != _console.Profile.Height ||
                    index.Unicode != _console.Profile.Capabilities.Unicode)
                {
                    // Resize during rendering; rebuild on the next pass.
                    continue;
                }

                if (displayed.VisualRows == 0)
                    _console.Write(new Text("(Empty page)" + Environment.NewLine));

                var lastSourceLine = displayed.Next.WrapRow > 0
                    ? displayed.Next.Line + 1 : displayed.Next.Line;
                _console.WriteLine();
                WriteGrayLine(
                    "Page " + (pageNumber + 1).ToString(CultureInfo.InvariantCulture) +
                    " of " + index.PageCount.ToString(CultureInfo.InvariantCulture) +
                    " | Source lines " +
                    (displayed.VisualRows == 0 ? "0" :
                        (displayed.Start.Line + 1).ToString(
                            CultureInfo.InvariantCulture)) +
                    "–" + lastSourceLine.ToString(CultureInfo.InvariantCulture) +
                    " of " +
                    snapshot.Lines.LineCount.ToString(CultureInfo.InvariantCulture),
                    gray, width);

                if (displayed.ContainsClippedCode)
                    WriteGrayLine("→ Code clipped horizontally (not wrapped).",
                        gray, width);
                if (feedback is not null)
                {
                    WriteGrayLine(feedback, gray, width);
                    feedback = null;
                }

                var choices = new List<string>(6);
                if (pageNumber + 1 < index.PageCount)
                    choices.Add(next);
                if (pageNumber > 0)
                    choices.Add(previous);
                if (snapshot.Lines.LineCount > 0)
                {
                    choices.Add(jump);
                    choices.Add(headings);
                    choices.Add(links);
                }
                choices.Add(exit);

                var chosen = await new SelectionPrompt<string>()
                    .Title("Mixed preview navigation")
                    .PageSize(Math.Clamp(choices.Count, 3, 7))
                    .AddChoices(choices)
                    .AddCancelResult(exit)
                    .ShowAsync(_console, cancellationToken).ConfigureAwait(false);

                if (chosen == exit)
                    return;

                if (chosen == next)
                {
                    pageNumber++;
                    continue;
                }

                if (chosen == previous)
                {
                    pageNumber--;
                    continue;
                }

                if (chosen == jump)
                {
                    var response = await new TextPrompt<string>(
                        "Physical source line (1-based, empty = cancel): ")
                        .AllowEmpty()
                        .ShowAsync(_console, cancellationToken).ConfigureAwait(false);
                    if (string.IsNullOrWhiteSpace(response))
                        continue;
                    if (!long.TryParse(response, NumberStyles.None,
                            CultureInfo.InvariantCulture, out var requested) ||
                        requested < 1 || requested > snapshot.Lines.LineCount)
                    {
                        feedback = "Source line is outside this document.";
                        continue;
                    }

                    pageNumber = await index.FindPageAsync(
                        requested - 1, cancellationToken).ConfigureAwait(false);
                    feedback = "Source line " +
                        requested.ToString(CultureInfo.InvariantCulture) +
                        " is on this canonical page.";
                    continue;
                }

                if (chosen == headings || chosen == links)
                {
                    try
                    {
                        if (navigation is null)
                        {
                            WriteGrayLine(
                                "Indexing validated source navigation on disk...",
                                gray, width);
                            navigation = await MdViewDiskNavigationIndex.CreateAsync(
                                snapshot, cancellationToken: cancellationToken)
                                .ConfigureAwait(false);
                        }

                        var kind = chosen == headings
                            ? MdViewIndexedItemKind.Heading
                            : MdViewIndexedItemKind.Link;
                        var selected = await picker.PickAsync(
                            navigation, kind, cancellationToken).ConfigureAwait(false);
                        if (selected is not null)
                        {
                            pageNumber = await index.FindPageAsync(
                                selected.LineNumber, cancellationToken)
                                .ConfigureAwait(false);
                            feedback = "Source line " +
                                (selected.LineNumber + 1).ToString(
                                    CultureInfo.InvariantCulture) +
                                " is on this canonical page.";
                        }
                    }
                    catch (Exception ex) when (ex is IOException or
                        InvalidDataException or UnauthorizedAccessException)
                    {
                        feedback = "Navigation index unavailable: " + ex.Message;
                    }
                }
            }
        }
        finally
        {
            if (navigation is not null)
                await navigation.DisposeAsync().ConfigureAwait(false);
            if (index is not null)
                await index.DisposeAsync().ConfigureAwait(false);
        }
    }

    private static bool CanDisplay(int width, int height) =>
        width >= 28 && height >= 18;

    private static int ContentRows(int terminalHeight, int requestedRows) =>
        Math.Min(128, Math.Min(Math.Max(1, terminalHeight - 16),
            requestedRows == 0 ? 128 : requestedRows));

    private void WriteGrayLine(string content, Style style, int terminalWidth)
    {
        var columns = Math.Clamp(terminalWidth - 2, 8, 4096);
        var line = MdViewTerminalWordWrap.Wrap(content, columns, code: true)[0];
        _console.Write(new Text(line + Environment.NewLine, style));
    }
}
