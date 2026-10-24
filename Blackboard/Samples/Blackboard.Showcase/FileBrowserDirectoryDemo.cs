using Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;
using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Showcase;

/// <summary>Demonstrates choosing a folder without previewing or modifying its files.</summary>
public sealed class FileBrowserDirectoryDemo : IBlackboardShowcaseDemo
{
    /// <inheritdoc/>
    public string Id => "filebrowser-directory";

    /// <inheritdoc/>
    public string Title => "FileBrowser — choose a directory";

    /// <inheritdoc/>
    public async Task RunAsync(
        IAnsiConsole console, string startingDirectory, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentException.ThrowIfNullOrWhiteSpace(startingDirectory);

        var browser = new SpectreFileBrowser(new FileBrowserLocalSource(), console);
        var selected = await browser.PickAsync(
            new FileBrowserLocation("local", startingDirectory),
            FileBrowserPickMode.Directory,
            cancellationToken: cancellationToken);

        console.Write(new Text(selected.IsSelected
            ? "Selected directory: " + selected.Location!.Value + Environment.NewLine
            : "No directory selected." + Environment.NewLine));
    }
}
