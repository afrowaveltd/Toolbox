namespace Afrowave.Toolbox.UnderstandMe.Localization;

/// <summary>Outcome of validating a strict key-equals-value source dictionary.</summary>
/// <param name="Inspected">Number of source entries inspected.</param>
/// <param name="Invalid">Number of entries whose decoded source key differs from its value.</param>
/// <param name="Removed">Number of invalid entries removed by conditional compare-and-remove.</param>
/// <param name="Unresolved">Number of invalid entries left for a later pass or administrator.</param>
public sealed record AutoSourceDictionaryValidationResult(int Inspected, int Invalid, int Removed, int Unresolved);

/// <summary>Validates and conditionally removes inconsistent entries from an automatically generated source dictionary.</summary>
/// <remarks>
/// Applicable only to source stores explicitly configured with SetValueFromKey semantics.
/// Reads deserialized strings; JSON escape spelling does not affect equality.
/// A missing deletion capability leaves invalid entries untouched and reported.
/// No target dictionaries are changed.
/// </remarks>
public static class AutoSourceDictionaryValidator
{
    /// <summary>Scans an automatic source locale and conditionally removes mismatched entries.</summary>
    /// <param name="store">Dictionary store to inspect.</param>
    /// <param name="sourceCulture">Source-language identifier.</param>
    /// <returns>Counts describing invalid entries and whether conditional removal succeeded.</returns>
    /// <exception cref="ArgumentNullException">The store is null.</exception>
    /// <exception cref="ArgumentException">The source culture is empty or whitespace.</exception>
    /// <remarks>Call only for SetValueFromKey-enabled source storage. A failed comparison is never repaired by overwriting either key or value.</remarks>
    public static AutoSourceDictionaryValidationResult ValidateAndPrune(
        ILocalizationDictionaryStore store, string sourceCulture)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCulture);
        var entries = store.Load(sourceCulture.Trim());
        int invalid = 0;
        int removed = 0;
        foreach (var pair in entries)
        {
            if (string.Equals(pair.Key, pair.Value, StringComparison.Ordinal))
                continue;
            invalid++;
            if (store is ILocalizationTranslationInvalidationStore writable &&
                writable.TryRemove(sourceCulture.Trim(), pair.Key, pair.Value))
                removed++;
        }
        return new(entries.Count, invalid, removed, invalid - removed);
    }
}
