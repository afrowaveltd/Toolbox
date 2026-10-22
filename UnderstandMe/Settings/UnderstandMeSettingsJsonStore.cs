using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Afrowave.Toolbox.UnderstandMe.Settings;

/// <summary>A validated settings document and the revision of its original file.</summary>
/// <param name="Settings">Parsed settings object.</param>
/// <param name="Revision">SHA-256 revision of the bytes read from disk.</param>
public sealed record UnderstandMeSettingsDocument(UnderstandMeSettings Settings, string Revision);

/// <summary>Raised when configuration validation fails.</summary>
public sealed class UnderstandMeSettingsValidationException : Exception
{
    /// <summary>Individual errors, including editable field paths.</summary>
    public IReadOnlyList<SettingsValidationIssue> Issues { get; }

    /// <summary>Creates an exception for invalid configuration.</summary>
    /// <param name="issues">The validation issues.</param>
    public UnderstandMeSettingsValidationException(IReadOnlyList<SettingsValidationIssue> issues)
        : base("UnderstandMe settings contain invalid values.")
        => Issues = issues.ToArray();
}

/// <summary>Raised if a settings file changed after it was loaded.</summary>
public sealed class UnderstandMeSettingsConflictException : IOException
{
    /// <summary>Creates a settings revision conflict.</summary>
    public UnderstandMeSettingsConflictException() : base("Settings changed on disk; reload before saving.") { }
}

/// <summary>Reads and safely writes versioned UnderstandMe JSON configuration.</summary>
/// <remarks>
/// This is an offline persistence service; it does not start backend services.
/// Use a single coordinated writer for each file. Revision checks detect earlier
/// external edits but are not a cross-process compare-and-swap transaction.
/// </remarks>
public static class UnderstandMeSettingsJsonStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    /// <summary>Loads a settings file, enforcing JSON structure and shared semantic validation.</summary>
    /// <param name="path">Path of the JSON configuration file.</param>
    /// <returns>Validated settings and content revision.</returns>
    public static UnderstandMeSettingsDocument Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var bytes = File.ReadAllBytes(path);
        var settings = JsonSerializer.Deserialize<UnderstandMeSettings>(bytes, JsonOptions);
        Validate(settings);
        return new UnderstandMeSettingsDocument(settings!, Hash(bytes));
    }

    /// <summary>
    /// Writes a validated draft, refusing to overwrite an unexpected disk revision.
    /// Pass null for <paramref name="expectedRevision"/> only when creating a new file.
    /// </summary>
    /// <param name="path">Destination JSON file path.</param>
    /// <param name="settings">Candidate settings.</param>
    /// <param name="expectedRevision">Revision obtained from Load, or null for create-only.</param>
    /// <returns>The new persisted revision.</returns>
    /// <exception cref="UnderstandMeSettingsValidationException">The candidate is invalid.</exception>
    /// <exception cref="UnderstandMeSettingsConflictException">The file was changed or unexpectedly exists.</exception>
    public static string Save(string path, UnderstandMeSettings settings, string? expectedRevision = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        Validate(settings);

        // Serialize before touching the destination: a bad draft cannot corrupt the last good file.
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(settings, JsonOptions) + "\n");
        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(directory);
        var tempPath = Path.Combine(directory, "." + Path.GetFileName(fullPath) + "." + Guid.NewGuid().ToString("N") + ".tmp");
        try
        {
            // The temporary file resides on the destination filesystem for atomic rename semantics.
            using (var stream = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes);
                stream.Flush(flushToDisk: true);
            }

            if (expectedRevision is null)
            {
                // Create-only mode: do not overwrite a configuration created by someone else.
                if (File.Exists(fullPath))
                    throw new UnderstandMeSettingsConflictException();
                File.Move(tempPath, fullPath, overwrite: false);
            }
            else
            {
                if (!File.Exists(fullPath) || !string.Equals(Hash(File.ReadAllBytes(fullPath)), expectedRevision, StringComparison.Ordinal))
                    throw new UnderstandMeSettingsConflictException();
                // A replace is atomic on supported local filesystems. An external writer
                // between the revision check and rename still requires process coordination.
                File.Move(tempPath, fullPath, overwrite: true);
            }

            return Hash(bytes);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    private static void Validate(UnderstandMeSettings? settings)
    {
        var issues = UnderstandMeSettingsValidator.Validate(settings);
        if (issues.Count != 0)
            throw new UnderstandMeSettingsValidationException(issues);
    }

    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
}
