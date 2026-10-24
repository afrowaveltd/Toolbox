using Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;
using Spectre.Console;
using Spectre.Console.Json;

namespace Afrowave.Toolbox.Blackboard.Showcase;

/// <summary>Exercises FileBrowser JSON filtering and Spectre's highlighted JSON renderable.</summary>
public sealed class FileBrowserJsonDemo : IBlackboardShowcaseDemo
{
    /// <inheritdoc/>
    public string Id => "filebrowser-json";

    /// <inheritdoc/>
    public string Title => "FileBrowser — filter JSON and show colored JSON preview";

    /// <inheritdoc/>
    public async Task RunAsync(
        IAnsiConsole console, string startingDirectory, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentException.ThrowIfNullOrWhiteSpace(startingDirectory);

        console.Write(new Text(
            "Choose a .json file. Directories remain navigable; JSON will be syntax-highlighted" +
            " if it is valid and at most 64 KiB." + Environment.NewLine));

        var picker = new SpectreFileBrowser(new FileBrowserLocalSource(), console);
        var result = await picker.PickAsync(
            new FileBrowserLocation("local", startingDirectory),
            FileBrowserPickMode.File,
            new SpectreFileBrowserOptions
            {
                // This demonstrates FileBrowser's public, caller-defined filtering API.
                // JSON is a selectable extension, not a constraint baked into the picker.
                FileFilter = new FileBrowserFileFilter(
                    nameMatcher: new FileBrowserNameMatcher("*.json"))
            },
            cancellationToken);

        if (!result.IsSelected)
        {
            console.Write(new Text("JSON selection cancelled." + Environment.NewLine));
            return;
        }

        var path = result.Location!.Value;
        console.Write(new Text("Selected JSON: " + path + Environment.NewLine));

        try
        {
            var preview = await JsonFilePreviewReader.ReadAsync(path, cancellationToken);
            if (preview.Kind != JsonPreviewKind.Valid)
            {
                console.Write(new Text("JSON preview unavailable: " + preview.Details +
                    Environment.NewLine));
                return;
            }

            console.Write(new Text(
                "JSON preview (" + preview.TotalBytes + " bytes):" + Environment.NewLine));
            // JsonText is supplied by Spectre.Console.Json, installed in the SHOWCASE only.
            // It syntax-highlights the already validated complete JSON document.
            console.Write(new JsonText(preview.Json));
            console.Write(new Text(Environment.NewLine));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            console.Write(new Text("Cannot read the selected JSON file: " +
                ex.Message + Environment.NewLine));
        }
    }
}
