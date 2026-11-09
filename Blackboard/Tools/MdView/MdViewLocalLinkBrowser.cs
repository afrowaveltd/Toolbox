using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Opt-in keyboard-first navigation of local Markdown documents using one
/// validated snapshot, bounded history and explicit link selection.
/// </summary>
/// <remarks>
/// The current whole-document renderer and link extractor are deliberately
/// capped at 1 MiB of UTF-8 source per visited document. Large documents
/// should use the separate indexed preview until streaming link indexing
/// becomes available. No HTTP/GitHub or external links are followed here.
/// </remarks>
public sealed class MdViewLocalLinkBrowser
{
    /// <summary>Maximum UTF-8 source bytes rendered in this browsing mode (1 MiB).</summary>
    public const long MaximumDocumentBytes = 1024 * 1024;

    private readonly IAnsiConsole _console;

    /// <summary>Creates the opt-in browser using an injected terminal.</summary>
    /// <param name="console">Interactive Spectre.Console terminal.</param>
    public MdViewLocalLinkBrowser(IAnsiConsole console) =>
        _console = console ?? throw new ArgumentNullException(nameof(console));

    /// <summary>
    /// Opens a local Markdown document and allows Follow / Back / Forward / Exit.
    /// Follows only links resolved within the initial document directory.
    /// </summary>
    /// <param name="path">Existing .md or .markdown source file.</param>
    /// <param name="cancellationToken">Cancels reading and terminal prompts.</param>
    public async Task RunAsync(
        string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!_console.Profile.Capabilities.Interactive)
            throw new InvalidOperationException(
                "Markdown link browsing requires an interactive terminal.");
        var file = Path.GetFullPath(path);
        var root = Path.GetDirectoryName(file)!;
        var origin = MdViewDocumentOrigin.FromLocal(file, root);
        await using var navigation = await MdViewLocalNavigationSession.OpenAsync(
            origin, maximumSnapshotBytes: MaximumDocumentBytes,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        var picker = new MdViewDocumentLinkPicker(_console);
        string? message = null;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _console.Clear();
            var current = navigation.Current;
            _console.Write(new Text(
                "Markdown: " + current.Target.Origin.Location +
                (current.Target.Fragment is null or "" ? "" : "#" + current.Target.Fragment) +
                Environment.NewLine));
            if (message is not null)
            {
                _console.Write(new Text(message + Environment.NewLine));
                message = null;
            }

            using var reader = current.Snapshot.OpenReader();
            var markdown = await reader.ReadToEndAsync(cancellationToken)
                .ConfigureAwait(false);
            if (markdown.Length > MdViewDocumentLinkExtractor.MaximumDocumentCharacters)
                throw new InvalidDataException("Document exceeds the bounded link browser limit.");

            MarkdownSpectreRenderer.Render(
                _console, markdown, MdViewHighlighting.CreateTheme());

            var links = MdViewDocumentLinkExtractor.Extract(
                markdown, current.Target.Origin);
            var choices = new List<string>();
            if (links.Count > 0)
                choices.Add("Follow a link");
            if (navigation.History.CanGoBack)
                choices.Add("Back");
            if (navigation.History.CanGoForward)
                choices.Add("Forward");
            choices.Add("Exit browser");

            var action = await new SelectionPrompt<string>()
                .Title("Markdown navigation")
                .PageSize(Math.Clamp(choices.Count, 3, 8))
                .AddChoices(choices)
                .AddCancelResult("Exit browser")
                .ShowAsync(_console, cancellationToken).ConfigureAwait(false);
            if (action == "Exit browser")
                return;

            try
            {
                switch (action)
                {
                    case "Follow a link":
                    {
                        var selected = await picker.PickAsync(
                            links, cancellationToken).ConfigureAwait(false);
                        if (selected is not null)
                            await navigation.FollowAsync(
                                selected.Destination, cancellationToken).ConfigureAwait(false);
                        break;
                    }
                    case "Back":
                        await navigation.BackAsync(cancellationToken).ConfigureAwait(false);
                        break;
                    case "Forward":
                        await navigation.ForwardAsync(cancellationToken).ConfigureAwait(false);
                        break;
                }
            }
            catch (Exception ex) when (ex is IOException or InvalidDataException or
                ArgumentException or UnauthorizedAccessException)
            {
                // Failed targets must not discard the currently validated page.
                message = "Unable to follow Markdown link: " + ex.Message;
            }
        }
    }
}
