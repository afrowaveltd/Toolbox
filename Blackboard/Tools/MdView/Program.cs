using Afrowave.Toolbox.Blackboard.TextFiles;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Spectre.Console;

return await RunAsync(args);

static async Task<int> RunAsync(string[] args)
{
    MdViewCommand command;
    try { command = MdViewCommand.Parse(args); }
    catch (ArgumentException ex)
    {
        Console.Error.WriteLine(ex.Message);
        PrintHelp();
        return 2;
    }

    if (command.Kind == MdViewSourceKind.Help)
    {
        PrintHelp();
        return 0;
    }

    if (command.Kind == MdViewSourceKind.Interactive)
    {
        try
        {
            var chosen = await new MdViewInteractiveSourcePicker(AnsiConsole.Console)
                .PickAsync(Environment.CurrentDirectory);
            if (chosen is null) return 0;
            command = chosen;
        }
        catch (Exception ex) when (ex is InvalidOperationException or IOException or
                                   UnauthorizedAccessException or ArgumentException)
        {
            Console.Error.WriteLine("Interactive selection failed: " + ex.Message);
            return 1;
        }
    }

    try
    {
        string content;
        if (command.Kind == MdViewSourceKind.Local)
        {
            var path = Path.GetFullPath(command.Value ?? "");
            if (!MdViewInteractiveSourcePicker.IsMarkdownPath(path))
            {
                Console.Error.WriteLine("Local source must end in .md or .markdown.");
                return 2;
            }
            var result = await TextFilePreviewReader.ReadCompleteAsync(path);
            if (result.Kind == TextPreviewKind.Empty)
            {
                Console.WriteLine("(Empty Markdown document)");
                return 0;
            }
            if (result.Kind != TextPreviewKind.Text)
            {
                Console.Error.WriteLine("Invalid encoding, binary input or unsafe terminal control characters.");
                return 1;
            }
            content = result.Content;
        }
        else
        {
            using var handler = new HttpClientHandler { AllowAutoRedirect = false, AutomaticDecompression = System.Net.DecompressionMethods.None };
            using var http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
            var cache = new VerifiedDocumentCache(CacheDirectory.GetDefaultPath());
            HttpDocumentResult result;
            if (command.Kind == MdViewSourceKind.GitHub)
                result = await new GitHubReadmeSource(http, cache).OpenAsync(command.Value!, command.Repository!);
            else if (command.Kind == MdViewSourceKind.Url)
                result = await new HttpMarkdownDocumentSource(http, cache).OpenAsync(new Uri(command.Value!, UriKind.Absolute));
            else throw new ArgumentException("Unrecognized source type.");

            if (result.Document is null)
            {
                Console.Error.WriteLine(result.Warning ?? "Document is unavailable.");
                return 1;
            }
            Console.Error.WriteLine("Source: " + result.State +
                (result.State == HttpDocumentState.Stale ? " (offline or stale copy)" : ""));
            if (result.Warning is not null) Console.Error.WriteLine(result.Warning);
            // The complete cached text is loaded by this first CLI version; it is not a pager.
            content = await File.ReadAllTextAsync(result.Document.ContentPath);
        }

        if (content.Length > 1024 * 1024)
            Console.Error.WriteLine("Large Markdown document: current renderer loads the entire text in memory.");
        MarkdownSpectreRenderer.Render(AnsiConsole.Console, content, MdViewHighlighting.CreateTheme());
        return 0;
    }
    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or
                               ArgumentException or InvalidOperationException or
                               HttpRequestException or UriFormatException or System.Text.DecoderFallbackException)
    {
        Console.Error.WriteLine("Unable to display the requested Markdown document: " + ex.Message);
        return 1;
    }
}

static void PrintHelp()
{
    Console.WriteLine("mdview [PATH | URL | github:OWNER/REPOSITORY]");
    Console.WriteLine("mdview local PATH");
    Console.WriteLine("mdview url HTTPS_URL");
    Console.WriteLine("mdview git OWNER REPOSITORY");
    Console.WriteLine("mdview git OWNER/REPOSITORY");
    Console.WriteLine("mdview --help");
    Console.WriteLine("No arguments: interactive Spectre source menu with a local Markdown FileBrowser.");
}
