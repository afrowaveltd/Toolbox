namespace Afrowave.Toolbox.Blackboard.SpectreConsole.FileBrowser;

/// <summary>
/// Conservative supplemental Unix terminal detection for hosts where the Spectre.Console
/// Unicode capability flag is unavailable or falsely negative.
/// </summary>
public static class FileBrowserTerminalDetection
{
    /// <summary>
    /// Decides whether Auto mode may display conservative Unicode symbols.
    /// Never infers emoji support from UTF-8.
    /// </summary>
    /// <remarks>
    /// The caller must supply the effective Spectre profile and real terminal conditions.
    /// A UTF-8 locale is only a fallback for Unix interactive terminals; it must never
    /// override redirected output, a dumb terminal or a noninteractive Spectre console.
    /// </remarks>
    public static bool SupportsUnicode(
        bool spectreSupportsUnicode,
        bool isInteractive,
        bool isOutputRedirected,
        bool isUnix,
        string? term,
        string? locale)
    {
        if (!isInteractive || isOutputRedirected ||
            string.Equals(term, "dumb", StringComparison.OrdinalIgnoreCase))
            return false;

        if (spectreSupportsUnicode)
            return true;

        if (!isUnix || string.IsNullOrWhiteSpace(locale))
            return false;

        return locale.Contains("UTF-8", StringComparison.OrdinalIgnoreCase) ||
               locale.Contains("UTF8", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Returns the first defined POSIX locale in precedence order.</summary>
    public static string? GetEffectiveLocale()
    {
        foreach (var key in new[] { "LC_ALL", "LC_CTYPE", "LANG" })
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return null;
    }
}
