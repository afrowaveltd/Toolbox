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
        var theme = MdViewHighlighting.CreateTheme();
        string? message = null;
        string? markdown = null;
        string? activeSource = null;
        string? activeDigest = null;
        MdViewSmallMarkdownPageIndex? pageIndex = null;
        IReadOnlyList<MdViewDocumentLink> links = [];
        IReadOnlyList<MdViewHeadingAnchor> headings = [];
        MdViewResolvedLink? displayedTarget = null;
        int firstLine = 0;

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

            // Only reload/reparse after a verified source identity or content
            // digest changes. Page changes do not reread the whole document.
            var source = current.Result.Document!;
            if (pageIndex is null || markdown is null ||
                activeSource != origin.Location || activeDigest != source.Sha256)
            {
                markdown = await File.ReadAllTextAsync(
                    source.ContentPath, new UTF8Encoding(false, true),
                    cancellationToken).ConfigureAwait(false);
                if (markdown.Length > MdViewDocumentLinkExtractor.MaximumDocumentCharacters)
                    throw new InvalidDataException(
                        "GitHub document exceeds the bounded small-document browser limit.");
                pageIndex = new MdViewSmallMarkdownPageIndex(markdown);
                links = MdViewDocumentLinkExtractor.Extract(markdown, origin);
                headings = MdViewHeadingIndex.Create(markdown);
                activeSource = origin.Location;
                activeDigest = source.Sha256;
                displayedTarget = null;
            }

            if (displayedTarget != current.Target)
            {
                firstLine = 0;
                if (current.Target.Fragment is { Length: > 0 } fragment)
                {
                    var heading = MdViewHeadingIndex.Find(headings, fragment);
                    if (heading is null)
                        message = "Heading anchor not found: #" + fragment;
                    else
                        firstLine = pageIndex.FindSourceLine(heading.SourceOffset);
                }
                displayedTarget = current.Target;
            }

            _console.Write(new Text(
                "Navigable Markdown links: " + links.Count +
                (links.Count > 0
                    ? " (select 'Follow a link' in the menu below the document)"
                    : " (no supported repository-relative Markdown links)") +
                Environment.NewLine));
            if (links.Count > 0)
            {
                _console.Write(new Text("Available links (first 3):" + Environment.NewLine));
                for (var index = 0; index < Math.Min(3, links.Count); index++)
                {
                    var link = links[index];
                    _console.Write(new Text("  " + (index + 1) + ". " +
                        ShortLabel(link.Label) + " → " +
                        ShortLabel(link.Destination) + Environment.NewLine));
                }
            }

            var paged = pageIndex.LineCount > MdViewSmallMarkdownPageIndex.DefaultPageLines;
            MdViewMarkdownSourcePage? visiblePage = null;
            if (paged)
            {
                visiblePage = pageIndex.GetPage(firstLine);
                _console.Write(new Text(
                    "Page: source lines " + (visiblePage.FirstLine + 1) +
                    "–" + visiblePage.NextLine + " / " + visiblePage.LineCount +
                    Environment.NewLine));
                MdViewSmallMarkdownPageIndex.RenderPage(_console, visiblePage, theme);
                if (visiblePage.TruncatedLine)
                    _console.Write(new Text(
                        "Long source line shortened on this page; use source pager for full text." +
                        Environment.NewLine));
            }
            else
            {
                // Keep the familiar complete-document presentation and
                // keyboard actions for short Markdown files.
                var rendered = markdown;
                if (current.Target.Fragment is { Length: > 0 } fragment)
                {
                    var heading = MdViewHeadingIndex.Find(headings, fragment);
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
                MarkdownSpectreRenderer.Render(_console, rendered, theme);
            }

            var choices = new List<string>();
            if (links.Count > 0)
                choices.Add("Follow a link");
            if (navigation.History.CanGoBack)
                choices.Add("Back");
            if (navigation.History.CanGoForward)
                choices.Add("Forward");
            if (paged && visiblePage!.HasMore)
                choices.Add("Next page");
            if (paged && firstLine > 0)
                choices.Add("Previous page");
            if (paged && headings.Count > 0)
                choices.Add("Go to heading");
            if (paged)
                choices.Add("Go to source line");
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
                    case "Next page":
                        firstLine = visiblePage!.NextLine;
                        break;
                    case "Previous page":
                        firstLine = Math.Max(0,
                            firstLine - MdViewSmallMarkdownPageIndex.DefaultPageLines);
                        break;
                    case "Go to heading":
                    {
                        // Keep selection bounded independently of source size.
                        var shown = headings.Take(512).ToArray();
                        var options = shown.Select((heading, index) =>
                            (index + 1) + ". " + heading.Title + " (#" + heading.Anchor + ")")
                            .ToList();
                        const string back = "Back / cancel";
                        options.Add(back);
                        var picked = await new SelectionPrompt<string>()
                            .Title("Choose a heading (first 512)")
                            .PageSize(Math.Clamp(options.Count, 3, 15))
                            .UseConverter(Markup.Escape)
                            .AddChoices(options)
                            .AddCancelResult(back)
                            .ShowAsync(_console, cancellationToken).ConfigureAwait(false);
                        var headingIndex = options.IndexOf(picked);
                        if (headingIndex >= 0 && headingIndex < shown.Length)
                            firstLine = pageIndex!.FindSourceLine(shown[headingIndex].SourceOffset);
                        break;
                    }
                    case "Go to source line":
                    {
                        var input = await new TextPrompt<string>(
                            "Source line (1-based, empty = cancel): ")
                            .AllowEmpty().ShowAsync(_console, cancellationToken)
                            .ConfigureAwait(false);
                        if (input.Length > 0)
                        {
                            if (int.TryParse(input, System.Globalization.NumberStyles.None,
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    out var number) &&
                                number >= 1 && number <= pageIndex!.LineCount)
                                firstLine = number - 1;
                            else
                                message = "Source line is outside this document.";
                        }
                        break;
                    }
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
