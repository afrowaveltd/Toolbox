namespace Afrowave.Toolbox.UnderstandMe.Localization;

/// <summary>Classifies a target-language dictionary entry that needs repair.</summary>
public enum LocalizationRepairReason
{
    /// <summary>The exact target locale does not contain the dictionary key.</summary>
    MissingTarget,
    /// <summary>The exact target translation violates the source placeholder contract.</summary>
    InvalidTargetPlaceholders
}

/// <summary>Represents one missing or invalid translation for a specific target locale scan.</summary>
/// <param name="Key">Exact source dictionary key.</param>
/// <param name="SourceTemplate">Authoritative source-language value used for validation and translation.</param>
/// <param name="Reason">Why the exact target-locale entry needs attention.</param>
public sealed record LocalizationRepairItem(string Key, string SourceTemplate, LocalizationRepairReason Reason);

/// <summary>Finds missing or placeholder-invalid target translations using read-only dictionary snapshots.</summary>
/// <remarks>The scanner neither changes dictionaries nor registers dictionaryless keys or calls translation providers.</remarks>
public static class LocalizationTranslationScanner
{
    /// <summary>Compares authoritative source entries with entries for a single exact target locale.</summary>
    /// <param name="store">Store from which source and target snapshots are loaded once each.</param>
    /// <param name="sourceCulture">Authoritative source locale.</param>
    /// <param name="targetCulture">Exact target locale; a parent-language fallback does not suppress missing entries.</param>
    /// <returns>Repairs ordered by ordinal key; empty for equal source and target locales or empty source dictionaries.</returns>
    /// <exception cref="ArgumentNullException">The store is null.</exception>
    /// <exception cref="ArgumentException">A culture identifier is blank.</exception>
    /// <remarks>Entries with malformed source placeholder syntax are skipped for source-side correction. Store read exceptions propagate to the caller.</remarks>
    public static IReadOnlyList<LocalizationRepairItem> Scan(
        ILocalizationDictionaryStore store, string sourceCulture, string targetCulture)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCulture);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetCulture);
        sourceCulture = sourceCulture.Trim();
        targetCulture = targetCulture.Trim();
        if (string.Equals(sourceCulture, targetCulture, StringComparison.OrdinalIgnoreCase))
            return Array.Empty<LocalizationRepairItem>();

        // Loading each snapshot once avoids destructive, key-by-key read/modify/write cycles.
        IReadOnlyDictionary<string, string> source = store.Load(sourceCulture);
        IReadOnlyDictionary<string, string> target = store.Load(targetCulture);
        var repairs = new List<LocalizationRepairItem>();
        foreach (var entry in source.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            if (!LocalizationPlaceholderValidator.HasValidPlaceholderSyntax(entry.Value))
                continue;
            if (!target.TryGetValue(entry.Key, out string? candidate))
                repairs.Add(new(entry.Key, entry.Value, LocalizationRepairReason.MissingTarget));
            else if (!LocalizationPlaceholderValidator.HasMatchingPlaceholders(entry.Value, candidate))
                repairs.Add(new(entry.Key, entry.Value, LocalizationRepairReason.InvalidTargetPlaceholders));
        }
        return repairs;
    }
}
