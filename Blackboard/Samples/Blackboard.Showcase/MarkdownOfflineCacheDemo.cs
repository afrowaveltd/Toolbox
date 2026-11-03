using Afrowave.Toolbox.ExchangeHouse.Markdown;
using Spectre.Console;

namespace Afrowave.Toolbox.Blackboard.Showcase;

/// <summary>
/// Experimental offline source-cache manager using bounded catalogue pages.
/// Cache inspection and page viewing do not require network connectivity.
/// </summary>
public sealed class MarkdownOfflineCacheDemo : IBlackboardShowcaseDemo
{
    private const int PageSize = 8;
    private const string Browse = "Browse saved Markdown documents";
    private const string Prune = "Prune old document versions (older than 24 hours)";
    private const string Back = "Back to Showcase";
    private const string Open = "Read saved copy offline";
    private const string Export = "Save a permanent copy elsewhere";
    private const string Remove = "Remove this saved document";
    private const string BackToList = "Back to saved documents";
    private readonly MarkdownRemoteDocumentCache _cache;

    /// <summary>Use the per-user cache or inject an isolated directory for tests.</summary>
    public MarkdownOfflineCacheDemo(MarkdownRemoteDocumentCache? cache = null)
    {
        _cache = cache ?? new MarkdownRemoteDocumentCache();
    }

    /// <inheritdoc/>
    public string Id => "markdown-offline-cache";

    /// <inheritdoc/>
    public string Title => "Markdown — offline document cache (experimental)";

    /// <inheritdoc/>
    public async Task RunAsync(
        IAnsiConsole console, string startingDirectory,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentException.ThrowIfNullOrWhiteSpace(startingDirectory);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var usage = await _cache.GetUsageAsync(cancellationToken);
                console.Write(new Text(
                    "Saved Markdown documents (offline)\n" +
                    $"Disk cache: {usage.UsedBytes:N0} / {usage.MaxTotalBytes:N0} bytes " +
                    $"in {usage.FileCount:N0} files.\n"));

                var action = await new SelectionPrompt<string>()
                    .Title("Offline cache management (Esc returns)")
                    .PageSize(3)
                    .AddChoices(Browse, Prune, Back)
                    .AddCancelResult(Back)
                    .ShowAsync(console, cancellationToken);

                if (action == Back)
                    return;
                if (action == Browse)
                    await BrowseAsync(console, startingDirectory, cancellationToken);
                else
                    await PruneAsync(console, cancellationToken);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or
                                       InvalidDataException)
            {
                console.Write(new Text(
                    "Cannot access the Markdown cache: " + ex.Message +
                    Environment.NewLine));
                return;
            }
        }
    }

    private async Task BrowseAsync(
        IAnsiConsole console, string startingDirectory,
        CancellationToken cancellationToken)
    {
        var skip = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var page = await _cache.ListPageAsync(skip, PageSize, cancellationToken);
            if (page.Entries.Count == 0)
            {
                if (skip > 0)
                {
                    skip = Math.Max(0, skip - PageSize);
                    continue;
                }
                console.Write(new Text("No saved Markdown documents found.\n"));
                return;
            }

            // A menu page retains only a small fixed number of choices.
            // No global source catalog or document body is kept in memory.
            var choices = new List<string>(PageSize + 3);
            for (var i = 0; i < page.Entries.Count; i++)
            {
                var entry = page.Entries[i];
                var address = entry.Source is null
                    ? "(legacy source unknown)"
                    : entry.Source.GetLeftPart(UriPartial.Path);
                choices.Add($"{i + 1}. {SafeLabel(address, 76)} " +
                    $"[{entry.ByteCount:N0} B; {entry.CacheKey[..8]}]");
            }
            const string next = "Next saved documents";
            const string previous = "Previous saved documents";
            if (page.HasMore)
                choices.Add(next);
            if (skip > 0)
                choices.Add(previous);
            choices.Add(BackToList);

            var selection = await new SelectionPrompt<string>()
                .Title($"Offline documents {skip + 1}–{skip + page.Entries.Count} " +
                    "(Esc returns)")
                .PageSize(Math.Min(12, choices.Count))
                .UseConverter(Markup.Escape)
                .AddChoices(choices)
                .AddCancelResult(BackToList)
                .ShowAsync(console, cancellationToken);
            if (selection == BackToList)
                return;
            if (selection == next)
            {
                skip = checked(skip + PageSize);
                continue;
            }
            if (selection == previous)
            {
                skip = Math.Max(0, skip - PageSize);
                continue;
            }

            var index = choices.IndexOf(selection);
            if (index < 0 || index >= page.Entries.Count)
                continue;
            var changed = await DocumentActionsAsync(console,
                startingDirectory, page.Entries[index], cancellationToken);
            if (changed)
                skip = 0;
        }
    }

    private async Task<bool> DocumentActionsAsync(
        IAnsiConsole console, string startingDirectory,
        MarkdownRemoteCacheEntry entry, CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var choices = entry.Source is null
                ? new[] { Open, Export, BackToList }
                : new[] { Open, Export, Remove, BackToList };
            var selected = await new SelectionPrompt<string>()
                .Title("Saved document actions (Esc returns)")
                .PageSize(4)
                .AddChoices(choices)
                .AddCancelResult(BackToList)
                .ShowAsync(console, cancellationToken);

            if (selected == BackToList)
                return false;
            if (selected == Open)
            {
                var label = entry.Source is null
                    ? "Saved Markdown"
                    : Path.GetFileName(entry.Source.AbsolutePath);
                await MarkdownPagedSourceDemo.ShowFileAsync(
                    console, entry.LocalPath, cancellationToken, label);
            }
            else if (selected == Export)
            {
                // Do not overwrite a user file silently; save is always
                // an explicit, independent copy outside the cache.
                var name = entry.Source is null
                    ? "Saved-Markdown.md"
                    : Path.GetFileName(entry.Source.AbsolutePath);
                if (string.IsNullOrWhiteSpace(name))
                    name = "Saved-Markdown.md";
                name = SafeLabel(name, 60);
                var path = await new TextPrompt<string>(
                    "Save a permanent copy to which path?")
                    .DefaultValue(Path.Combine(startingDirectory, name))
                    .ShowAsync(console, cancellationToken);
                var destination = Path.GetFullPath(
                    Path.IsPathRooted(path) ? path :
                    Path.Combine(startingDirectory, path));
                try
                {
                    await MarkdownRemoteDocumentCache.SaveCopyAsync(
                        new MarkdownCachedRemoteDocument(
                            entry.Source ?? new Uri("https://example.invalid/legacy"),
                            entry.LocalPath, entry.ByteCount,
                            true, true, null, entry.LastModified),
                        destination, cancellationToken: cancellationToken);
                    console.Write(new Text(
                        "Permanent copy saved: " + SafeLabel(destination, 160) + "\n"));
                }
                catch (Exception ex) when (ex is IOException or
                    UnauthorizedAccessException or ArgumentException)
                {
                    console.Write(new Text(
                        "Could not save copy (existing files are not overwritten): " +
                        ex.Message + "\n"));
                }
            }
            else if (selected == Remove && entry.Source is not null)
            {
                const string confirm = "Yes, remove saved document and its old versions";
                const string keep = "No, keep the offline document";
                var answer = await new SelectionPrompt<string>()
                    .Title("Remove this offline document? This cannot be undone.")
                    .PageSize(2)
                    .AddChoices(keep, confirm)
                    .AddCancelResult(keep)
                    .ShowAsync(console, cancellationToken);
                if (answer == confirm)
                {
                    var removed = await _cache.RemoveAsync(
                        entry.Source, cancellationToken);
                    console.Write(new Text(
                        $"Removed {removed} cache files for this source.\n"));
                    return true;
                }
            }
        }
    }

    private async Task PruneAsync(
        IAnsiConsole console, CancellationToken cancellationToken)
    {
        const string keep = "Keep all cached versions";
        const string clean = "Prune older redundant versions (24-hour grace period)";
        var choice = await new SelectionPrompt<string>()
            .Title("This will never delete the verified current offline copy.")
            .PageSize(2)
            .AddChoices(keep, clean)
            .AddCancelResult(keep)
            .ShowAsync(console, cancellationToken);
        if (choice == clean)
        {
            var removed = await _cache.PruneObsoleteVersionsAsync(
                cancellationToken: cancellationToken);
            console.Write(new Text(
                $"Pruned {removed} obsolete saved versions.\n"));
        }
    }

    private static string SafeLabel(string input, int limit) =>
        new string(input.Take(limit)
            .Select(c => char.IsControl(c) ? '\uFFFD' : c).ToArray());
}
