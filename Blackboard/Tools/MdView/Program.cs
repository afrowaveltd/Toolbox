using Afrowave.Toolbox.Blackboard.TextFiles;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Spectre.Console;

return await RunAsync(args);

static async Task<int> RunAsync(string[] args)
{
    MdViewCommand initial;
    try { initial = MdViewCommand.Parse(args); }
    catch (ArgumentException ex)
    {
        Console.Error.WriteLine(ex.Message);
        PrintHelp();
        return 2;
    }

    if (initial.Kind == MdViewSourceKind.Help)
    {
        PrintHelp();
        return 0;
    }

    var console = AnsiConsole.Console;
    // A piped script must never block after printing a document. The additional
    // redirect guards are kept at the executable boundary for TestConsole tests.
    var canWait = console.Profile.Capabilities.Interactive &&
                  !Console.IsInputRedirected && !Console.IsOutputRedirected;

    try
    {
        return await new MdViewSession(console).RunAsync(
            initial,
            token => new MdViewInteractiveSourcePicker(console)
                .PickAsync(Environment.CurrentDirectory, token),
            (command, token) => DisplayAsync(command, console, token),
            waitForCompletion: canWait);
    }
    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or
                               ArgumentException or InvalidOperationException)
    {
        Console.Error.WriteLine("Interactive selection failed: " + ex.Message);
        return 1;
    }
}

static async Task<MdViewDisplayResult> DisplayAsync(
    MdViewCommand command, IAnsiConsole console, CancellationToken cancellationToken)
{
    try
    {
        if (command.Kind == MdViewSourceKind.GitHubLinkBrowser)
        {
            using var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = System.Net.DecompressionMethods.None
            };
            using var http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
            var cacheRoot = CacheDirectory.GetDefaultPath();
            // The repository catalog is independently bounded and can exceed
            // the 1 MiB per-document display cap of the GitHub link browser.
            if (command.Repository is null)
            {
                var selected = await new MdViewGitHubRepositoryPicker(
                    console,
                    new GitHubRepositoryCatalogSource(
                        http, new VerifiedDocumentCache(cacheRoot)))
                    .PickAsync(command.Value!, cancellationToken);
                if (selected is null)
                    return new MdViewDisplayResult(0, false);

                // Picker selection is historically a one-shot README command.
                // In this explicit browsing branch it becomes a browser target.
                command = selected with { Kind = MdViewSourceKind.GitHubLinkBrowser };
            }

            var cache = new VerifiedDocumentCache(cacheRoot,
                maximumDocumentBytes: MdViewGitHubNavigationSession.DefaultMaximumDocumentBytes);
            await new MdViewGitHubLinkBrowser(console,
                new GitHubRepositoryFileSource(http, cache))
                .RunAsync(command.Value!, command.Repository!, cancellationToken);
            return new MdViewDisplayResult(0, false);
        }

        if (command.Kind == MdViewSourceKind.HttpsLinkBrowser)
        {
            // The new interactive HTTPS browser stays independent of
            // normal one-shot remote Markdown rendering.
            using var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = System.Net.DecompressionMethods.None
            };
            using var http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
            var cache = new VerifiedDocumentCache(CacheDirectory.GetDefaultPath(),
                maximumDocumentBytes: MdViewHttpsNavigationSession.DefaultMaximumDocumentBytes);
            await new MdViewHttpsLinkBrowser(console,
                new HttpMarkdownDocumentSource(http, cache))
                .RunAsync(command.Value!, cancellationToken);
            return new MdViewDisplayResult(0, false);
        }

        if (command.Kind == MdViewSourceKind.LocalLinkBrowser)
        {
            await new MdViewLocalLinkBrowser(console).RunAsync(
                command.Value!, cancellationToken);
            return new MdViewDisplayResult(0, false);
        }

        if (command.Kind == MdViewSourceKind.FormattedPreview)
        {
            // Explicit experimental mode: bounded, independently parsed pages.
            await new MdViewFormattedPreviewPager(console).RunAsync(
                command.Value!, cancellationToken: cancellationToken);
            return new MdViewDisplayResult(0, false);
        }

        if (command.Kind == MdViewSourceKind.SourcePager)
        {
            // This source-line viewer already owns its own Next/Back/Exit prompts.
            await new MdViewSourcePager(console).RunAsync(
                command.Value!, cancellationToken: cancellationToken);
            return new MdViewDisplayResult(0, false);
        }

        string content;
        if (command.Kind == MdViewSourceKind.Local)
        {
            var path = Path.GetFullPath(command.Value ?? "");
            if (!MdViewInteractiveSourcePicker.IsMarkdownPath(path))
            {
                Console.Error.WriteLine("Local source must end in .md or .markdown.");
                return new MdViewDisplayResult(2, false);
            }
            var result = await TextFilePreviewReader.ReadCompleteAsync(
                path, cancellationToken);
            if (result.Kind == TextPreviewKind.Empty)
            {
                Console.WriteLine("(Empty Markdown document)");
                return new MdViewDisplayResult(0, true);
            }
            if (result.Kind != TextPreviewKind.Text)
            {
                Console.Error.WriteLine("Invalid encoding, binary input or unsafe terminal control characters.");
                return new MdViewDisplayResult(1, false);
            }
            content = result.Content;
        }
        else
        {
            using var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = System.Net.DecompressionMethods.None
            };
            using var http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
            var cache = new VerifiedDocumentCache(CacheDirectory.GetDefaultPath());

            if (command.Kind == MdViewSourceKind.GitHub && command.Repository is null)
            {
                var chosen = await new MdViewGitHubRepositoryPicker(
                    console, new GitHubRepositoryCatalogSource(http, cache))
                    .PickAsync(command.Value!, cancellationToken);
                if (chosen is null)
                    return new MdViewDisplayResult(0, false); // user cancelled before opening
                command = chosen;
            }

            HttpDocumentResult result;
            if (command.Kind == MdViewSourceKind.GitHub)
                result = await new GitHubReadmeSource(http, cache)
                    .OpenAsync(command.Value!, command.Repository!, cancellationToken);
            else if (command.Kind == MdViewSourceKind.Url)
                result = await new HttpMarkdownDocumentSource(http, cache)
                    .OpenAsync(new Uri(command.Value!, UriKind.Absolute), cancellationToken);
            else
                throw new ArgumentException("Unrecognized source type.");

            if (result.Document is null)
            {
                Console.Error.WriteLine(result.Warning ?? "Document is unavailable.");
                return new MdViewDisplayResult(1, false);
            }
            Console.Error.WriteLine("Source: " + result.State +
                (result.State == HttpDocumentState.Stale ? " (offline or stale copy)" : ""));
            if (result.Warning is not null)
                Console.Error.WriteLine(result.Warning);

            // The full cached text is still loaded; visual Markdown paging is a separate milestone.
            content = await File.ReadAllTextAsync(
                result.Document.ContentPath, cancellationToken);
        }

        if (content.Length > 1024 * 1024)
            Console.Error.WriteLine("Large Markdown document: current renderer loads the entire text in memory.");
        MarkdownSpectreRenderer.Render(console, content, MdViewHighlighting.CreateTheme());
        return new MdViewDisplayResult(0, true);
    }
    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or
                               ArgumentException or InvalidOperationException or
                               InvalidDataException or HttpRequestException or
                               UriFormatException or System.Text.DecoderFallbackException)
    {
        Console.Error.WriteLine("Unable to display the requested Markdown document: " + ex.Message);
        return new MdViewDisplayResult(1, false);
    }
}

static void PrintHelp()
{
    Console.WriteLine("mdview [PATH | URL | github:OWNER/REPOSITORY]");
    Console.WriteLine("mdview local PATH  (complete rendered Markdown)");
    Console.WriteLine("mdview source PATH (bounded-memory raw source pager)");
    Console.WriteLine("mdview browse PATH (opt-in local Markdown link navigation)");
    Console.WriteLine("mdview browse-url HTTPS_URL (opt-in same-origin HTTPS link navigation)");
    Console.WriteLine("mdview browse-git OWNER REPOSITORY (opt-in GitHub Markdown navigation)");
    Console.WriteLine("mdview browse-git OWNER/REPOSITORY");
    Console.WriteLine("mdview preview PATH (experimental bounded Markdown page preview)");
    Console.WriteLine("mdview url HTTPS_URL");
    Console.WriteLine("mdview OWNER           (browse public GitHub repositories)");
    Console.WriteLine("mdview github:OWNER    (browse public GitHub repositories)");
    Console.WriteLine("mdview git OWNER       (browse public GitHub repositories)");
    Console.WriteLine("mdview git OWNER REPOSITORY");
    Console.WriteLine("mdview git OWNER/REPOSITORY");
    Console.WriteLine("mdview --help");
    Console.WriteLine("No arguments: interactive Spectre source menu with a local Markdown FileBrowser.");
}
