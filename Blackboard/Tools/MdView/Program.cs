using Afrowave.Toolbox.Blackboard.Showcase;
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
        Console.WriteLine("Markdown viewer: 1 Local  2 URL  3 GitHub  Q Quit");
        var selection = Console.ReadLine()?.Trim();
        if (selection is null || selection.Equals("q", StringComparison.OrdinalIgnoreCase)) return 0;
        switch (selection)
        {
            case "1":
                Console.Write("Local Markdown path: ");
                command = new(MdViewSourceKind.Local, Console.ReadLine());
                break;
            case "2":
                Console.Write("HTTP(S) Markdown URL: ");
                command = new(MdViewSourceKind.Url, Console.ReadLine());
                break;
            case "3":
                Console.Write("GitHub owner/repository: ");
                var text = Console.ReadLine() ?? "";
                try { command = MdViewCommand.Parse("git", text); }
                catch (ArgumentException ex) { Console.Error.WriteLine(ex.Message); return 2; }
                break;
            default:
                Console.Error.WriteLine("Unknown menu choice.");
                return 2;
        }
    }

    try
    {
        string content;
        if (command.Kind == MdViewSourceKind.Local)
        {
            var path = Path.GetFullPath(command.Value ?? "");
            var ext = Path.GetExtension(path);
            if (!ext.Equals(".md", StringComparison.OrdinalIgnoreCase) &&
                !ext.Equals(".markdown", StringComparison.OrdinalIgnoreCase))
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
        MarkdownSpectreRenderer.Render(AnsiConsole.Console, content);
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
    Console.WriteLine("No arguments: interactive Local / URL / GitHub source menu.");
}
