namespace Afrowave.Toolbox.UnderstandMe.Localization;

/// <summary>Minimal localization contract, independent of UI, network and translation engines.</summary>
public interface ITextLocalizer
{
    /// <summary>Looks up target, parent culture and source; invalid placeholder templates are skipped by default. Returns key when absent.</summary>
    string Get(string key, string? culture = null);

    /// <summary>Interpolates named placeholders in the localized text, without modifying its catalog entry.</summary>
    string Format(string key, IReadOnlyDictionary<string, string> values, string? culture = null);

    /// <summary>Invalidates exactly one target-culture translation when the store permits writing; source entries are protected.</summary>
    /// <returns>True when the translation was actually removed; false for absent, source or read-only entries.</returns>
    bool InvalidateTranslation(string culture, string key);

    /// <summary>Reloads a locale; failure does not replace the last valid in-memory snapshot.</summary>
    bool Reload(string culture);
}
