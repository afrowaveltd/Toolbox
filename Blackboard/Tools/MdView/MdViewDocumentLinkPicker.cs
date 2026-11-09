using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Explicit keyboard selection of previously parsed Markdown links.
/// This selector never opens or downloads a destination.
/// </summary>
public sealed class MdViewDocumentLinkPicker
{
    private readonly IAnsiConsole _console;

    /// <summary>Creates a selector on an injected console.</summary>
    /// <param name="console">Interactive Spectre console.</param>
    public MdViewDocumentLinkPicker(IAnsiConsole console) =>
        _console = console ?? throw new ArgumentNullException(nameof(console));

    /// <summary>
    /// Selects one document link, or returns null on Escape/Back.
    /// </summary>
    /// <param name="links">Already validated and resolved candidates.</param>
    /// <param name="cancellationToken">Cancels selection.</param>
    /// <returns>Explicitly selected link; null when cancelled or empty.</returns>
    public async Task<MdViewDocumentLink?> PickAsync(
        IReadOnlyList<MdViewDocumentLink> links,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(links);
        if (!_console.Profile.Capabilities.Interactive)
            throw new InvalidOperationException("Link selection requires an interactive terminal.");
        cancellationToken.ThrowIfCancellationRequested();
        if (links.Count == 0)
        {
            _console.Write(new Text("No supported document links found." + Environment.NewLine));
            return null;
        }

        var choices = links.Select(link => new Choice(link,
            $"{link.Label} — {link.Destination}")).ToList();
        var back = new Choice(null, "Back / cancel");
        choices.Add(back);

        var selected = await new SelectionPrompt<Choice>()
            .Title("Follow a Markdown link")
            .PageSize(Math.Clamp(choices.Count, 3, 15))
            .UseConverter(choice => Markup.Escape(choice.Label))
            .AddChoices(choices)
            .AddCancelResult(back)
            .ShowAsync(_console, cancellationToken);
        return selected.Link;
    }

    private sealed record Choice(MdViewDocumentLink? Link, string Label);
}
