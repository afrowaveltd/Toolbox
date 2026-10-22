namespace Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;

/// <summary>Semantic file categories that may receive different display icons.</summary>
public enum FileBrowserFileKind
{
    /// <summary>A file whose type is not recognized.</summary>
    Unknown,
    /// <summary>A directory or folder.</summary>
    Directory,
    /// <summary>A plain-text or Markdown document.</summary>
    Text,
    /// <summary>Source code or a script.</summary>
    Code,
    /// <summary>Structured JSON data.</summary>
    Json,
    /// <summary>An application configuration file.</summary>
    Configuration,
    /// <summary>A word-processing document.</summary>
    Document,
    /// <summary>A PDF document.</summary>
    Pdf,
    /// <summary>A spreadsheet or tabular data file.</summary>
    Spreadsheet,
    /// <summary>A slide presentation.</summary>
    Presentation,
    /// <summary>An image.</summary>
    Image,
    /// <summary>Music or other audio.</summary>
    Audio,
    /// <summary>A video recording.</summary>
    Video,
    /// <summary>A compressed archive.</summary>
    Archive,
    /// <summary>An executable, application installer or installable package.</summary>
    Executable,
    /// <summary>A database file.</summary>
    Database,
    /// <summary>A font file.</summary>
    Font
}

/// <summary>Controls decorative icon rendering independently of actual filenames.</summary>
public enum FileBrowserIconMode
{
    /// <summary>Use terminal capability hints and conservative fallbacks.</summary>
    Auto,
    /// <summary>Force rich emoji icons, even when the terminal may not support them.</summary>
    On,
    /// <summary>Hide icons in every terminal.</summary>
    Off
}

/// <summary>Terminal capability hints, supplied by the hosting console adapter.</summary>
/// <param name="SupportsUnicode">Whether the terminal appears able to display Unicode.</param>
/// <param name="IsInteractive">Whether the output is an interactive terminal.</param>
/// <param name="SupportsEmoji">Whether emoji support has been explicitly confirmed by the host.</param>
public readonly record struct FileBrowserTerminalCapabilities(
    bool SupportsUnicode, bool IsInteractive, bool SupportsEmoji = false);

/// <summary>
/// Classifies file names into familiar categories and supplies optional display glyphs.
/// </summary>
/// <remarks>
/// Classification uses only the entry name and source-provided directory flag; it does
/// not inspect content, open files, alter console encoding or modify selected paths.
/// Optional overrides apply to a resolver instance, not to the entire process.
/// </remarks>
public sealed class FileBrowserFileIcons
{
    private static readonly IReadOnlyDictionary<string, FileBrowserFileKind> DefaultExtensions =
        new Dictionary<string, FileBrowserFileKind>(StringComparer.OrdinalIgnoreCase)
        {
            [".txt"] = FileBrowserFileKind.Text,
            [".md"] = FileBrowserFileKind.Text,
            [".markdown"] = FileBrowserFileKind.Text,
            [".log"] = FileBrowserFileKind.Text,
            [".cs"] = FileBrowserFileKind.Code,
            [".rs"] = FileBrowserFileKind.Code,
            [".py"] = FileBrowserFileKind.Code,
            [".js"] = FileBrowserFileKind.Code,
            [".ts"] = FileBrowserFileKind.Code,
            [".jsx"] = FileBrowserFileKind.Code,
            [".tsx"] = FileBrowserFileKind.Code,
            [".html"] = FileBrowserFileKind.Code,
            [".css"] = FileBrowserFileKind.Code,
            [".sh"] = FileBrowserFileKind.Code,
            [".ps1"] = FileBrowserFileKind.Code,
            [".sql"] = FileBrowserFileKind.Code,
            [".json"] = FileBrowserFileKind.Json,
            [".jsonc"] = FileBrowserFileKind.Configuration,
            [".yaml"] = FileBrowserFileKind.Configuration,
            [".yml"] = FileBrowserFileKind.Configuration,
            [".toml"] = FileBrowserFileKind.Configuration,
            [".ini"] = FileBrowserFileKind.Configuration,
            [".config"] = FileBrowserFileKind.Configuration,
            [".xml"] = FileBrowserFileKind.Configuration,
            [".doc"] = FileBrowserFileKind.Document,
            [".docx"] = FileBrowserFileKind.Document,
            [".odt"] = FileBrowserFileKind.Document,
            [".pdf"] = FileBrowserFileKind.Pdf,
            [".csv"] = FileBrowserFileKind.Spreadsheet,
            [".tsv"] = FileBrowserFileKind.Spreadsheet,
            [".xls"] = FileBrowserFileKind.Spreadsheet,
            [".xlsx"] = FileBrowserFileKind.Spreadsheet,
            [".ods"] = FileBrowserFileKind.Spreadsheet,
            [".ppt"] = FileBrowserFileKind.Presentation,
            [".pptx"] = FileBrowserFileKind.Presentation,
            [".odp"] = FileBrowserFileKind.Presentation,
            [".png"] = FileBrowserFileKind.Image,
            [".jpg"] = FileBrowserFileKind.Image,
            [".jpeg"] = FileBrowserFileKind.Image,
            [".gif"] = FileBrowserFileKind.Image,
            [".webp"] = FileBrowserFileKind.Image,
            [".svg"] = FileBrowserFileKind.Image,
            [".bmp"] = FileBrowserFileKind.Image,
            [".mp3"] = FileBrowserFileKind.Audio,
            [".wav"] = FileBrowserFileKind.Audio,
            [".flac"] = FileBrowserFileKind.Audio,
            [".ogg"] = FileBrowserFileKind.Audio,
            [".m4a"] = FileBrowserFileKind.Audio,
            [".aac"] = FileBrowserFileKind.Audio,
            [".mp4"] = FileBrowserFileKind.Video,
            [".mkv"] = FileBrowserFileKind.Video,
            [".webm"] = FileBrowserFileKind.Video,
            [".avi"] = FileBrowserFileKind.Video,
            [".mov"] = FileBrowserFileKind.Video,
            [".zip"] = FileBrowserFileKind.Archive,
            [".7z"] = FileBrowserFileKind.Archive,
            [".rar"] = FileBrowserFileKind.Archive,
            [".tar"] = FileBrowserFileKind.Archive,
            [".tar.gz"] = FileBrowserFileKind.Archive,
            [".tar.xz"] = FileBrowserFileKind.Archive,
            [".tar.zst"] = FileBrowserFileKind.Archive,
            [".tgz"] = FileBrowserFileKind.Archive,
            [".gz"] = FileBrowserFileKind.Archive,
            [".xz"] = FileBrowserFileKind.Archive,
            [".zst"] = FileBrowserFileKind.Archive,
            [".exe"] = FileBrowserFileKind.Executable,
            [".msi"] = FileBrowserFileKind.Executable,
            [".deb"] = FileBrowserFileKind.Executable,
            [".rpm"] = FileBrowserFileKind.Executable,
            [".apk"] = FileBrowserFileKind.Executable,
            [".sqlite"] = FileBrowserFileKind.Database,
            [".sqlite3"] = FileBrowserFileKind.Database,
            [".db"] = FileBrowserFileKind.Database,
            [".ttf"] = FileBrowserFileKind.Font,
            [".otf"] = FileBrowserFileKind.Font,
            [".woff"] = FileBrowserFileKind.Font,
            [".woff2"] = FileBrowserFileKind.Font
        };

    private static readonly IReadOnlyDictionary<string, FileBrowserFileKind> DefaultNames =
        new Dictionary<string, FileBrowserFileKind>(StringComparer.OrdinalIgnoreCase)
        {
            ["README"] = FileBrowserFileKind.Text,
            ["LICENSE"] = FileBrowserFileKind.Text,
            ["COPYING"] = FileBrowserFileKind.Text,
            ["Dockerfile"] = FileBrowserFileKind.Code,
            ["Makefile"] = FileBrowserFileKind.Code,
            [".gitignore"] = FileBrowserFileKind.Configuration,
            [".gitattributes"] = FileBrowserFileKind.Configuration,
            [".env"] = FileBrowserFileKind.Configuration
        };

    private readonly KeyValuePair<string, FileBrowserFileKind>[] _extensions;
    private readonly IReadOnlyDictionary<string, FileBrowserFileKind> _names;

    /// <summary>Creates the default classifications, with optional per-instance overrides.</summary>
    /// <param name="extensionOverrides">Extensions such as ".abc" mapped to a file kind.</param>
    /// <param name="fileNameOverrides">Exact entry names, for instance "my.data".</param>
    /// <exception cref="ArgumentException">An override has an invalid key or category.</exception>
    public FileBrowserFileIcons(
        IReadOnlyDictionary<string, FileBrowserFileKind>? extensionOverrides = null,
        IReadOnlyDictionary<string, FileBrowserFileKind>? fileNameOverrides = null)
    {
        var extensions = new Dictionary<string, FileBrowserFileKind>(DefaultExtensions, StringComparer.OrdinalIgnoreCase);
        AddOverrides(extensionOverrides, extensions, true);
        // Check long suffixes first: custom compound extensions override their shorter suffixes.
        _extensions = extensions.OrderByDescending(x => x.Key.Length).ToArray();

        var names = new Dictionary<string, FileBrowserFileKind>(DefaultNames, StringComparer.OrdinalIgnoreCase);
        AddOverrides(fileNameOverrides, names, false);
        _names = names;
    }

    /// <summary>Determines a file category from its name without reading file contents.</summary>
    /// <param name="fileName">A single entry name, not a full path.</param>
    /// <param name="isDirectory">The authoritative directory flag supplied by the source.</param>
    /// <returns>The semantic category.</returns>
    public FileBrowserFileKind Classify(string? fileName, bool isDirectory)
    {
        if (isDirectory)
            return FileBrowserFileKind.Directory;
        if (string.IsNullOrWhiteSpace(fileName))
            return FileBrowserFileKind.Unknown;
        if (_names.TryGetValue(fileName, out var nameKind))
            return nameKind;

        foreach (var item in _extensions)
        {
            // A bare hidden name like ".json" is not automatically a JSON extension.
            if (fileName.Length > item.Key.Length &&
                fileName.EndsWith(item.Key, StringComparison.OrdinalIgnoreCase))
                return item.Value;
        }

        return FileBrowserFileKind.Unknown;
    }

    /// <summary>Selects the display glyph for a file type and terminal's effective icon policy.</summary>
    /// <param name="kind">Semantic file type.</param>
    /// <param name="mode">Automatic, force-enabled or disabled icons.</param>
    /// <param name="terminal">Capabilities already detected by the presentation adapter.</param>
    /// <returns>An optional glyph, or an empty string for icon-free output.</returns>
    public static string GetIcon(
        FileBrowserFileKind kind, FileBrowserIconMode mode, FileBrowserTerminalCapabilities terminal)
    {
        if (!Enum.IsDefined(kind))
            throw new ArgumentOutOfRangeException(nameof(kind));
        if (!Enum.IsDefined(mode))
            throw new ArgumentOutOfRangeException(nameof(mode));

        if (mode == FileBrowserIconMode.Off ||
            (mode == FileBrowserIconMode.Auto && (!terminal.SupportsUnicode || !terminal.IsInteractive)))
            return string.Empty;

        // UTF-8/Unicode does not imply that an emoji font exists. Rich icons require
        // explicit user choice or a confirmed emoji-capable terminal.
        return mode == FileBrowserIconMode.On || terminal.SupportsEmoji
            ? Emoji(kind)
            : SimpleSymbol(kind);
    }

    /// <summary>Returns the English name of a file category for icon-free navigation and accessibility.</summary>
    /// <param name="kind">Semantic file type.</param>
    /// <returns>A label suitable for optional caller localization.</returns>
    public static string GetLabel(FileBrowserFileKind kind) => kind switch
    {
        FileBrowserFileKind.Unknown => "File",
        FileBrowserFileKind.Directory => "Folder",
        FileBrowserFileKind.Text => "Text",
        FileBrowserFileKind.Code => "Code",
        FileBrowserFileKind.Json => "JSON",
        FileBrowserFileKind.Configuration => "Configuration",
        FileBrowserFileKind.Document => "Document",
        FileBrowserFileKind.Pdf => "PDF",
        FileBrowserFileKind.Spreadsheet => "Spreadsheet",
        FileBrowserFileKind.Presentation => "Presentation",
        FileBrowserFileKind.Image => "Image",
        FileBrowserFileKind.Audio => "Audio",
        FileBrowserFileKind.Video => "Video",
        FileBrowserFileKind.Archive => "Archive",
        FileBrowserFileKind.Executable => "Application",
        FileBrowserFileKind.Database => "Database",
        FileBrowserFileKind.Font => "Font",
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    private static string Emoji(FileBrowserFileKind kind) => kind switch
    {
        FileBrowserFileKind.Directory => "📁",
        FileBrowserFileKind.Text => "📃",
        FileBrowserFileKind.Code => "💻",
        FileBrowserFileKind.Json => "🧩",
        FileBrowserFileKind.Configuration => "⚙️",
        FileBrowserFileKind.Document => "📄",
        FileBrowserFileKind.Pdf => "📕",
        FileBrowserFileKind.Spreadsheet => "📊",
        FileBrowserFileKind.Presentation => "📽️",
        FileBrowserFileKind.Image => "🖼️",
        FileBrowserFileKind.Audio => "🎵",
        FileBrowserFileKind.Video => "🎬",
        FileBrowserFileKind.Archive => "🗜️",
        FileBrowserFileKind.Executable => "⚙️",
        FileBrowserFileKind.Database => "🗃️",
        FileBrowserFileKind.Font => "🔤",
        _ => "📄"
    };

    private static string SimpleSymbol(FileBrowserFileKind kind) => kind switch
    {
        FileBrowserFileKind.Directory => "▸",
        FileBrowserFileKind.Audio => "♪",
        FileBrowserFileKind.Video => "▶",
        FileBrowserFileKind.Image => "◇",
        FileBrowserFileKind.Archive => "▣",
        FileBrowserFileKind.Code => "⌘",
        _ => "·"
    };

    private static void AddOverrides(
        IReadOnlyDictionary<string, FileBrowserFileKind>? source,
        IDictionary<string, FileBrowserFileKind> target,
        bool extension)
    {
        if (source is null)
            return;

        foreach (var (key, kind) in source)
        {
            if (string.IsNullOrWhiteSpace(key) || !Enum.IsDefined(kind) ||
                (extension && (!key.StartsWith(".", StringComparison.Ordinal) ||
                               key.Length < 2 || key.Contains('/') || key.Contains('\\'))) ||
                (!extension && (key.Contains('/') || key.Contains('\\'))))
                throw new ArgumentException("File type override has an invalid key or category.",
                    nameof(source));

            target[key] = kind;
        }
    }
}
