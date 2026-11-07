using Afrowave.Toolbox.ExchangeHouse.Markdown;
using System.Globalization;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Afrowave.Toolbox.Blackboard.TextFiles;
using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Experimental bounded-source Markdown preview. Confirmed fenced-code
/// state survives page boundaries; other Markdown fragments are still
/// parsed independently and may lose list/quote context.
/// </summary>
public sealed class MdViewFormattedPreviewPager
{
    private readonly IAnsiConsole _console;

    /// <summary>Creates a pager that renders to the supplied terminal.</summary>
    /// <param name="console">Interactive terminal for formatted preview.</param>
    public MdViewFormattedPreviewPager(IAnsiConsole console) =>
        _console = console ?? throw new ArgumentNullException(nameof(console));

    /// <summary>
    /// Displays one bounded, partially container-aware source page at a time.
    /// The existing complete-document renderer remains the default CLI mode.
    /// </summary>
    /// <param name="path">Local UTF-8 Markdown document.</param>
    /// <param name="linesPerPage">Maximum physical lines per source page (1–128).</param>
    /// <param name="cancellationToken">Cancels snapshot validation or navigation.</param>
    public async Task RunAsync(string path, int linesPerPage = 24,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (linesPerPage is < 1 or > 128)
            throw new ArgumentOutOfRangeException(nameof(linesPerPage));
        if (!_console.Profile.Capabilities.Interactive)
            throw new InvalidOperationException(
                "Formatted preview paging requires an interactive terminal.");

        await using var snapshot = await Utf8ValidatedSnapshot.CreateAsync(
            path, cancellationToken: cancellationToken);
        await using var containerCursor = new MdViewContainerPageCursor(snapshot);
        var index = snapshot.Lines;
        long firstLine = 0;
        var previousPages = new Stack<long>();
        string? feedback = null;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var page = await index.ReadPageAsync(
                firstLine, linesPerPage, cancellationToken).ConfigureAwait(false);
            _console.Clear();
            _console.Write(new Text("mdview — experimental Markdown page preview" +
                Environment.NewLine));
            _console.Write(new Text(Path.GetFileName(path) + " | " +
                snapshot.ByteLength.ToString(CultureInfo.InvariantCulture) +
                " bytes | " + index.LineCount.ToString(CultureInfo.InvariantCulture) +
                " physical lines" + Environment.NewLine));
            _console.Write(new Text(
                "Incremental fence ownership; lists, quotes and visual rows remain experimental." +
                Environment.NewLine));
            if (feedback is not null)
            {
                _console.Write(new Text(feedback + Environment.NewLine));
                feedback = null;
            }

            if (page.Lines.Count == 0)
            {
                _console.Write(new Text("(Empty page)" + Environment.NewLine));
            }
            else
            {
                // Scanner context survives forward page boundaries. Only
                // confirmed fenced lines bypass the fragment parser so that
                // "# heading" or "- item" inside code stays literal.
                var classified = await containerCursor.ReadAsync(
                    page.FirstLine, page.Lines.Count, cancellationToken)
                    .ConfigureAwait(false);
                if (classified.Count != page.Lines.Count)
                    throw new InvalidDataException(
                        "Container scanner and validated line index disagree.");

                var theme = MdViewHighlighting.CreateTheme();
                var prose = new List<string>();
                void FlushProse()
                {
                    if (prose.Count == 0)
                        return;
                    MarkdownSpectreRenderer.Render(
                        _console, string.Join("\n", prose), theme);
                    prose.Clear();
                }

                foreach (var line in classified)
                {
                    if (line.Kind is MarkdownContainerLineKind.FenceOpen or
                        MarkdownContainerLineKind.CodeLine or
                        MarkdownContainerLineKind.FenceClose)
                    {
                        FlushProse();
                        // Literal Spectre Text: code cannot be mistaken for
                        // markdown headings, lists or executable markup.
                        _console.Write(new Text(
                            line.Source.Text + Environment.NewLine));
                    }
                    else
                        prose.Add(line.Source.Text);
                }
                FlushProse();
            }

            _console.Write(new Text("Source lines " +
                (page.Lines.Count == 0 ? "0" :
                    (page.FirstLine + 1).ToString(CultureInfo.InvariantCulture)) +
                "–" + page.NextLine.ToString(CultureInfo.InvariantCulture) +
                " of " + index.LineCount.ToString(CultureInfo.InvariantCulture) +
                Environment.NewLine));

            const string next = "Next page";
            const string previous = "Previous page";
            const string jump = "Go to source line";
            const string exit = "Exit preview";

            var choices = new List<string>();
            if (page.HasMore) choices.Add(next);
            if (previousPages.Count != 0) choices.Add(previous);
            if (index.LineCount != 0) choices.Add(jump);
            choices.Add(exit);

            var chosen = await new SelectionPrompt<string>()
                .Title("Preview navigation")
                .PageSize(Math.Clamp(choices.Count, 3, 8))
                .AddChoices(choices)
                .AddCancelResult(exit)
                .ShowAsync(_console, cancellationToken);

            if (chosen == exit)
                return;
            if (chosen == next)
            {
                previousPages.Push(firstLine);
                firstLine = page.NextLine;
            }
            else if (chosen == previous)
            {
                firstLine = previousPages.Pop();
            }
            else if (chosen == jump)
            {
                var input = await new TextPrompt<string>(
                    "Go to physical source line (1-based, empty = cancel): ")
                    .AllowEmpty().ShowAsync(_console, cancellationToken);
                if (string.IsNullOrWhiteSpace(input))
                    continue;
                if (!long.TryParse(input, NumberStyles.None,
                        CultureInfo.InvariantCulture, out var number) ||
                    number < 1 || number > index.LineCount)
                {
                    feedback = "Source line is outside this document.";
                    continue;
                }

                previousPages.Clear();
                firstLine = number - 1;
            }
        }
    }
}
