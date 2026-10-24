using Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;
using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Showcase;

/// <summary>Shows FileBrowser selection and a safe, bounded text file preview.</summary>
public sealed class FileBrowserPreviewDemo : IBlackboardShowcaseDemo
{
    /// <inheritdoc/>
    public string Id => "filebrowser-preview";

    /// <inheritdoc/>
    public string Title => "FileBrowser — choose file and preview readable text";

    /// <inheritdoc/>
    public async Task RunAsync(
        IAnsiConsole console, string startingDirectory, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentException.ThrowIfNullOrWhiteSpace(startingDirectory);

        console.Write(new Text(
            "Choose a local file. Text files will be previewed up to 16 KiB; binary files will not be printed." +
            Environment.NewLine));

        var browser = new SpectreFileBrowser(new FileBrowserLocalSource(), console);
        var selected = await browser.PickAsync(
            new FileBrowserLocation("local", startingDirectory),
            FileBrowserPickMode.File,
            new SpectreFileBrowserOptions(),
            cancellationToken);

        if (!selected.IsSelected)
        {
            console.Write(new Text("No file selected." + Environment.NewLine));
            return;
        }

        var path = selected.Location!.Value;
        console.Write(new Text("Selected: " + path + Environment.NewLine));
        try
        {
            var preview = await TextFilePreviewReader.ReadAsync(
                path, cancellationToken: cancellationToken);

            switch (preview.Kind)
            {
                case TextPreviewKind.Empty:
                    console.Write(new Text("The file is empty." + Environment.NewLine));
                    return;
                case TextPreviewKind.BinaryOrUnsupportedEncoding:
                    console.Write(new Text(
                        "Binary, unsupported text encoding, or unsafe terminal controls detected." +
                        " File contents were not displayed." + Environment.NewLine));
                    return;
                case TextPreviewKind.Text:
                    console.Write(new Text(
                        "Text preview (" + preview.EncodingName +
                        ", file size " + preview.TotalBytes + " bytes):" + Environment.NewLine));
                    // Text renderable is not Spectre markup. Normalize CR to prevent
                    // raw carriage returns from manipulating terminal cursor position.
                    var safeText = preview.Content.Replace("\r\n", "\n").Replace('\r', '\n');
                    console.Write(new Text(safeText + Environment.NewLine));
                    if (preview.Truncated)
                        console.Write(new Text("[Preview truncated at byte limit]" +
                            Environment.NewLine));
                    return;
                default:
                    throw new InvalidOperationException("Unexpected text preview status.");
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            console.Write(new Text("Cannot read selected file: " + ex.Message +
                Environment.NewLine));
        }
    }
}
