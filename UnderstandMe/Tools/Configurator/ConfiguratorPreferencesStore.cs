using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Afrowave.Toolbox.UnderstandMe.Configurator;

/// <summary>Per-user Configurator preferences, independent of edited UnderstandMe settings.</summary>
public sealed record ConfiguratorPreferences
{
    /// <summary>Version of the application's own preferences schema.</summary>
    public int SchemaVersion { get; init; } = 1;

    /// <summary>UI language; currently "en" or "cs".</summary>
    public string Language { get; init; } = "en";
}

/// <summary>Validated preferences and the revision of the original file (null when absent).</summary>
public sealed record ConfiguratorPreferencesDocument(ConfiguratorPreferences Settings, string? Revision);

/// <summary>Reads and atomically writes local user preferences without touching host settings.</summary>
public sealed class ConfiguratorPreferencesStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    /// <summary>Construct a preferences store at an explicit full or relative path.</summary>
    public ConfiguratorPreferencesStore(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        FilePath = System.IO.Path.GetFullPath(path);
    }

    /// <summary>Absolute location of the Configurator's own per-user JSON document.</summary>
    public string FilePath { get; }

    /// <summary>Resolve an OS-appropriate per-user location; never depends on the executable directory.</summary>
    public static string DefaultFilePath()
    {
        string root;
        if (OperatingSystem.IsWindows())
            root = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        else
        {
            var xdg = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
            root = !string.IsNullOrWhiteSpace(xdg) && System.IO.Path.IsPathFullyQualified(xdg)
                ? xdg : System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
        }

        if (string.IsNullOrWhiteSpace(root))
            throw new InvalidOperationException("A user configuration directory could not be resolved.");

        return System.IO.Path.Combine(root, "afrowave", "understandme-configurator", "settings.json");
    }

    /// <summary>Read preferences; missing file produces an unsaved English default, not a write.</summary>
    /// <exception cref="InvalidDataException">Unsupported version, language or empty document.</exception>
    public ConfiguratorPreferencesDocument Load()
    {
        if (!File.Exists(FilePath))
            return new(new ConfiguratorPreferences(), null);
        var bytes = File.ReadAllBytes(FilePath);
        var preferences = JsonSerializer.Deserialize<ConfiguratorPreferences>(bytes, JsonOptions);
        Validate(preferences);
        return new(preferences!, Convert.ToHexString(SHA256.HashData(bytes)));
    }

    /// <summary>Persist validated preferences with a revision check; null means create-only.</summary>
    /// <remarks>No preference file or directory is created during Load.</remarks>
    public string Save(ConfiguratorPreferences preferences, string? expectedRevision)
    {
        Validate(preferences);
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(preferences, JsonOptions) + "\n");
        var folder = System.IO.Path.GetDirectoryName(FilePath)!;
        Directory.CreateDirectory(folder);
        var tempPath = System.IO.Path.Combine(folder, "." + System.IO.Path.GetFileName(FilePath) +
            "." + Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            using (var stream = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes);
                stream.Flush(flushToDisk: true);
            }

            if (expectedRevision is null)
            {
                if (File.Exists(FilePath))
                    throw new IOException("User preferences were created externally; reload before saving.");
                File.Move(tempPath, FilePath);
            }
            else
            {
                if (!File.Exists(FilePath) ||
                    !string.Equals(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(FilePath))),
                        expectedRevision, StringComparison.Ordinal))
                    throw new IOException("User preferences changed externally; reload before saving.");
                File.Move(tempPath, FilePath, overwrite: true);
            }

            return Convert.ToHexString(SHA256.HashData(bytes));
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    private static void Validate(ConfiguratorPreferences? settings)
    {
        if (settings is null || settings.SchemaVersion != 1 ||
            settings.Language is not ("en" or "cs"))
            throw new InvalidDataException("Unsupported Configurator preferences or UI language.");
    }
}
