using System.Text;

namespace Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;

/// <summary>
/// A selectable search result with its complete path relative to the search origin.
/// </summary>
/// <param name="MatchIndex">Index into the unmodified search result's matches.</param>
/// <param name="RelativePath">Plain-text display path (not a provider path or Spectre markup).</param>
/// <param name="Kind">Semantic type suitable for the existing icon policy.</param>
/// <param name="Location">Original source-qualified location for the matched entry.</param>
public sealed record FileBrowserSearchDisplayRow(
    int MatchIndex, string RelativePath, FileBrowserFileKind Kind, FileBrowserLocation Location);

/// <summary>One branch or matched leaf in a search-results-only directory tree.</summary>
/// <param name="Name">Plain-text entry name.</param>
/// <param name="IsDirectory">Whether this row is a containing directory.</param>
/// <param name="MatchIndex">Match index for a selectable result; null for structural folders.</param>
/// <param name="Children">Matching descendants only, not unrelated filesystem entries.</param>
public sealed record FileBrowserSearchTreeNode(
    string Name, bool IsDirectory, int? MatchIndex,
    IReadOnlyList<FileBrowserSearchTreeNode> Children);

/// <summary>
/// Presents every returned search match as a relative path or a pruned tree.
/// Results are never silently collapsed to one match or moved into a folder.
/// </summary>
public static class FileBrowserSearchResultsPresentation
{
    /// <summary>
    /// Produces a sorted list such as 'Folder/Subfolder/config.json', preserving
    /// each result's original index for the eventual keyboard selection UI.
    /// </summary>
    public static IReadOnlyList<FileBrowserSearchDisplayRow> CreateRows(
        FileBrowserSearchResult result, FileBrowserFileIcons? classifier = null,
        bool isCaseSensitive = true)
    {
        ArgumentNullException.ThrowIfNull(result);
        classifier ??= new FileBrowserFileIcons();
        var rows = new List<FileBrowserSearchDisplayRow>(result.Matches.Count);

        for (var index = 0; index < result.Matches.Count; index++)
        {
            var match = result.Matches[index];
            var segments = match.RelativeDirectories
                .Select(EscapeForDisplay)
                .Append(EscapeForDisplay(match.Entry.Name));
            var path = string.Join("/", segments);
            rows.Add(new FileBrowserSearchDisplayRow(index, path,
                classifier.Classify(match.Entry.Name, match.Entry.IsDirectory),
                match.Entry.Location));
        }

        var comparer = isCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
        return rows.OrderBy(row => row.RelativePath, comparer)
            .ThenBy(row => row.MatchIndex).ToArray();
    }

    /// <summary>
    /// Creates a compact tree containing only directories leading to matches.
    /// Each matched file or folder retains its original selectable match index.
    /// </summary>
    /// <remarks>
    /// This builds only from already collected results and makes no additional
    /// filesystem or remote calls. Directory branches have no selection action.
    /// </remarks>
    public static IReadOnlyList<FileBrowserSearchTreeNode> CreateTree(
        FileBrowserSearchResult result, bool isCaseSensitive = true)
    {
        ArgumentNullException.ThrowIfNull(result);

        var comparer = isCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
        var root = new MutableBranch("", comparer);

        for (var index = 0; index < result.Matches.Count; index++)
        {
            var match = result.Matches[index];
            var current = root;
            foreach (var segment in match.RelativeDirectories)
            {
                var display = EscapeForDisplay(segment);
                if (!current.Directories.TryGetValue(display, out var next))
                {
                    next = new MutableBranch(display, comparer);
                    current.Directories.Add(display, next);
                }

                current = next;
            }

            current.Matches.Add(new FileBrowserSearchTreeNode(
                EscapeForDisplay(match.Entry.Name), match.Entry.IsDirectory, index,
                Array.Empty<FileBrowserSearchTreeNode>()));
        }

        return Freeze(root, comparer);
    }

    private static IReadOnlyList<FileBrowserSearchTreeNode> Freeze(
        MutableBranch branch, StringComparer comparer)
    {
        var contents = branch.Directories.Values
            .Select(child => new FileBrowserSearchTreeNode(
                child.Name, true, null, Freeze(child, comparer)))
            .Concat(branch.Matches);

        return contents.OrderByDescending(x => x.IsDirectory)
            .ThenBy(x => x.Name, comparer)
            .ThenBy(x => x.MatchIndex ?? -1).ToArray();
    }

    private static string EscapeForDisplay(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            // Display separators must remain unambiguous even for providers whose
            // opaque filenames contain slash/backslash or terminal control chars.
            if (ch is '/' or '\\' || char.IsControl(ch))
                builder.Append(@"\u").Append(((int)ch).ToString("X4"));
            else
                builder.Append(ch);
        }

        return builder.ToString();
    }

    private sealed class MutableBranch
    {
        public MutableBranch(string name, StringComparer comparer)
        {
            Name = name;
            Directories = new Dictionary<string, MutableBranch>(comparer);
        }

        public string Name { get; }
        public Dictionary<string, MutableBranch> Directories { get; }
        public List<FileBrowserSearchTreeNode> Matches { get; } = [];
    }
}
