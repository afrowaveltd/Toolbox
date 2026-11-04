using System.Globalization;
using Afrowave.Toolbox.Blackboard.TextFiles;
using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Read-only, low-memory navigation of raw UTF-8 source lines using an on-disk
/// offset index. This is intentionally not a rendered Markdown pager.
/// </summary>
public sealed class MdViewSourcePager
{
    private readonly IAnsiConsole _console;

    /// <summary>Use an interactive Spectre.Console terminal.</summary>
    public MdViewSourcePager(IAnsiConsole console) =>
        _console = console ?? throw new ArgumentNullException(nameof(console));

    /// <summary>
    /// Browse physical lines with Next, Previous, Jump and Exit. Every displayed
    /// line was validated before the first page; no entire document is loaded.
    /// </summary>
    public async Task RunAsync(string path, int linesPerPage = 24,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (linesPerPage is < 1 or > 128)
            throw new ArgumentOutOfRangeException(nameof(linesPerPage));
        if (!_console.Profile.Capabilities.Interactive)
            throw new InvalidOperationException(
                "Source paging requires an interactive terminal; use the normal mdview command for output.");

        await using var index = await Utf8DiskLineIndex.OpenAsync(
            path, cancellationToken: cancellationToken);

        long firstLine = 0;
        var previousPages = new Stack<long>();
        string? feedback = null;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var page = await index.ReadPageAsync(firstLine, linesPerPage, cancellationToken);
            _console.Clear();
            _console.Write(new Text("mdview — raw source pager (not Markdown rendering)" +
                Environment.NewLine));
            _console.Write(new Text(Path.GetFileName(path) + " | " + index.ByteLength +
                " bytes | " + index.LineCount + " lines" + Environment.NewLine));
            if (feedback is not null)
            {
                _console.Write(new Text(feedback + Environment.NewLine));
                feedback = null;
            }

            if (page.Lines.Count == 0)
                _console.Write(new Text("(No lines on this page)" + Environment.NewLine));
            else
                for (var i = 0; i < page.Lines.Count; i++)
                    _console.Write(new Text((page.FirstLine + i + 1).ToString(
                        CultureInfo.InvariantCulture) + ": " + page.Lines[i] +
                        Environment.NewLine));

            _console.Write(new Text("Lines " +
                (page.Lines.Count == 0 ? "0" : (page.FirstLine + 1).ToString(
                    CultureInfo.InvariantCulture)) + "–" +
                page.NextLine.ToString(CultureInfo.InvariantCulture) + " of " +
                index.LineCount.ToString(CultureInfo.InvariantCulture) +
                Environment.NewLine));

            const string next = "Next page";
            const string previous = "Previous page";
            const string jump = "Go to line";
            const string exit = "Exit source pager";

            var options = new List<string>();
            if (page.HasMore) options.Add(next);
            if (previousPages.Count > 0) options.Add(previous);
            if (index.LineCount > 0) options.Add(jump);
            options.Add(exit);

            var chosen = await new SelectionPrompt<string>()
                .Title("Source navigation")
                .PageSize(Math.Clamp(options.Count, 3, 8))
                .AddChoices(options)
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
                var input = await new TextPrompt<string>("Go to physical line (1-based, empty = cancel): ")
                    .AllowEmpty().ShowAsync(_console, cancellationToken);
                if (string.IsNullOrWhiteSpace(input))
                    continue;
                if (!long.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture,
                        out var number) || number < 1 || number > index.LineCount)
                {
                    feedback = "Line number is outside this document.";
                    continue;
                }

                previousPages.Clear();
                firstLine = number - 1;
            }
        }
    }
}
