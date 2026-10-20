using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Afrowave.Toolbox.UnderstandMe.Localization;

/// <summary>Independent offline localizer with dictionary-less source registration.</summary>
public sealed class DictionaryTextLocalizer : ITextLocalizer
{
    private readonly ILocalizationDictionaryStore _store;
    private readonly LocalizationOptions _options;
    private readonly object _gate = new();
    private readonly Dictionary<string, IReadOnlyDictionary<string, string>> _snapshots =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Creates a localizer over an interchangeable dictionary store.</summary>
    /// <param name="store">Dictionary snapshot and source-registration implementation.</param>
    /// <param name="options">Localization settings, or defaults.</param>
    public DictionaryTextLocalizer(ILocalizationDictionaryStore store, LocalizationOptions? options = null)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _options = options ?? new LocalizationOptions();
        ArgumentException.ThrowIfNullOrWhiteSpace(_options.SourceCulture);
    }

    /// <inheritdoc />
    public string Get(string key, string? culture = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        string source = _options.SourceCulture.Trim();
        string selected = string.IsNullOrWhiteSpace(culture)
            ? CultureInfo.CurrentUICulture.Name : culture.Trim();
        if (string.IsNullOrWhiteSpace(selected))
            selected = source;

        lock (_gate)
        {
            // A symbolic key may differ from its source-language template.
            // Missing dictionaryless entries use the original key as the template.
            string sourceTemplate = GetSnapshot(source).TryGetValue(key, out string? sourceValue)
                ? sourceValue
                : key;

            // Never delete target entries merely because the authoritative
            // source template itself contains invalid placeholder syntax.
            bool sourceSyntaxValid = LocalizationPlaceholderValidator.HasValidPlaceholderSyntax(sourceTemplate);
            foreach (string candidate in CultureCandidates(selected, source))
            {
                if (!GetSnapshot(candidate).TryGetValue(key, out string? value))
                    continue;

                if (!_options.ValidateLocalizedPlaceholders ||
                    LocalizationPlaceholderValidator.HasMatchingPlaceholders(sourceTemplate, value))
                    return value;

                // Repair only this broken target-culture entry. Do not remove
                // its source key, parent translation, or any other culture.
                if (_options.AutoInvalidateInvalidTargetTranslations &&
                    sourceSyntaxValid &&
                    !string.Equals(candidate, source, StringComparison.OrdinalIgnoreCase))
                {
                    TryInvalidateStoredValue(candidate, key, value);
                    // Another writer might already have fixed this entry;
                    // conditional removal preserves that newer valid value.
                    if (GetSnapshot(candidate).TryGetValue(key, out string? latest) &&
                        LocalizationPlaceholderValidator.HasMatchingPlaceholders(sourceTemplate, latest))
                        return latest;
                }
            }

            if (_options.RegisterMissingSourceKeys)
            {
                try
                {
                    if (_store.TryAdd(source, key, key))
                        _snapshots[source] = _store.Load(source);
                }
                catch (IOException) { /* Preserve fallback and disk state. */ }
                catch (UnauthorizedAccessException) { /* Read-only deployment is valid. */ }
                catch (JsonException) { /* Never overwrite a malformed dictionary. */ }
            }
            return key;
        }
    }

    /// <inheritdoc />
    public string Format(string key, IReadOnlyDictionary<string, string> values, string? culture = null)
    {
        ArgumentNullException.ThrowIfNull(values);
        string template = Get(key, culture);
        var result = new StringBuilder(template.Length);

        for (int i = 0; i < template.Length;)
        {
            if (template[i] == '{' && i + 1 < template.Length && template[i + 1] == '{')
            {
                result.Append("{{");
                i += 2;
                continue;
            }

            if (template[i] == '{')
            {
                int end = template.IndexOf('}', i + 1);
                if (end > i + 1 && IsPlaceholderName(template.AsSpan(i + 1, end - i - 1)))
                {
                    string name = template.Substring(i + 1, end - i - 1);
                    if (values.TryGetValue(name, out string? replacement))
                        result.Append(replacement);
                    else
                        result.Append(template, i, end - i + 1);
                    i = end + 1;
                    continue;
                }
            }

            result.Append(template[i++]);
        }
        return result.ToString();
    }

    /// <inheritdoc />
    public bool InvalidateTranslation(string culture, string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(culture);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        string normalized = culture.Trim();

        // Never destroy the canonical dictionaryless or symbolic source.
        if (string.Equals(normalized, _options.SourceCulture.Trim(), StringComparison.OrdinalIgnoreCase))
            return false;

        lock (_gate)
        {
            if (_store is not ILocalizationTranslationInvalidationStore)
                return false;

            try
            {
                // Read the current value instead of trusting a cached snapshot.
                // The actual removal still uses a conditional compare-and-remove.
                var current = _store.Load(normalized);
                _snapshots[normalized] = current;
                if (!current.TryGetValue(key, out string? value))
                    return false;

                return TryInvalidateStoredValue(normalized, key, value);
            }
            catch (IOException) { return false; }
            catch (UnauthorizedAccessException) { return false; }
            catch (JsonException) { return false; }
        }
    }

    private bool TryInvalidateStoredValue(string culture, string key, string expectedValue)
    {
        if (_store is not ILocalizationTranslationInvalidationStore invalidator)
            return false;

        try
        {
            bool removed = invalidator.TryRemove(culture, key, expectedValue);
            // Refresh also when a compare failed: a concurrent writer may
            // have replaced the invalid snapshot with a correct translation.
            _snapshots[culture] = _store.Load(culture);
            return removed;
        }
        catch (IOException) { return false; }
        catch (UnauthorizedAccessException) { return false; }
        catch (JsonException) { return false; }
    }

    /// <inheritdoc />
    public bool Reload(string culture)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(culture);
        try
        {
            IReadOnlyDictionary<string, string> next = _store.Load(culture.Trim());
            lock (_gate)
                _snapshots[culture.Trim()] = next;
            return true;
        }
        catch (IOException) { return false; }
        catch (UnauthorizedAccessException) { return false; }
        catch (JsonException) { return false; }
    }

    private IReadOnlyDictionary<string, string> GetSnapshot(string culture)
    {
        if (_snapshots.TryGetValue(culture, out var existing))
            return existing;

        try
        {
            var loaded = _store.Load(culture);
            _snapshots[culture] = loaded;
            return loaded;
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        catch (JsonException) { }

        // Failed loads are not cached, so repairs are picked up on later lookups.
        return new Dictionary<string, string>(StringComparer.Ordinal);
    }

    private static IEnumerable<string> CultureCandidates(string culture, string source)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string current = culture;
        while (current.Length > 0)
        {
            if (seen.Add(current))
                yield return current;
            int split = current.LastIndexOfAny(['-', '_']);
            if (split < 0)
                break;
            current = current[..split];
        }
        if (seen.Add(source))
            yield return source;
    }

    private static bool IsPlaceholderName(ReadOnlySpan<char> name)
    {
        if (!(char.IsLetter(name[0]) || name[0] == '_'))
            return false;
        foreach (char c in name[1..])
            if (!(char.IsLetterOrDigit(c) || c == '_'))
                return false;
        return true;
    }
}
