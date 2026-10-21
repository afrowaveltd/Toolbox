using System.Text.Json;

namespace Afrowave.Toolbox.UnderstandMe.Localization;

/// <summary>Flat JSON localization store; one culture per file. No network dependency.</summary>
public sealed class FileJsonDictionaryStore : ILocalizationDictionaryStore, ILocalizationTranslationInvalidationStore, ILocalizationConditionalTranslationWriter
{
    private readonly string _directory;
    private readonly object _gate = new();

    /// <summary>Opens a flat-JSON dictionary store rooted at the specified directory.</summary>
    /// <param name="directory">Root directory for locale JSON files.</param>
    public FileJsonDictionaryStore(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        _directory = Path.GetFullPath(directory);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> Load(string culture)
    {
        string path = GetPath(culture);
        lock (_gate)
            return ReadFile(path);
    }

    /// <inheritdoc />
    public bool TryAdd(string culture, string key, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);
        string path = GetPath(culture);

        lock (_gate)
        {
            var entries = ReadFile(path).ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
            if (entries.ContainsKey(key))
                return false;

            entries.Add(key, value);
            WriteFile(path, entries);
            return true;
        }
    }

    /// <inheritdoc />
    public bool TryRemove(string culture, string key, string expectedValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(expectedValue);
        string path = GetPath(culture);

        lock (_gate)
        {
            var entries = ReadFile(path).ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
            if (!entries.TryGetValue(key, out string? current) ||
                !string.Equals(current, expectedValue, StringComparison.Ordinal))
                return false;

            entries.Remove(key);
            WriteFile(path, entries);
            return true;
        }
    }


    /// <inheritdoc />
    public bool TryPublish(string sourceCulture, string targetCulture, string key,
        string expectedSourceTemplate, string? expectedTargetValue, string translation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCulture);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetCulture);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(expectedSourceTemplate);
        ArgumentNullException.ThrowIfNull(translation);
        string sourcePath = GetPath(sourceCulture);
        string targetPath = GetPath(targetCulture);
        if (string.Equals(Path.GetFileName(sourcePath), Path.GetFileName(targetPath),
                StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Cannot publish translations into the source culture.", nameof(targetCulture));

        if (!LocalizationPlaceholderValidator.HasMatchingPlaceholders(expectedSourceTemplate, translation))
            return false;

        lock (_gate)
        {
            var source = ReadFile(sourcePath);
            if (!source.TryGetValue(key, out string? actualSource) ||
                !string.Equals(actualSource, expectedSourceTemplate, StringComparison.Ordinal))
                return false;

            var target = ReadFile(targetPath).ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
            bool present = target.TryGetValue(key, out string? current);
            if (expectedTargetValue is null ? present :
                !present || !string.Equals(current, expectedTargetValue, StringComparison.Ordinal))
                return false;
            if (present && string.Equals(current, translation, StringComparison.Ordinal))
                return false;

            target[key] = translation;
            WriteFile(targetPath, target);
            return true;
        }
    }

    private void WriteFile(string path, Dictionary<string, string> entries)
    {
        Directory.CreateDirectory(_directory);
        var sorted = entries.OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        string temporaryPath = Path.Combine(_directory, "." + Guid.NewGuid().ToString("N") + ".tmp");

        try
        {
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(sorted,
                new JsonSerializerOptions { WriteIndented = true }));
            File.Move(temporaryPath, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }

    private string GetPath(string culture)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(culture);
        string tag = culture.Trim();
        if (tag[0] is '-' or '_' || tag[^1] is '-' or '_' ||
            !tag.All(c => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9' or '-' or '_'))
            throw new ArgumentException("Invalid culture identifier.", nameof(culture));

        return Path.Combine(_directory, tag + ".json");
    }

    private static IReadOnlyDictionary<string, string> ReadFile(string path)
    {
        var dictionary = new Dictionary<string, string>(StringComparer.Ordinal);
        if (!File.Exists(path))
            return dictionary;

        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            throw new JsonException("A localization dictionary must be a flat JSON object.");

        foreach (JsonProperty property in document.RootElement.EnumerateObject())
        {
            if (string.IsNullOrWhiteSpace(property.Name) || property.Value.ValueKind != JsonValueKind.String)
                throw new JsonException("Localization entries must have a string key and value.");
            if (!dictionary.TryAdd(property.Name, property.Value.GetString()!))
                throw new JsonException("Duplicate dictionary key: " + property.Name);
        }
        return dictionary;
    }
}
