namespace Afrowave.Toolbox.UnderstandMe.Localization;

/// <summary>Reason why a source dictionary key needs a target-locale translation.</summary>
public enum LocalizationRepairReason
{
    MissingTarget,
    InvalidTargetPlaceholders
}

/// <summary>One exact source key requiring a target-specific translation or review.</summary>
public sealed record LocalizationRepairItem(string Key, string SourceTemplate, LocalizationRepairReason Reason);

/// <summary>
/// Read-only snapshot scanner for missing or invalid target translations.
/// It does not mutate the dictionary, call a provider, or suppress dictionaryless registration.
/// </summary>
public static class LocalizationTranslationScanner
{
    /// <summary>
    /// Scans the authoritative source dictionary against one exact target locale.
    /// A valid parent-locale fallback does not hide a missing exact target entry.
    /// Invalid source placeholders are skipped: they require source repair first.
    /// </summary>
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
