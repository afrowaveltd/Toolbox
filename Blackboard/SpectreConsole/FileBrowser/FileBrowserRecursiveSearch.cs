namespace Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;

/// <summary>Limits recursive file searches so large volumes and remote sources stay responsive.</summary>
public sealed record FileBrowserSearchOptions
{
    /// <summary>Maximum directory nesting below the starting directory.</summary>
    public int MaxDepth { get; init; } = 32;

    /// <summary>Maximum number of directories to enumerate.</summary>
    public int MaxDirectories { get; init; } = 10_000;

    /// <summary>Maximum number of matching entries to return.</summary>
    public int MaxResults { get; init; } = 1_000;

    /// <summary>Maximum number of entries processed from one directory listing.</summary>
    public int MaxEntriesPerDirectory { get; init; } = 20_000;

    /// <summary>Interpret the search expression as wildcard mask(s) or regex.</summary>
    public FileBrowserPatternMode PatternMode { get; init; } = FileBrowserPatternMode.Wildcard;

    /// <summary>Also match directory names, rather than files only.</summary>
    public bool IncludeDirectories { get; init; }

    /// <summary>Optional additional file-kind or extension constraint on matches.</summary>
    public FileBrowserFileFilter? Filter { get; init; }

    internal void Validate()
    {
        if (!Enum.IsDefined(PatternMode))
            throw new ArgumentOutOfRangeException(nameof(PatternMode));
        if (MaxDepth is < 0 or > 512)
            throw new ArgumentOutOfRangeException(nameof(MaxDepth));
        if (MaxDirectories is < 1 or > 1_000_000)
            throw new ArgumentOutOfRangeException(nameof(MaxDirectories));
        if (MaxResults is < 1 or > 100_000)
            throw new ArgumentOutOfRangeException(nameof(MaxResults));
        if (MaxEntriesPerDirectory is < 1 or > 1_000_000)
            throw new ArgumentOutOfRangeException(nameof(MaxEntriesPerDirectory));
    }
}

/// <summary>How completely a recursive search explored the accessible tree.</summary>
public enum FileBrowserSearchStatus
{
    /// <summary>Every reachable directory within the chosen search scope was visited.</summary>
    Completed,
    /// <summary>Some directories could not be read; results may be incomplete.</summary>
    Partial,
    /// <summary>A depth, directory, match or entries-per-directory limit was reached.</summary>
    LimitReached
}

/// <summary>A match with its parent folder, so no path parsing is needed to navigate to it.</summary>
/// <param name="Entry">Matched file or directory.</param>
/// <param name="ParentDirectory">Directory containing the entry.</param>
/// <param name="Depth">Depth of the parent directory, relative to the search origin.</param>
/// <param name="RelativeDirectories">
/// Exact sequence of traversed directory names from the search origin to the parent.
/// These are display segments, not an interpretation of provider-native paths.
/// </param>
public sealed record FileBrowserSearchMatch(
    FileBrowserSourceEntry Entry, FileBrowserLocation ParentDirectory, int Depth,
    IReadOnlyList<string> RelativeDirectories);

/// <summary>Search results and completeness diagnostics, without changing current navigation.</summary>
/// <param name="Origin">Directory searched from.</param>
/// <param name="Matches">Matches, which may have duplicate display names at different locations.</param>
/// <param name="Status">Indicates whether traversal was exhaustive.</param>
/// <param name="VisitedDirectories">Number of directories enumerated.</param>
/// <param name="SkippedDirectories">Number of unreadable directories skipped.</param>
public sealed record FileBrowserSearchResult(
    FileBrowserLocation Origin,
    IReadOnlyList<FileBrowserSearchMatch> Matches,
    FileBrowserSearchStatus Status,
    int VisitedDirectories,
    int SkippedDirectories);

/// <summary>
/// Finds files or optionally directories recursively using '*' and '?' filename patterns.
/// </summary>
/// <remarks>
/// Searches in breadth-first order, never follows provider-declared non-traversable
/// links, and never navigates, opens, modifies or downloads files. The user interface
/// must explicitly choose a match before changing the browser's current folder.
/// </remarks>
public sealed class FileBrowserRecursiveSearch
{
    private readonly IFileBrowserSource _source;

    /// <summary>Binds a search engine to a read-only storage source.</summary>
    public FileBrowserRecursiveSearch(IFileBrowserSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        _source = source;
    }

    /// <summary>
    /// Searches from one directory. Wildcards apply to a single file/folder name,
    /// not to full paths; no wildcard means an exact name. Multiple masks may
    /// be separated with semicolons; use PatternMode for an optional regex.
    /// </summary>
    /// <param name="origin">Source-qualified starting directory.</param>
    /// <param name="pattern">File name or simple glob such as "*.json" or "config?.xml".</param>
    /// <param name="options">Optional bounded search configuration.</param>
    /// <param name="cancellationToken">Stops promptly and propagates cancellation.</param>
    /// <returns>Matches and information about incomplete or limited traversal.</returns>
    public async Task<FileBrowserSearchResult> FindAsync(
        FileBrowserLocation origin, string pattern,
        FileBrowserSearchOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(origin);
        if (!string.Equals(origin.SourceId, _source.Id, StringComparison.Ordinal))
            throw new ArgumentException("Search origin belongs to another source.", nameof(origin));
        options ??= new FileBrowserSearchOptions();
        options.Validate();
        var matcher = new FileBrowserNameMatcher(
            pattern, options.PatternMode, ignoreCase: !_source.IsCaseSensitive);

        var queue = new Queue<(FileBrowserLocation Location, int Depth, string[] Directories)>();
        queue.Enqueue((origin, 0, []));

        var visited = new HashSet<string>(_source.IsCaseSensitive
            ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
        var matches = new List<FileBrowserSearchMatch>();
        var skipped = 0;
        var truncated = false;
        var enumerated = 0;

        while (queue.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (enumerated >= options.MaxDirectories)
            {
                truncated = true;
                break;
            }

            var (directory, depth, segments) = queue.Dequeue();
            if (!visited.Add(directory.Value))
                continue;

            IReadOnlyList<FileBrowserSourceEntry> entries;
            try
            {
                entries = await _source.ListAsync(directory, cancellationToken).ConfigureAwait(false)
                    ?? throw new InvalidOperationException("Storage source returned a null listing.");
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // A vanished or inaccessible subtree must not abort otherwise useful matches.
                skipped++;
                continue;
            }

            enumerated++;
            var seenEntries = 0;

            foreach (var entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (seenEntries++ >= options.MaxEntriesPerDirectory)
                {
                    truncated = true;
                    break;
                }

                if (!string.Equals(entry.Location.SourceId, _source.Id, StringComparison.Ordinal))
                    throw new InvalidOperationException("Storage source returned a foreign location.");

                var isCandidate = !entry.IsDirectory || options.IncludeDirectories;
                if (isCandidate &&
                    (options.Filter is null || options.Filter.Allows(entry.Name, entry.IsDirectory)) &&
                    matcher.Matches(entry.Name))
                {
                    matches.Add(new FileBrowserSearchMatch(
                        entry, directory, depth, Array.AsReadOnly(segments)));
                    if (matches.Count >= options.MaxResults)
                    {
                        truncated = true;
                        break;
                    }
                }

                if (entry.IsDirectory && entry.CanTraverse)
                {
                    if (depth < options.MaxDepth)
                        queue.Enqueue((entry.Location, depth + 1, [.. segments, entry.Name]));
                    else
                        truncated = true;
                }
            }

            if (matches.Count >= options.MaxResults)
                break;
        }

        var status = truncated ? FileBrowserSearchStatus.LimitReached
            : skipped > 0 ? FileBrowserSearchStatus.Partial
            : FileBrowserSearchStatus.Completed;

        return new FileBrowserSearchResult(origin, matches.AsReadOnly(), status, enumerated, skipped);
    }
}

/// <summary>
/// Tracks the current folder and selection independently of searches and tree browsing.
/// </summary>
/// <remarks>
/// Searching is read-only and never navigates by itself. A UI explicitly confirms a
/// chosen result, then calls <see cref="SelectSearchResult"/>. A failed/no-hit search
/// leaves both the current directory and highlighted entry unchanged.
/// </remarks>
public sealed class FileBrowserNavigationSession
{
    private readonly IFileBrowserSource _source;

    /// <summary>Creates a session starting in one source-qualified directory.</summary>
    public FileBrowserNavigationSession(IFileBrowserSource source, FileBrowserLocation initialDirectory)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(initialDirectory);
        if (!string.Equals(source.Id, initialDirectory.SourceId, StringComparison.Ordinal))
            throw new ArgumentException("Initial directory belongs to another source.",
                nameof(initialDirectory));

        _source = source;
        CurrentDirectory = initialDirectory;
    }

    /// <summary>The current browsing folder, unaffected by a search until a match is chosen.</summary>
    public FileBrowserLocation CurrentDirectory { get; private set; }

    /// <summary>The selected entry to highlight after navigation, if any.</summary>
    public FileBrowserSourceEntry? HighlightedEntry { get; private set; }

    /// <summary>Searches without changing current directory or highlighted selection.</summary>
    public Task<FileBrowserSearchResult> SearchAsync(
        string pattern, FileBrowserSearchOptions? options = null,
        CancellationToken cancellationToken = default)
        => new FileBrowserRecursiveSearch(_source).FindAsync(
            CurrentDirectory, pattern, options, cancellationToken);

    /// <summary>
    /// Navigates to the parent folder of one explicitly chosen result and highlights it.
    /// Returns false for no matches, invalid indices and stale/different-source results.
    /// </summary>
    public bool SelectSearchResult(FileBrowserSearchResult result, int matchIndex)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (CurrentDirectory != result.Origin ||
            matchIndex < 0 || matchIndex >= result.Matches.Count)
            return false;

        var match = result.Matches[matchIndex];
        if (!string.Equals(match.ParentDirectory.SourceId, _source.Id, StringComparison.Ordinal) ||
            !string.Equals(match.Entry.Location.SourceId, _source.Id, StringComparison.Ordinal))
            return false;

        CurrentDirectory = match.ParentDirectory;
        HighlightedEntry = match.Entry;
        return true;
    }
}
