namespace Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;

/// <summary>A storage-provider-qualified location, not necessarily a local filesystem path.</summary>
/// <remarks>
/// The value is opaque outside its source. In particular, consumers must never treat
/// an FTP/SFTP location as an ordinary path that can be opened with System.IO.
/// </remarks>
public sealed record FileBrowserLocation
{
    /// <summary>Creates a location owned by a specific storage source.</summary>
    public FileBrowserLocation(string sourceId, string value)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
            throw new ArgumentException("A source ID is required.", nameof(sourceId));
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A location value is required.", nameof(value));

        SourceId = sourceId;
        Value = value;
    }

    /// <summary>The registered provider's stable source identifier.</summary>
    public string SourceId { get; }

    /// <summary>An opaque path or identifier interpreted only by its provider.</summary>
    public string Value { get; }
}

/// <summary>One file or directory returned by a file-browser storage source.</summary>
public sealed record FileBrowserSourceEntry
{
    /// <summary>Creates an entry returned by a storage provider.</summary>
    public FileBrowserSourceEntry(
        string name, FileBrowserLocation location, bool isDirectory, bool canTraverse = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("An entry name is required.", nameof(name));

        ArgumentNullException.ThrowIfNull(location);
        Name = name;
        Location = location;
        IsDirectory = isDirectory;
        CanTraverse = isDirectory && canTraverse;
    }

    /// <summary>The exact entry name; never derive a filesystem path from this label.</summary>
    public string Name { get; }

    /// <summary>The opaque, source-qualified identity of the entry.</summary>
    public FileBrowserLocation Location { get; }

    /// <summary>Whether the entry is a directory.</summary>
    public bool IsDirectory { get; }

    /// <summary>
    /// Whether recursive navigation is permitted. Providers should return false
    /// for symlinks/reparse points unless safe cycle detection is explicitly provided.
    /// </summary>
    public bool CanTraverse { get; }
}

/// <summary>
/// Read-only source contract for directory-tree and recursive-search operations.
/// </summary>
/// <remarks>
/// Only the source owns native path parsing, case rules, connections and listing.
/// No writes, downloads, uploads or implicit authentication are part of this contract.
/// </remarks>
public interface IFileBrowserSource
{
    /// <summary>A stable identifier qualifying every location returned by this source.</summary>
    string Id { get; }

    /// <summary>Whether filename wildcard matching is case-sensitive for this source.</summary>
    bool IsCaseSensitive { get; }

    /// <summary>Lists a single directory without recursively traversing its descendants.</summary>
    Task<IReadOnlyList<FileBrowserSourceEntry>> ListAsync(
        FileBrowserLocation directory, CancellationToken cancellationToken = default);

    /// <summary>Returns the parent directory, or null for a root location.</summary>
    Task<FileBrowserLocation?> GetParentAsync(
        FileBrowserLocation directory, CancellationToken cancellationToken = default);
}
