namespace Afrowave.Toolbox.UnderstandMe.Localization;

/// <summary>Optional store capability for conditional, source-validated publishing of translated templates.</summary>
/// <remarks>Implementations must check the authoritative source and current target under the same storage transaction or lock as the write.</remarks>
public interface ILocalizationConditionalTranslationWriter
{
    /// <summary>Writes a validated translation only if its source revision and observed target entry are unchanged.</summary>
    /// <param name="sourceCulture">Culture containing the authoritative source dictionary entry.</param>
    /// <param name="targetCulture">Exact culture receiving the translation.</param>
    /// <param name="key">Dictionary key matched using ordinal comparison.</param>
    /// <param name="expectedSourceTemplate">Expected authoritative source value observed before translation began.</param>
    /// <param name="expectedTargetValue">Previously observed exact target value, or <see langword="null"/> if the target key was absent.</param>
    /// <param name="translation">Candidate template to publish.</param>
    /// <returns><see langword="true"/> if a new target value was committed; otherwise <see langword="false"/> without modification.</returns>
    /// <exception cref="ArgumentException">A culture or key is blank, or source and target cultures are equivalent.</exception>
    /// <exception cref="ArgumentNullException">A required template is null.</exception>
    /// <remarks>Ownership tokens must be checked separately by orchestration. A <see langword="false"/> result also covers an unchanged value.</remarks>
    bool TryPublish(string sourceCulture, string targetCulture, string key,
        string expectedSourceTemplate, string? expectedTargetValue, string translation);
}
