namespace Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;

/// <summary>Path conventions used when identifying well-known local directories.</summary>
public enum FileBrowserPlatform
{
    /// <summary>Microsoft Windows drive paths.</summary>
    Windows,
    /// <summary>Linux POSIX paths.</summary>
    Linux,
    /// <summary>macOS POSIX paths.</summary>
    MacOS,
    /// <summary>An unidentified platform; only explicitly supplied home paths are recognized.</summary>
    Other
}

/// <summary>Well-known local directory roles, independent of their displayed names.</summary>
public enum FileBrowserKnownFolderKind
{
    /// <summary>No safely identifiable special directory.</summary>
    None,
    /// <summary>The directory containing user profiles, such as /home or C:\Users.</summary>
    UserProfiles,
    /// <summary>The current user's home directory.</summary>
    UserHome,
    /// <summary>The system configuration directory.</summary>
    SystemConfiguration,
    /// <summary>The system log directory.</summary>
    Logs,
    /// <summary>The system temporary-files directory.</summary>
    Temporary,
    /// <summary>A platform-standard applications directory.</summary>
    Applications
}

/// <summary>
/// Identifies a small number of well-known local folders using full paths,
/// never just a directory's display name.
/// </summary>
/// <remarks>
/// This classifier does not access the filesystem, resolve symlinks or infer paths for
/// non-local sources. FTP/SFTP providers must not apply local platform conventions.
/// </remarks>
public sealed class FileBrowserKnownFolders
{
    private readonly Dictionary<string, FileBrowserKnownFolderKind> _locations;
    private readonly FileBrowserPlatform _platform;

    /// <summary>Creates an environment-independent classifier, suitable for testing.</summary>
    /// <param name="platform">The platform whose path conventions apply.</param>
    /// <param name="userHomeDirectory">Optional absolute local home directory for the current user.</param>
    public FileBrowserKnownFolders(FileBrowserPlatform platform, string? userHomeDirectory = null)
    {
        if (!Enum.IsDefined(platform))
            throw new ArgumentOutOfRangeException(nameof(platform));

        _platform = platform;
        _locations = new Dictionary<string, FileBrowserKnownFolderKind>(
            platform == FileBrowserPlatform.Windows
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal);

        switch (platform)
        {
            case FileBrowserPlatform.Linux:
                Add("/home", FileBrowserKnownFolderKind.UserProfiles);
                Add("/etc", FileBrowserKnownFolderKind.SystemConfiguration);
                Add("/var/log", FileBrowserKnownFolderKind.Logs);
                Add("/tmp", FileBrowserKnownFolderKind.Temporary);
                break;
            case FileBrowserPlatform.MacOS:
                Add("/Users", FileBrowserKnownFolderKind.UserProfiles);
                Add("/Applications", FileBrowserKnownFolderKind.Applications);
                break;
        }

        var normalizedHome = Normalize(userHomeDirectory);
        if (normalizedHome is null)
            return;

        _locations[normalizedHome] = FileBrowserKnownFolderKind.UserHome;

        if (platform == FileBrowserPlatform.Windows)
        {
            // Locate C:\Users (or D:\Users) from the actual profile path, rather than
            // assuming a particular system drive or labelling every directory "Users".
            var end = normalizedHome.LastIndexOf('\\');
            if (end > 2)
            {
                var parent = normalizedHome[..end];
                if (parent.Length == 8 &&
                    parent.AsSpan(3).Equals("Users".AsSpan(), StringComparison.OrdinalIgnoreCase))
                    _locations[parent] = FileBrowserKnownFolderKind.UserProfiles;
            }
        }
    }

    /// <summary>Creates a classifier for the running system and its current user profile.</summary>
    public static FileBrowserKnownFolders ForCurrentPlatform()
    {
        var platform = OperatingSystem.IsWindows() ? FileBrowserPlatform.Windows
            : OperatingSystem.IsLinux() ? FileBrowserPlatform.Linux
            : OperatingSystem.IsMacOS() ? FileBrowserPlatform.MacOS
            : FileBrowserPlatform.Other;

        return new FileBrowserKnownFolders(
            platform, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
    }

    /// <summary>Recognizes the meaning of an absolute local directory path.</summary>
    /// <param name="fullPath">Absolute local path, rather than the entry name.</param>
    /// <param name="isDirectory">Whether the entry is known to be a directory.</param>
    /// <returns>A known role, or <see cref="FileBrowserKnownFolderKind.None"/>.</returns>
    public FileBrowserKnownFolderKind Classify(string? fullPath, bool isDirectory)
    {
        if (!isDirectory)
            return FileBrowserKnownFolderKind.None;

        var normalized = Normalize(fullPath);
        return normalized is not null && _locations.TryGetValue(normalized, out var kind)
            ? kind : FileBrowserKnownFolderKind.None;
    }

    /// <summary>Returns the English fallback description of a special folder.</summary>
    public static string GetLabel(FileBrowserKnownFolderKind kind) => kind switch
    {
        FileBrowserKnownFolderKind.None => "Folder",
        FileBrowserKnownFolderKind.UserProfiles => "User profiles",
        FileBrowserKnownFolderKind.UserHome => "Home",
        FileBrowserKnownFolderKind.SystemConfiguration => "System configuration",
        FileBrowserKnownFolderKind.Logs => "System logs",
        FileBrowserKnownFolderKind.Temporary => "Temporary files",
        FileBrowserKnownFolderKind.Applications => "Applications",
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    /// <summary>Returns a special-folder glyph, respecting terminal icon settings.</summary>
    public static string GetIcon(
        FileBrowserKnownFolderKind kind, FileBrowserIconMode mode,
        FileBrowserTerminalCapabilities terminal)
    {
        if (!Enum.IsDefined(kind))
            throw new ArgumentOutOfRangeException(nameof(kind));
        if (!Enum.IsDefined(mode))
            throw new ArgumentOutOfRangeException(nameof(mode));

        if (kind == FileBrowserKnownFolderKind.None)
            return FileBrowserFileIcons.GetIcon(FileBrowserFileKind.Directory, mode, terminal);

        if (mode == FileBrowserIconMode.Off ||
            (mode == FileBrowserIconMode.Auto && (!terminal.SupportsUnicode || !terminal.IsInteractive)))
            return string.Empty;

        if (mode == FileBrowserIconMode.On || terminal.SupportsEmoji)
            return kind switch
            {
                FileBrowserKnownFolderKind.UserProfiles => "👥",
                FileBrowserKnownFolderKind.UserHome => "🏠",
                FileBrowserKnownFolderKind.SystemConfiguration => "⚙️",
                FileBrowserKnownFolderKind.Logs => "📜",
                FileBrowserKnownFolderKind.Temporary => "🧹",
                FileBrowserKnownFolderKind.Applications => "🗂️",
                _ => throw new ArgumentOutOfRangeException(nameof(kind))
            };

        return kind switch
        {
            FileBrowserKnownFolderKind.UserHome => "⌂",
            FileBrowserKnownFolderKind.UserProfiles => "▸",
            FileBrowserKnownFolderKind.SystemConfiguration => "⚙",
            FileBrowserKnownFolderKind.Logs => "≡",
            FileBrowserKnownFolderKind.Temporary => "·",
            FileBrowserKnownFolderKind.Applications => "▣",
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
    }

    private void Add(string path, FileBrowserKnownFolderKind kind)
    {
        var normalized = Normalize(path);
        if (normalized is not null)
            _locations[normalized] = kind;
    }

    private string? Normalize(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        // Parse by the selected platform rather than Path.GetFullPath, which would
        // interpret simulated Windows paths as Unix paths when tests run on Linux.
        if (_platform == FileBrowserPlatform.Windows)
        {
            var value = path.Replace('/', '\\');
            if (value.Length < 3 || !char.IsLetter(value[0]) ||
                value[1] != ':' || value[2] != '\\')
                return null;

            if (value.Length > 3)
                value = value.TrimEnd('\\');

            var segments = value[3..].Split('\\');
            return segments.Any(s => s is "." or ".." or "") ? null : value;
        }

        if (!path.StartsWith("/", StringComparison.Ordinal) ||
            path.StartsWith("//", StringComparison.Ordinal) || path.Contains('\\'))
            return null;

        var unix = path.Length > 1 ? path.TrimEnd('/') : path;
        var parts = unix.Split('/');
        return parts.Skip(1).Any(s => s is "." or ".." or "") ? null : unix;
    }
}
