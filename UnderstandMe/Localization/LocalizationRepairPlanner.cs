namespace Afrowave.Toolbox.UnderstandMe.Localization;

/// <summary>
/// Scheduler-owned state for a previously accepted or attempted repair.
/// Persist it outside this planner to retain protection across restarts.
/// </summary>
public sealed record LocalizationRepairLease(
    string TargetCulture,
    string Key,
    string SourceTemplate,
    bool IsPending,
    DateTimeOffset RetryNotBefore);

/// <summary>A pure, provider-independent selector for eligible per-locale repairs.</summary>
/// <remarks>
/// This does not enqueue jobs, write dictionaries, or retry translations itself.
/// Queue admission must atomically claim the selected key/source revision in a durable store.
/// </remarks>
public static class LocalizationRepairPlanner
{
    /// <summary>
    /// Selects repair candidates that have no active lease and whose retry delay elapsed.
    /// A changed source template is treated as a new revision, eligible immediately.
    /// </summary>
    public static IReadOnlyList<LocalizationRepairItem> SelectEligible(
        string targetCulture,
        IEnumerable<LocalizationRepairItem> candidates,
        IEnumerable<LocalizationRepairLease> leases,
        DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetCulture);
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(leases);
        string target = targetCulture.Trim();

        var existing = new Dictionary<string, LocalizationRepairLease>(StringComparer.Ordinal);
        foreach (var lease in leases)
        {
            ArgumentNullException.ThrowIfNull(lease);
            if (!string.Equals(lease.TargetCulture.Trim(), target, StringComparison.OrdinalIgnoreCase))
                continue;
            if (!existing.TryAdd(lease.Key, lease))
                throw new ArgumentException("Duplicate lease for the target culture and key.", nameof(leases));
        }

        var selected = new List<LocalizationRepairItem>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var candidate in candidates)
        {
            ArgumentNullException.ThrowIfNull(candidate);
            if (!seen.Add(candidate.Key))
                continue;

            if (existing.TryGetValue(candidate.Key, out var lease) &&
                string.Equals(lease.SourceTemplate, candidate.SourceTemplate, StringComparison.Ordinal) &&
                (lease.IsPending || lease.RetryNotBefore > now))
                continue;

            selected.Add(candidate);
        }
        return selected;
    }
}
