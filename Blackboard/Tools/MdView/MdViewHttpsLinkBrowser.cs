using System.Text;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache;
using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Opt-in HTTPS Markdown link browser. Reads only fully validated cached
/// documents and requires explicit same-origin link selection.
/// </summary>
public sealed class MdViewHttpsLinkBrowser
{
    private readonly IAnsiConsole _console;
    private readonly HttpMarkdownDocumentSource _source;

    /// <summary>Creates a browser with caller-owned HTTP/cache infrastructure.</summary>
    /// <param name="console">Interactive terminal.</param>
    /// <param name="source">Verified document adapter; disable automatic redirects on its client.</param>
    public MdViewHttpsLinkBrowser(IAnsiConsole console, HttpMarkdownDocumentSource source)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _source = source ?? throw new ArgumentNullException(nameof(source));
    }

    /// <summary>Opens and navigates same-origin HTTPS Markdown documents.</summary>
    /// <param name="url">Initial HTTPS Markdown URL without query or fragment.</param>
    /// <param name="cancellationToken">Cancels terminal prompts and network operations.</param>
    public async Task RunAsync(string url, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        if (!_console.Profile.Capabilities.Interactive)
            throw new InvalidOperationException("HTTPS Markdown browsing requires an interactive terminal.");

        var origin = MdViewDocumentOrigin.FromHttps(url);
        var navigation = await MdViewHttpsNavigationSession.OpenAsync(
            _source, origin, cancellationToken: cancellationToken).ConfigureAwait(false);
        var picker = new MdViewDocumentLinkPicker(_console);
        string? message = null;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _console.Clear();
            var current = navigation.Current;
            _console.Write(new Text("Markdown: " + current.Target.Origin.Location +
                (current.Target.Fragment is { Length: > 0 } fragment ? "#" + fragment : "") +
                Environment.NewLine));
            _console.Write(new Text("Source: " + current.Result.State +
                (current.Result.State == HttpDocumentState.Stale
                    ? " (offline or stale verified copy)" : "") +
                Environment.NewLine));
            if (current.Result.Warning is not null)
                _console.Write(new Text(current.Result.Warning + Environment.NewLine));
            if (message is not null)
            {
                _console.Write(new Text(message + Environment.NewLine));
                message = null;
            }

            // The source adapter has already fully validated the immutable
            // cache blob and the navigation session enforced the 1 MiB cap.
            var markdown = await File.ReadAllTextAsync(
                current.Result.Document!.ContentPath, new UTF8Encoding(false, true),
                cancellationToken).ConfigureAwait(false);
            if (markdown.Length > MdViewDocumentLinkExtractor.MaximumDocumentCharacters)
                throw new InvalidDataException("HTTPS document exceeds the bounded link extractor limit.");

            var rendered = markdown;
            if (current.Target.Fragment is { Length: > 0 } headingFragment)
            {
                var heading = MdViewHeadingIndex.Find(
                    MdViewHeadingIndex.Create(markdown), headingFragment);
                if (heading is null)
                    _console.Write(new Text(
                        "Heading anchor not found: #" + headingFragment + Environment.NewLine));
                else
                {
                    rendered = markdown[heading.SourceOffset..];
                    _console.Write(new Text(
                        "Jumped to heading: " + heading.Title + Environment.NewLine));
                }
            }
            MarkdownSpectreRenderer.Render(_console, rendered, MdViewHighlighting.CreateTheme());

            var links = MdViewDocumentLinkExtractor.Extract(markdown, current.Target.Origin);
            var choices = new List<string>();
            if (links.Count > 0)
                choices.Add("Follow a link");
            if (navigation.History.CanGoBack)
                choices.Add("Back");
            if (navigation.History.CanGoForward)
                choices.Add("Forward");
            choices.Add("Exit browser");

            var action = await new SelectionPrompt<string>()
                .Title("Markdown navigation (HTTPS, same origin)")
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
                        var selected = await picker.PickAsync(
                            links, cancellationToken).ConfigureAwait(false);
                        if (selected is not null)
                            await navigation.FollowAsync(
                                selected.Destination, cancellationToken).ConfigureAwait(false);
                        break;
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
                // A failed network/cache/size check must not replace the
                // last valid document or mutate its navigation history.
                message = "Unable to follow HTTPS Markdown link: " + ex.Message;
            }
        }
    }
}
