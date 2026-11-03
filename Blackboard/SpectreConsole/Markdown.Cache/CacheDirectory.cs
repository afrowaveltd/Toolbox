namespace Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown.Cache;

/// <summary>Suggests a private per-user cache location. Does not create or secure directories.</summary>
public static class CacheDirectory
{
    public static string GetDefaultPath()
    {
        if (OperatingSystem.IsWindows())
        {
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrWhiteSpace(basePath))
                throw new InvalidOperationException("Local application-data directory is unavailable.");
            return Path.Combine(basePath, "Afrowave", "MarkdownViewer", "Cache");
        }

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var baseDirectory = Environment.GetEnvironmentVariable("XDG_CACHE_HOME");
        if (string.IsNullOrWhiteSpace(baseDirectory) || !Path.IsPathFullyQualified(baseDirectory))
        {
            if (string.IsNullOrWhiteSpace(home))
                throw new InvalidOperationException("User home directory is unavailable.");
            baseDirectory = Path.Combine(home, ".cache");
        }
        return Path.Combine(baseDirectory, "afrowave", "markdown-viewer");
    }
}
