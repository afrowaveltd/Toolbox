namespace Afrowave.Toolbox.UnderstandMe.Localization;

/// <summary>Represents scheduler-owned tracking state for a previously admitted or attempted locale repair.</summary>
/// <param name="TargetCulture">Exact target locale of this repair, such as <c>pt-BR</c>.</param>
/// <param name="Key">Ordinal dictionary key.</param>
/// <param name="SourceTemplate">Source-language template when this lease was created; distinguishes source revisions.</param>
/// <param name="IsPending">Whether this source revision has already been admitted and is awaiting completion.</param>
/// <param name="RetryNotBefore">Earliest allowed time for another attempt after a non-pending outcome.</param>
/// <remarks>Persist the lease in the scheduler's durable storage. This record does not itself acquire an atomic lock.</remarks>
public sealed record LocalizationRepairLease(
    string TargetCulture,
    string Key,
    string SourceTemplate,
    bool IsPending,
    DateTimeOffset RetryNotBefore);

/// <summary>Selects eligible per-locale translation repairs without depending on a provider or job queue.</summary>
/// <remarks>
/// Selection is side-effect free. It does not enqueue work, persist cooldowns, or atomically claim repairs.
/// An orchestration layer must atomically claim a selected key and source revision before dispatch.
/// </remarks>
public static class LocalizationRepairPlanner
{
    /// <summary>Filters repairs with a pending lease or an unexpired retry cooldown for the same source revision.</summary>
    /// <param name="targetCulture">Target locale; compared case-insensitively after trimming.</param>
    /// <param name="candidates">Missing or invalid target entries supplied by the read-only scanner.</param>
    /// <param name="leases">Existing per-target repair state, normally loaded from durable storage.</param>
    /// <param name="now">Current instant supplied by the caller to enable deterministic testing and scheduling.</param>
    /// <returns>Eligible candidates in input order, at most one per ordinal dictionary key.</returns>
    /// <exception cref="ArgumentException">The locale is blank or multiple leases exist for the same target and key.</exception>
    /// <exception cref="ArgumentNullException">A required sequence or one of its elements is null.</exception>
    /// <remarks>
    /// A changed source template is a new revision and may be retried immediately.
    /// Calling this method does not reserve the returned work; concurrent callers require external atomic admission.
    /// </remarks>
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
