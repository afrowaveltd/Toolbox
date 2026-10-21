namespace Afrowave.Toolbox.UnderstandMe.Localization;

/// <summary>A token proving ownership of one exact translation repair attempt.</summary>
/// <param name="TargetCulture">Target locale for the claimed translation.</param>
/// <param name="Key">Exact ordinal dictionary key.</param>
/// <param name="SourceTemplate">Source revision claimed for translation.</param>
/// <param name="Token">Unique token identifying this claim, including across reclaims.</param>
public sealed record LocalizationRepairClaim(string TargetCulture, string Key, string SourceTemplate, Guid Token);

/// <summary>
/// Atomically tracks translation repair claims within a single process.
/// </summary>
/// <remarks>
/// This is a volatile in-memory implementation: it does not survive restarts or coordinate
/// separate hosts. Use a durable transactional implementation for distributed QueueMaster workers.
/// A claim represents job ownership only; the final translation must be revalidated against
/// the latest source before being published.
/// </remarks>
public sealed class InMemoryLocalizationRepairClaims
{
    private readonly object _gate = new();
    private readonly Dictionary<(string Culture, string Key), Entry> _entries = new();

    private sealed record Entry(string SourceTemplate, Guid Token, DateTimeOffset ExpiresAt, DateTimeOffset RetryNotBefore);

    /// <summary>Attempts to claim the exact locale/key/source revision until a finite expiration instant.</summary>
    /// <param name="targetCulture">Target locale. Case-insensitive, with underscores normalized to hyphens.</param>
    /// <param name="key">Exact ordinal dictionary key.</param>
    /// <param name="sourceTemplate">Source template being translated.</param>
    /// <param name="now">Caller-supplied current instant.</param>
    /// <param name="leaseDuration">Positive lease duration, allowing crashed workers to be replaced.</param>
    /// <param name="claim">New ownership token on success; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> when ownership was acquired atomically.</returns>
    /// <exception cref="ArgumentException">A locale or dictionary key is blank.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The duration is not positive or expiration overflows.</exception>
    public bool TryClaim(string targetCulture, string key, string sourceTemplate,
        DateTimeOffset now, TimeSpan leaseDuration, out LocalizationRepairClaim? claim)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(targetCulture);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(sourceTemplate);
        if (leaseDuration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(leaseDuration));
        DateTimeOffset expiresAt = now.Add(leaseDuration);
        string culture = NormalizeCulture(targetCulture);
        lock (_gate)
        {
            var identity = (culture, key);
            if (_entries.TryGetValue(identity, out Entry? current) &&
                string.Equals(current.SourceTemplate, sourceTemplate, StringComparison.Ordinal) &&
                (current.Token != Guid.Empty && current.ExpiresAt > now || current.RetryNotBefore > now))
            {
                claim = null;
                return false;
            }

            var token = Guid.NewGuid();
            _entries[identity] = new Entry(sourceTemplate, token, expiresAt, DateTimeOffset.MinValue);
            claim = new LocalizationRepairClaim(culture, key, sourceTemplate, token);
            return true;
        }
    }

    /// <summary>Releases an owned claim, optionally imposing a retry cooldown.</summary>
    /// <param name="claim">Token returned by a successful <see cref="TryClaim"/>.</param>
    /// <param name="retryNotBefore">Earliest instant for another attempt of this source revision.</param>
    /// <returns><see langword="false"/> if the claim has been replaced or already released.</returns>
    /// <remarks>Stale workers cannot release a newer claim. Expired workers must not publish results.</remarks>
    public bool TryComplete(LocalizationRepairClaim claim, DateTimeOffset retryNotBefore)
    {
        ArgumentNullException.ThrowIfNull(claim);
        lock (_gate)
        {
            var identity = (NormalizeCulture(claim.TargetCulture), claim.Key);
            if (!_entries.TryGetValue(identity, out Entry? current) ||
                current.Token != claim.Token ||
                !string.Equals(current.SourceTemplate, claim.SourceTemplate, StringComparison.Ordinal))
                return false;

            _entries[identity] = current with
            {
                Token = Guid.Empty,
                ExpiresAt = DateTimeOffset.MinValue,
                RetryNotBefore = retryNotBefore
            };
            return true;
        }
    }

    private static string NormalizeCulture(string culture) =>
        culture.Trim().Replace('_', '-').ToUpperInvariant();
}
