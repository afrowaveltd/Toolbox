namespace Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;

/// <summary>
/// Read-only access to local filesystem directories for a generic file browser.
/// </summary>
/// <remarks>
/// Directory symlinks and Windows reparse points are displayed but are not traversed
/// by recursive search or tree expansion, preventing common filesystem cycles.
/// This is not a security sandbox; the host must enforce its own access policy.
/// </remarks>
public sealed class FileBrowserLocalSource : IFileBrowserSource
{
    /// <inheritdoc />
    public string Id => "local";

    /// <inheritdoc />
    public bool IsCaseSensitive => !OperatingSystem.IsWindows();

    /// <inheritdoc />
    public Task<IReadOnlyList<FileBrowserSourceEntry>> ListAsync(
        FileBrowserLocation directory, CancellationToken cancellationToken = default)
    {
        var path = ValidateLocation(directory);
        cancellationToken.ThrowIfCancellationRequested();

        var entries = new List<FileBrowserSourceEntry>();
        foreach (var item in Directory.EnumerateFileSystemEntries(path))
        {
            cancellationToken.ThrowIfCancellationRequested();

            FileAttributes attributes;
            try
            {
                attributes = File.GetAttributes(item);
            }
            catch (FileNotFoundException)
            {
                // A directory may change during enumeration. Ignore vanished children.
                continue;
            }
            catch (DirectoryNotFoundException)
            {
                continue;
            }

            var isDirectory = (attributes & FileAttributes.Directory) != 0;
            var isLink = (attributes & FileAttributes.ReparsePoint) != 0;
            var entryPath = Path.GetFullPath(item);
            entries.Add(new FileBrowserSourceEntry(
                Path.GetFileName(entryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)),
                new FileBrowserLocation(Id, entryPath),
                isDirectory,
                canTraverse: !isLink));
        }

        return Task.FromResult<IReadOnlyList<FileBrowserSourceEntry>>(
            entries.OrderByDescending(e => e.IsDirectory)
                .ThenBy(e => e.Name, IsCaseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase)
                .ToArray());
    }

    /// <inheritdoc />
    public Task<FileBrowserLocation?> GetParentAsync(
        FileBrowserLocation directory, CancellationToken cancellationToken = default)
    {
        var path = ValidateLocation(directory);
        cancellationToken.ThrowIfCancellationRequested();

        var root = Path.GetPathRoot(path);
        if (root is null || string.Equals(
                path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                IsCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<FileBrowserLocation?>(null);

        var parent = Directory.GetParent(path);
        return Task.FromResult(parent is null ? null : new FileBrowserLocation(Id, parent.FullName));
    }

    private string ValidateLocation(FileBrowserLocation directory)
    {
        ArgumentNullException.ThrowIfNull(directory);
        if (!string.Equals(directory.SourceId, Id, StringComparison.Ordinal))
            throw new ArgumentException("The directory belongs to another storage source.", nameof(directory));
        if (!Path.IsPathFullyQualified(directory.Value))
            throw new ArgumentException("Local browser paths must be fully qualified.", nameof(directory));

        return Path.GetFullPath(directory.Value);
    }
}
