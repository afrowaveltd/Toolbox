using System.Reflection;
using System.Text.Json;

namespace Afrowave.Toolbox.UnderstandMe.Configurator;

/// <summary>Offline, embedded, English-first UI messages for the standalone Configurator.</summary>
/// <remarks>
/// Bundled messages are an offline bootstrap: they must work even if the edited
/// UnderstandMe configuration or any translation provider is broken.
/// This lookup can later delegate to UnderstandMe's localization primitives.
/// </remarks>
public sealed class ConfiguratorStrings
{
    private static readonly IReadOnlyDictionary<string, string> English = ReadEmbedded("en");
    private static readonly IReadOnlyDictionary<string, string> Czech = ReadEmbedded("cs");

    /// <summary>Currently selected locale code.</summary>
    public string Language { get; }

    /// <summary>Select one of the bundled locales, falling back to English for unsupported codes.</summary>
    public ConfiguratorStrings(string? language)
    {
        Language = language == "cs" ? "cs" : "en";
    }

    /// <summary>Look up a UI message using English fallback, then its stable key.</summary>
    public string this[string key]
    {
        get
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            if (Language == "cs" && Czech.TryGetValue(key, out var localized))
                return localized;
            return English.TryGetValue(key, out var english) ? english : key;
        }
    }

    /// <summary>Return a snapshot of the bundled locale messages for parity tests.</summary>
    public static IReadOnlyDictionary<string, string> GetBundledMessages(string language) =>
        language switch
        {
            "en" => English,
            "cs" => Czech,
            _ => throw new ArgumentOutOfRangeException(nameof(language))
        };

    private static IReadOnlyDictionary<string, string> ReadEmbedded(string language)
    {
        var assembly = typeof(ConfiguratorStrings).Assembly;
        using var stream = assembly.GetManifestResourceStream("Configurator." + language + ".json")
            ?? throw new InvalidOperationException("Missing embedded Configurator localization: " + language);
        var values = JsonSerializer.Deserialize<Dictionary<string, string>>(stream)
            ?? throw new InvalidOperationException("Invalid embedded Configurator localization: " + language);

        if (values.Any(item => string.IsNullOrWhiteSpace(item.Key) || string.IsNullOrWhiteSpace(item.Value)))
            throw new InvalidOperationException("Blank Configurator translation key or value: " + language);
        return values;
    }
}
