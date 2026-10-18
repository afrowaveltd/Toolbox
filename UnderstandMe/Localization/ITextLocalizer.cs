namespace Afrowave.Toolbox.UnderstandMe.Localization;

/// <summary>Minimal localization contract, independent of UI, network and translation engines.</summary>
public interface ITextLocalizer
{
    /// <summary>Looks up target, parent culture and source; returns key when absent.</summary>
    string Get(string key, string? culture = null);

    /// <summary>Interpolates named placeholders in the localized text, without modifying its catalog entry.</summary>
    string Format(string key, IReadOnlyDictionary<string, string> values, string? culture = null);

    /// <summary>Reloads a locale; failure does not replace the last valid in-memory snapshot.</summary>
    bool Reload(string culture);
}
