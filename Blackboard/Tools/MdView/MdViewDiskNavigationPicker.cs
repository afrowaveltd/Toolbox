using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Keyboard selection of disk-indexed source locations in bounded batches.
/// The selector never loads a whole source file or follows a link.
/// </summary>
public sealed class MdViewDiskNavigationPicker
{
    /// <summary>Number of displayed navigation entries per selection page.</summary>
    public const int ResultsPerPage = 12;

    private readonly IAnsiConsole _console;

    /// <summary>Creates a picker using the caller's interactive terminal.</summary>
    /// <param name="console">Spectre terminal instance.</param>
    public MdViewDiskNavigationPicker(IAnsiConsole console) =>
        _console = console ?? throw new ArgumentNullException(nameof(console));

    /// <summary>Finds one indexed heading or link by browsing small result pages.</summary>
    /// <param name="index">Completed, caller-owned disk navigation index.</param>
    /// <param name="kind">Heading or link source positions.</param>
    /// <param name="cancellationToken">Cancels prompt or disk access.</param>
    /// <returns>Chosen source position, or null on Escape/Back/empty catalog.</returns>
    public async Task<MdViewIndexedNavigationItem?> PickAsync(
        MdViewDiskNavigationIndex index, MdViewIndexedItemKind kind,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(index);
        if (!_console.Profile.Capabilities.Interactive)
            throw new InvalidOperationException(
                "Disk navigation selection requires an interactive terminal.");
        var total = kind switch
        {
            MdViewIndexedItemKind.Heading => index.HeadingCount,
            MdViewIndexedItemKind.Link => index.LinkCount,
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
        cancellationToken.ThrowIfCancellationRequested();
        if (total == 0)
        {
            _console.Write(new Text("No indexed " +
                (kind == MdViewIndexedItemKind.Heading ? "headings" : "links") +
                " were found." + Environment.NewLine));
            return null;
        }

        long first = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var items = await index.ReadAsync(
                kind, first, ResultsPerPage, cancellationToken).ConfigureAwait(false);
            var choices = new List<Choice>(items.Count + 3);
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var label = kind == MdViewIndexedItemKind.Heading
                    ? item.Label
                    : item.Label + " → " + Clip(item.Destination ?? "");
                choices.Add(new Choice(item, Movement.None,
                    (first + i + 1) + ". " + Clip(label) +
                    " (line " + (item.LineNumber + 1) + ")"));
            }
            if (first + items.Count < total)
                choices.Add(new Choice(null, Movement.Next, "Next results"));
            if (first != 0)
                choices.Add(new Choice(null, Movement.Previous, "Previous results"));
            var back = new Choice(null, Movement.Back, "Back / cancel");
            choices.Add(back);

            var selection = await new SelectionPrompt<Choice>()
                .Title("Indexed " +
                    (kind == MdViewIndexedItemKind.Heading ? "headings" : "links") +
                    " (" + (first + 1) + "–" + (first + items.Count) +
                    " of " + total + ")")
                .PageSize(Math.Clamp(choices.Count, 3, 15))
                .UseConverter(choice => Markup.Escape(choice.Label))
                .AddChoices(choices)
                .AddCancelResult(back)
                .ShowAsync(_console, cancellationToken).ConfigureAwait(false);

            if (selection.Item is not null)
                return selection.Item;
            switch (selection.Move)
            {
                case Movement.Next:
                    first += items.Count;
                    break;
                case Movement.Previous:
                    first = Math.Max(0, first - ResultsPerPage);
                    break;
                default:
                    return null;
            }
        }
    }

    private static string Clip(string text)
    {
        const int max = 96;
        if (text.Length <= max)
            return text;
        var length = char.IsHighSurrogate(text[max - 1]) ? max - 1 : max;
        return text[..length] + "…";
    }

    private enum Movement
    {
        None,
        Next,
        Previous,
        Back
    }

    private sealed record Choice(
        MdViewIndexedNavigationItem? Item, Movement Move, string Label);
}
