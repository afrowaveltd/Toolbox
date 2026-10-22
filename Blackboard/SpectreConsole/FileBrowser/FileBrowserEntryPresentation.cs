using System.Text;

namespace Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;

/// <summary>Creates plain-text file picker labels with optional icons and a small file indent.</summary>
public static class FileBrowserEntryPresentation
{
    /// <summary>
    /// Formats one entry, placing files exactly one ASCII space farther right than folders.
    /// </summary>
    /// <remarks>
    /// This is display text, not a filesystem path or Spectre markup. Render it as plain
    /// Spectre.Console Text (not Markup); callers must never select by matching this label.
    /// Emoji may occupy multiple terminal cells, so pixel-perfect alignment is the
    /// responsibility of the actual terminal renderer.
    /// </remarks>
    /// <param name="entryName">The original entry name.</param>
    /// <param name="isDirectory">Whether the entry is a directory.</param>
    /// <param name="icons">The configured file type resolver.</param>
    /// <param name="mode">The decorative-icon policy.</param>
    /// <param name="terminal">Current terminal capability hints.</param>
    /// <param name="knownFolder">Optional role for a recognized local directory.</param>
    /// <returns>Plain display text, never a modified selection path.</returns>
    public static string Format(
        string entryName, bool isDirectory, FileBrowserFileIcons icons,
        FileBrowserIconMode mode, FileBrowserTerminalCapabilities terminal,
        FileBrowserKnownFolderKind knownFolder = FileBrowserKnownFolderKind.None)
    {
        ArgumentNullException.ThrowIfNull(entryName);
        ArgumentNullException.ThrowIfNull(icons);
        if (entryName.Length == 0)
            throw new ArgumentException("Entry name cannot be empty.", nameof(entryName));
        if (!Enum.IsDefined(knownFolder) ||
            (!isDirectory && knownFolder != FileBrowserKnownFolderKind.None))
            throw new ArgumentOutOfRangeException(nameof(knownFolder));

        var glyph = isDirectory
            ? FileBrowserKnownFolders.GetIcon(knownFolder, mode, terminal)
            : FileBrowserFileIcons.GetIcon(icons.Classify(entryName, false), mode, terminal);

        // Keep the directory at the left edge and place file rows one space farther right.
        var indent = isDirectory ? "" : " ";
        var iconPrefix = glyph.Length == 0 ? "" : glyph + " ";
        // The suffix is a visual navigation hint, not part of the real filename.
        var suffix = isDirectory && !entryName.EndsWith('/') && !entryName.EndsWith('\\') ? "/" : "";

        return indent + iconPrefix + EscapeControls(entryName) + suffix;
    }

    private static string EscapeControls(string entryName)
    {
        var result = new StringBuilder(entryName.Length);
        foreach (var ch in entryName)
        {
            // Do not allow untrusted filenames to inject CR, ESC or other terminal controls.
            if (char.IsControl(ch))
                result.Append(@"\u").Append(((int)ch).ToString("X4"));
            else
                result.Append(ch);
        }

        return result.ToString();
    }
}
