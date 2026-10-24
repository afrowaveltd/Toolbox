namespace Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;

/// <summary>
/// Filters file entries by semantic type and/or extension while keeping directories
/// available for navigation by default.
/// </summary>
/// <remarks>
/// Filtering inspects only the entry name; it never opens or downloads a file.
/// A null set means no constraint, while an explicitly empty set matches no files.
/// Within each set values are OR-ed; supplying both sets combines them with AND.
/// </remarks>
public sealed class FileBrowserFileFilter
{
    private readonly HashSet<FileBrowserFileKind>? _kinds;
    private readonly string[]? _extensions;
    private readonly FileBrowserFileIcons _classifier;
    private readonly FileBrowserNameMatcher? _nameMatcher;

    /// <summary>Creates a lightweight, reusable file type filter.</summary>
    /// <param name="includedKinds">Optional allowed semantic file kinds.</param>
    /// <param name="includedExtensions">Optional allowed extensions, e.g. ".json" or ".tar.gz".</param>
    /// <param name="includeDirectories">Keep directories visible for navigation (default true).</param>
    /// <param name="classifier">Optional customized file-kind classifier.</param>
    /// <param name="nameMatcher">Optional wildcard-mask list or bounded regular expression.</param>
    public FileBrowserFileFilter(
        IEnumerable<FileBrowserFileKind>? includedKinds = null,
        IEnumerable<string>? includedExtensions = null,
        bool includeDirectories = true,
        FileBrowserFileIcons? classifier = null,
        FileBrowserNameMatcher? nameMatcher = null)
    {
        if (includedKinds is not null)
        {
            _kinds = new HashSet<FileBrowserFileKind>();
            foreach (var kind in includedKinds)
            {
                if (!Enum.IsDefined(kind))
                    throw new ArgumentOutOfRangeException(nameof(includedKinds),
                        "One of the requested file kinds is not defined.");
                _kinds.Add(kind);
            }
        }

        if (includedExtensions is not null)
        {
            var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var extension in includedExtensions)
            {
                if (string.IsNullOrWhiteSpace(extension) ||
                    !extension.StartsWith(".", StringComparison.Ordinal) ||
                    extension.Length < 2 ||
                    extension.Contains('/') || extension.Contains('\\'))
                    throw new ArgumentException("Extensions must start with '.' and contain no path separators.",
                        nameof(includedExtensions));

                extensions.Add(extension);
            }

            _extensions = extensions.ToArray();
        }

        IncludeDirectories = includeDirectories;
        _classifier = classifier ?? new FileBrowserFileIcons();
        _nameMatcher = nameMatcher;
    }

    /// <summary>Whether directories remain visible even when files are filtered.</summary>
    public bool IncludeDirectories { get; }

    /// <summary>Indicates whether a directory or file should be displayed.</summary>
    /// <param name="entryName">The file/directory name, not the full source path.</param>
    /// <param name="isDirectory">Whether the entry is a directory.</param>
    public bool Allows(string? entryName, bool isDirectory)
    {
        if (isDirectory)
            return IncludeDirectories;

        if (string.IsNullOrEmpty(entryName))
            return false;

        if (_nameMatcher is not null && !_nameMatcher.Matches(entryName))
            return false;

        if (_kinds is not null && !_kinds.Contains(_classifier.Classify(entryName, false)))
            return false;

        if (_extensions is not null &&
            !_extensions.Any(ext => entryName.Length > ext.Length &&
                                    entryName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
            return false;

        return true;
    }
}
