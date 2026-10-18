using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Afrowave.Toolbox.UnderstandMe.Localization;

/// <summary>Independent offline localizer with dictionary-less source registration.</summary>
public sealed class JsonTextLocalizer : ITextLocalizer
{
    private readonly ILocalizationDictionaryStore _store;
    private readonly LocalizationOptions _options;
    private readonly object _gate = new();
    private readonly Dictionary<string, IReadOnlyDictionary<string, string>> _snapshots =
        new(StringComparer.OrdinalIgnoreCase);

    public JsonTextLocalizer(ILocalizationDictionaryStore store, LocalizationOptions? options = null)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _options = options ?? new LocalizationOptions();
        ArgumentException.ThrowIfNullOrWhiteSpace(_options.SourceCulture);
    }

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
            foreach (string candidate in CultureCandidates(selected, source))
            {
                if (GetSnapshot(candidate).TryGetValue(key, out string? value))
                    return value;
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
