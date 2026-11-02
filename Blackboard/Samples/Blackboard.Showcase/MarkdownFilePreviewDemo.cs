using Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Afrowave.Toolbox.ExchangeHouse.Markdown;
using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Showcase;

/// <summary>
/// A read-only, complete local Markdown file viewer. Uses existing FileBrowser,
/// strict streaming text decoding and Markdown rendering without new dependencies.
/// </summary>
public sealed class MarkdownFilePreviewDemo : IBlackboardShowcaseDemo
{
    /// <summary>Informational size warning threshold (64 KiB), never a read limit.</summary>
    public const int LargeFileNoticeBytes = 64 * 1024;

    /// <inheritdoc/>
    public string Id => "markdown-file";

    /// <inheritdoc/>
    public string Title => "Markdown — open local README or .md file";

    /// <inheritdoc/>
    public async Task RunAsync(
        IAnsiConsole console, string startingDirectory, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentException.ThrowIfNullOrWhiteSpace(startingDirectory);

        console.Write(new Text(
            "Select a local .md or .markdown file (read-only; complete document)." +
            Environment.NewLine));

        var picker = new SpectreFileBrowser(new FileBrowserLocalSource(), console);
        var picked = await picker.PickAsync(
            new FileBrowserLocation("local", startingDirectory),
            FileBrowserPickMode.File,
            new SpectreFileBrowserOptions
            {
                FileFilter = new FileBrowserFileFilter(
                    nameMatcher: new FileBrowserNameMatcher("*.md;*.markdown"))
            },
            cancellationToken);

        if (!picked.IsSelected)
        {
            console.Write(new Text("Markdown selection cancelled." + Environment.NewLine));
            return;
        }

        var path = picked.Location!.Value;
        // The picker permits interactive filter changes; independently verify the
        // extension rather than trusting the filter to enforce file type.
        if (!IsMarkdownPath(path))
        {
            console.Write(new Text("Selected file is not Markdown (.md or .markdown)." +
                Environment.NewLine));
            return;
        }

        try
        {
            var preview = await TextFilePreviewReader.ReadCompleteAsync(
                path, cancellationToken);
            if (preview.Kind == TextPreviewKind.Empty)
            {
                console.Write(new Text("The Markdown file is empty." + Environment.NewLine));
                return;
            }

            if (preview.Kind != TextPreviewKind.Text)
            {
                console.Write(new Text(
                    "Markdown preview unavailable: binary data, unsupported encoding, " +
                    "or unsafe terminal control characters." + Environment.NewLine));
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();
            console.Write(new Text("Markdown preview: " + Path.GetFileName(path) +
                " (" + preview.EncodingName + ", " + preview.TotalBytes + " bytes)" +
                Environment.NewLine));
            if (preview.TotalBytes > LargeFileNoticeBytes)
                console.Write(new Text(
                    "[Large Markdown document: the full file is being shown (" +
                    preview.TotalBytes + " bytes); terminal output may be lengthy.]" +
                    Environment.NewLine));

            // The renderer uses literal Spectre text, never interpolated Markup.
            // The sample and file-preview modes share one optional Classic theme.
            var document = MarkdownParser.Parse(preview.Content);
            MarkdownSpectreRenderer.Render(console, document, MarkdownThemeDemo.ClassicPreviewTheme);
            console.Write(new Text(Environment.NewLine));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            console.Write(new Text(
                "Unable to read selected Markdown file (I/O or permission error)." +
                Environment.NewLine));
        }
    }

    /// <summary>Recognize the two file extensions supported by this demo.</summary>
    public static bool IsMarkdownPath(string path) =>
        Path.GetExtension(path) is { } extension &&
        (extension.Equals(".md", StringComparison.OrdinalIgnoreCase) ||
         extension.Equals(".markdown", StringComparison.OrdinalIgnoreCase));
}
