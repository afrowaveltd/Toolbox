namespace Afrowave.Toolbox.UnderstandMe.Localization;

/// <summary>
/// Optional writable-store capability to invalidate one target translation
/// without changing any source-language phrase or unrelated locale entry.
/// </summary>
/// <remarks>
/// Removing a value is conditional on its still matching the observed value,
/// to avoid deleting a newer translation produced by another editor.
/// Read-only stores do not need to implement this interface.
/// </remarks>
public interface ILocalizationTranslationInvalidationStore
{
    /// <summary>Removes the key only if its current value exactly matches the expected value.</summary>
    /// <param name="culture">The target culture to modify.</param>
    /// <param name="key">The exact dictionary key.</param>
    /// <param name="expectedValue">The value observed before invalidation.</param>
    /// <returns>True only when that exact entry was actually removed.</returns>
    bool TryRemove(string culture, string key, string expectedValue);
}
