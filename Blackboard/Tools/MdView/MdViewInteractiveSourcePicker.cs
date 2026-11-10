using Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;
using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Interactive source selection for mdview. Only selects a source; downloading and
/// rendering stay in the command execution path. Cancellation never reads a file.
/// </summary>
public sealed class MdViewInteractiveSourcePicker
{
    private const string LocalLabel = "Local Markdown file";
    private const string UrlLabel = "HTTPS Markdown URL";
    private const string GitHubLabel = "GitHub repository (browse Markdown links)";
    private const string ExitLabel = "Exit";
    private readonly IAnsiConsole _console;

    /// <summary>Create the picker for an interactive Spectre.Console terminal.</summary>
    public MdViewInteractiveSourcePicker(IAnsiConsole console) =>
        _console = console ?? throw new ArgumentNullException(nameof(console));

    /// <summary>
    /// Selects an existing local Markdown file or an HTTPS/GitHub identity.
    /// Returns null when the user cancels from the main menu.
    /// </summary>
    public async Task<MdViewCommand?> PickAsync(
        string startingDirectory, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(startingDirectory);
        if (!_console.Profile.Capabilities.Interactive)
            throw new InvalidOperationException("Interactive mdview requires a terminal.");

        var start = Path.GetFullPath(startingDirectory);
        var browser = new SpectreFileBrowser(new FileBrowserLocalSource(), _console);
        string? feedback = null;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _console.Clear();
            _console.Write(new Text("Afrowave mdview — choose a document source" + Environment.NewLine));
            if (feedback is not null)
            {
                _console.Write(new Text(feedback + Environment.NewLine));
                feedback = null;
            }

            var selection = await new SelectionPrompt<string>()
                .Title("Open Markdown from")
                .PageSize(4)
                .AddChoices(LocalLabel, UrlLabel, GitHubLabel, ExitLabel)
                .AddCancelResult(ExitLabel)
                .ShowAsync(_console, cancellationToken);

            if (selection == ExitLabel)
                return null;

            if (selection == LocalLabel)
            {
                var result = await browser.PickAsync(
                    new FileBrowserLocation("local", start), FileBrowserPickMode.File,
                    new SpectreFileBrowserOptions
                    {
                        FileFilter = new FileBrowserFileFilter(
                            nameMatcher: new FileBrowserNameMatcher("*.md;*.markdown"))
                    }, cancellationToken);

                if (!result.IsSelected)
                    continue; // Escape in FileBrowser means back to source menu, not exit.

                var path = result.Location!.Value;
                // FileBrowser permits interactive filter changes: validate again independently.
                if (!IsMarkdownPath(path))
                {
                    feedback = "Only .md and .markdown files can be displayed.";
                    continue;
                }
                return new MdViewCommand(MdViewSourceKind.Local, path);
            }

            if (selection == UrlLabel)
            {
                var input = await new TextPrompt<string>("HTTPS Markdown URL (empty = back): ")
                    .AllowEmpty().ShowAsync(_console, cancellationToken);
                if (string.IsNullOrWhiteSpace(input))
                    continue;

                input = input.Trim();
                if (!Uri.TryCreate(input, UriKind.Absolute, out var uri) ||
                    uri.Scheme != Uri.UriSchemeHttps ||
                    !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Query) ||
                    !string.IsNullOrEmpty(uri.Fragment))
                {
                    feedback = "Enter an absolute HTTPS URL without credentials, query or fragment.";
                    continue;
                }

                return new MdViewCommand(MdViewSourceKind.Url, uri.AbsoluteUri);
            }

            if (selection == GitHubLabel)
            {
                var input = await new TextPrompt<string>("GitHub owner or owner/repository (empty = back): ")
                    .AllowEmpty().ShowAsync(_console, cancellationToken);
                if (string.IsNullOrWhiteSpace(input))
                    continue;

                try
                {
                    // The interactive GitHub choice is a link browser.
                    // The explicit 'git OWNER REPO' CLI remains one-shot.
                    return MdViewCommand.Parse("git", input.Trim()) with
                    {
                        Kind = MdViewSourceKind.GitHubLinkBrowser
                    };
                }
                catch (ArgumentException)
                {
                    feedback = "Expected a GitHub owner or owner/repository.";
                }
            }
        }
    }

    /// <summary>Checks the extension independently of interactive browser filters.</summary>
    public static bool IsMarkdownPath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        var extension = Path.GetExtension(path);
        return extension.Equals(".md", StringComparison.OrdinalIgnoreCase) ||
               extension.Equals(".markdown", StringComparison.OrdinalIgnoreCase);
    }
}
