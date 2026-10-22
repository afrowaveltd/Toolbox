namespace Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;

/// <summary>A single expandable directory node of a lazily loaded browser tree.</summary>
/// <param name="Name">Human-readable directory name.</param>
/// <param name="Location">Provider-qualified node location.</param>
/// <param name="CanExpand">Whether the provider permits entering this directory.</param>
public sealed record FileBrowserTreeNode(
    string Name, FileBrowserLocation Location, bool CanExpand);

/// <summary>
/// Supplies directory children only when a caller expands a tree node.
/// </summary>
/// <remarks>
/// It never recursively scans the entire tree, opens files or mutates the
/// process working directory. Rendering and expanded/collapsed UI state
/// belong to a separate Spectre.Console adapter.
/// </remarks>
public sealed class FileBrowserDirectoryTree
{
    private readonly IFileBrowserSource _source;

    /// <summary>Creates a lazy tree bound to a read-only storage source.</summary>
    public FileBrowserDirectoryTree(IFileBrowserSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        _source = source;
    }

    /// <summary>Lists immediate directory children of one tree node.</summary>
    public async Task<IReadOnlyList<FileBrowserTreeNode>> GetChildrenAsync(
        FileBrowserLocation parent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parent);
        if (!string.Equals(parent.SourceId, _source.Id, StringComparison.Ordinal))
            throw new ArgumentException("Tree location belongs to another source.", nameof(parent));

        cancellationToken.ThrowIfCancellationRequested();
        var entries = await _source.ListAsync(parent, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Storage source returned a null listing.");

        var children = new List<FileBrowserTreeNode>();
        foreach (var entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!string.Equals(entry.Location.SourceId, _source.Id, StringComparison.Ordinal))
                throw new InvalidOperationException("Storage source returned a foreign location.");

            if (entry.IsDirectory)
                children.Add(new FileBrowserTreeNode(entry.Name, entry.Location, entry.CanTraverse));
        }

        return children.OrderBy(x => x.Name, _source.IsCaseSensitive
            ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase).ToArray();
    }
}
