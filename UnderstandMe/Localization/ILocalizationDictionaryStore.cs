namespace Afrowave.Toolbox.UnderstandMe.Localization;

/// <summary>Reads and registers normalized locale dictionaries independently of their backing store.</summary>
/// <remarks>Initial synchronous contract intended for local or cached snapshots. Future remote/database source refresh can use separate asynchronous interfaces.</remarks>
public interface ILocalizationDictionaryStore
{
    /// <summary>Returns an empty dictionary if the locale file is absent; throws on invalid data.</summary>
    IReadOnlyDictionary<string, string> Load(string culture);

    /// <summary>Adds a new source phrase without replacing an existing entry.</summary>
    /// <returns>True only if an entry was saved.</returns>
    bool TryAdd(string culture, string key, string value);
}
