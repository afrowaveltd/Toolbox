using Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;
using Afrowave.Toolbox.ExchangeHouse.Markdown;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Spectre.Console;
using System.Text;

namespace Afrowave.Toolbox.Blackboard.Showcase;

/// <summary>
/// Experimental disk-indexed Markdown source pager. Unlike the complete
/// formatted viewer, this intentionally displays raw source (page-local
/// Markdown cannot be safely interpreted without semantic state checkpoints).
/// </summary>
public sealed class MarkdownPagedSourceDemo : IBlackboardShowcaseDemo
{
    /// <inheritdoc/>
    public string Id => "markdown-paged-source";

    /// <inheritdoc/>
    public string Title => "Markdown — disk-indexed source pages (experimental)";

    /// <inheritdoc/>
    public async Task RunAsync(
        IAnsiConsole console, string startingDirectory, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentException.ThrowIfNullOrWhiteSpace(startingDirectory);
        console.Write(new Text("Choose a UTF-8 Markdown file for indexed source paging." +
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
            console.Write(new Text("Markdown paging cancelled." + Environment.NewLine));
            return;
        }

        var path = picked.Location!.Value;
        if (!MarkdownFilePreviewDemo.IsMarkdownPath(path))
        {
            console.Write(new Text("Selected file is not Markdown (.md or .markdown)." +
                Environment.NewLine));
            return;
        }

        try
        {
            console.Write(new Text("Indexing and validating UTF-8 Markdown on disk..." +
                Environment.NewLine));
            await using var index = await MarkdownDiskPageIndex.BuildAsync(
                path, cancellationToken: cancellationToken);
            if (index.PageCount == 0)
            {
                console.Write(new Text("The Markdown file is empty." + Environment.NewLine));
                return;
            }

            // Filenames are untrusted terminal input, even when the Markdown
            // bytes have been validated separately.
            var displayName = new string(Path.GetFileName(path)
                .Select(c => char.IsControl(c) ? '\uFFFD' : c).ToArray());
            await using var fences = await MarkdownFenceCheckpointIndex.BuildAsync(
                index, cancellationToken);
            long pageNumber = 0;
            var showConservativePreview = false;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var contextual = await fences.ReadPageAsync(pageNumber, cancellationToken);
                var page = contextual.Source;
                console.Write(new Text(Environment.NewLine +
                    "Markdown source: " + displayName +
                    " — page " + (pageNumber + 1) + "/" + index.PageCount +
                    ", lines " + page.FirstLine + "–" +
                    (page.FirstLine + page.LineCount - 1) + Environment.NewLine));
                if (showConservativePreview)
                {
                    // A code fence started on a previous page remains code on
                    // this page. Other multiline Markdown semantics are still
                    // conservative; this is NOT the complete document renderer.
                    foreach (var line in contextual.Lines)
                        MarkdownSpectreRenderer.RenderStreamingLine(
                            console, line, MarkdownThemeDemo.ClassicPreviewTheme);
                }
                else
                {
                    // Strictly validated literal source, not Spectre markup.
                    console.Write(new Text(page.Content));
                    if (!page.Content.EndsWith('\n') && !page.Content.EndsWith('\r'))
                        console.Write(new Text(Environment.NewLine));
                }

                var actions = new List<string>(3);
                if (pageNumber + 1 < index.PageCount)
                    actions.Add("Next page");
                if (pageNumber > 0)
                    actions.Add("Previous page");
                actions.Add(showConservativePreview ?
                    "Show literal source" : "Show conservative Markdown preview");
                actions.Add("Close pager");

                var choice = await new SelectionPrompt<string>()
                    .Title("Navigate source pages (Esc closes)")
                    .PageSize(3)
                    .AddChoices(actions)
                    .AddCancelResult("Close pager")
                    .ShowAsync(console, cancellationToken);
                if (choice == "Close pager")
                    return;
                if (choice is "Show literal source" or "Show conservative Markdown preview")
                {
                    showConservativePreview = !showConservativePreview;
                    continue;
                }
                pageNumber += choice == "Next page" ? 1 : -1;
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or
                                   DecoderFallbackException or InvalidDataException)
        {
            console.Write(new Text(
                "Cannot index this Markdown source: unsupported/unsafe UTF-8, " +
                "oversized physical line or file I/O error." + Environment.NewLine));
        }
    }
}
