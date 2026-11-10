using System.Text;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache;
using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Interactive GitHub Markdown document browsing using official metadata,
/// exact-ref file retrieval, verified cache and bounded navigation history.
/// </summary>
public sealed class MdViewGitHubLinkBrowser
{
    private readonly IAnsiConsole _console;
    private readonly GitHubRepositoryFileSource _source;

    /// <summary>Creates a terminal browser with a caller-owned GitHub file provider.</summary>
    /// <param name="console">Interactive terminal.</param>
    /// <param name="source">GitHub file provider with validated cache.</param>
    public MdViewGitHubLinkBrowser(IAnsiConsole console, GitHubRepositoryFileSource source)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _source = source ?? throw new ArgumentNullException(nameof(source));
    }

    /// <summary>Browses the actual preferred README and links in its repository.</summary>
    /// <param name="owner">GitHub account name.</param>
    /// <param name="repository">Public repository name.</param>
    /// <param name="cancellationToken">Cancels terminal and network operations.</param>
    public async Task RunAsync(
        string owner, string repository, CancellationToken cancellationToken = default)
    {
        if (!_console.Profile.Capabilities.Interactive)
            throw new InvalidOperationException("GitHub Markdown browsing requires an interactive terminal.");

        var navigation = await MdViewGitHubNavigationSession.OpenAsync(
            _source, owner, repository, cancellationToken: cancellationToken).ConfigureAwait(false);
        var picker = new MdViewDocumentLinkPicker(_console);
        string? message = null;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _console.Clear();
            var current = navigation.Current;
            var origin = current.Target.Origin;
            _console.Write(new Text(
                "GitHub: " + origin.Owner + "/" + origin.Repository +
                " @ " + origin.Ref + " / " + origin.Location +
                (current.Target.Fragment is { Length: > 0 } anchor ? "#" + anchor : "") +
                Environment.NewLine));
            _console.Write(new Text(
                "README metadata: " + navigation.Readme.State +
                (navigation.Readme.State == HttpDocumentState.Stale ? " (offline/stale)" : "") +
                Environment.NewLine));
            if (navigation.Readme.Warning is not null)
                _console.Write(new Text(navigation.Readme.Warning + Environment.NewLine));
            _console.Write(new Text(
                "Source: " + current.Result.State +
                (current.Result.State == HttpDocumentState.Stale ? " (offline/stale)" : "") +
                Environment.NewLine));
            if (current.Result.Warning is not null)
                _console.Write(new Text(current.Result.Warning + Environment.NewLine));
            if (message is not null)
            {
                _console.Write(new Text(message + Environment.NewLine));
                message = null;
            }

            var markdown = await File.ReadAllTextAsync(
                current.Result.Document!.ContentPath, new UTF8Encoding(false, true),
                cancellationToken).ConfigureAwait(false);
            if (markdown.Length > MdViewDocumentLinkExtractor.MaximumDocumentCharacters)
                throw new InvalidDataException("GitHub document exceeds the link extractor size limit.");

            var links = MdViewDocumentLinkExtractor.Extract(markdown, origin);
            _console.Write(new Text(
                "Navigable Markdown links: " + links.Count +
                (links.Count > 0
                    ? " (select 'Follow a link' in the menu below the document)"
                    : " (no supported repository-relative Markdown links)") +
                Environment.NewLine));
            if (links.Count > 0)
            {
                // A long README can push the navigation menu below the
                // terminal viewport. Show actual link labels up front, not
                // merely a count. The full list remains in the link picker.
                _console.Write(new Text("Available links (first 3):" + Environment.NewLine));
                for (var index = 0; index < Math.Min(3, links.Count); index++)
                {
                    var link = links[index];
                    _console.Write(new Text("  " + (index + 1) + ". " +
                        ShortLabel(link.Label) + " → " +
                        ShortLabel(link.Destination) + Environment.NewLine));
                }
            }

            var rendered = markdown;
            if (current.Target.Fragment is { Length: > 0 } fragment)
            {
                var heading = MdViewHeadingIndex.Find(
                    MdViewHeadingIndex.Create(markdown), fragment);
                if (heading is null)
                    _console.Write(new Text(
                        "Heading anchor not found: #" + fragment + Environment.NewLine));
                else
                {
                    rendered = markdown[heading.SourceOffset..];
                    _console.Write(new Text(
                        "Jumped to heading: " + heading.Title + Environment.NewLine));
                }
            }
            MarkdownSpectreRenderer.Render(_console, rendered, MdViewHighlighting.CreateTheme());

            var choices = new List<string>();
            if (links.Count > 0)
                choices.Add("Follow a link");
            if (navigation.History.CanGoBack)
                choices.Add("Back");
            if (navigation.History.CanGoForward)
                choices.Add("Forward");
            choices.Add("Exit browser");

            var action = await new SelectionPrompt<string>()
                .Title("GitHub Markdown navigation")
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
                // Previous validated document and history survive any failed link.
                message = "Unable to follow GitHub Markdown link: " + ex.Message;
            }
        }

    }

    private static string ShortLabel(string value) =>
        value.Length > 64 ? value[..64] + "…" : value;
}
