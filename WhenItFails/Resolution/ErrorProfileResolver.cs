using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.WhenItFails.Resolution;

/// <summary>
/// Resolves the effective collection of errors selected by an error profile.
/// </summary>
/// <remarks>
/// Include filters are combined using OR logic.
/// Exclusion filters act as vetoes.
/// Explicitly excluded error identifiers have the final priority.
/// </remarks>
public sealed class ErrorProfileResolver : IErrorProfileResolver
{
    /// <summary>
    /// Resolves errors from the specified catalog using the supplied profile.
    /// </summary>
    /// <param name="errorCatalog">Error catalog containing source error definitions.</param>
    /// <param name="profile">Profile describing include and exclude filters.</param>
    /// <returns>
    /// Errors matching the profile, preserving their original catalog order.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="errorCatalog"/> or <paramref name="profile"/> is null.
    /// </exception>
    public IReadOnlyList<ErrorDefinition> Resolve(
        ErrorCatalogDocument errorCatalog,
        ErrorProfileDefinition profile)
    {
        ArgumentNullException.ThrowIfNull(errorCatalog);
        ArgumentNullException.ThrowIfNull(profile);

        HashSet<string> includedErrorIds = CreateNormalizedSet(profile.IncludeErrors);
        HashSet<string> excludedErrorIds = CreateNormalizedSet(profile.ExcludeErrors);
        HashSet<string> includedOwners = CreateNormalizedSet(profile.IncludeOwners);
        HashSet<string> includedCodeGroups = CreateNormalizedSet(profile.IncludeCodeGroups);
        HashSet<string> includedCategories = CreateNormalizedSet(profile.IncludeCategories);
        HashSet<string> includedSubcategories = CreateNormalizedSet(profile.IncludeSubcategories);
        HashSet<string> includedTags = CreateNormalizedSet(profile.IncludeTags);
        HashSet<string> excludedTags = CreateNormalizedSet(profile.ExcludeTags);

        bool hasIncludeFilters =
            includedErrorIds.Count > 0 ||
            includedOwners.Count > 0 ||
            includedCodeGroups.Count > 0 ||
            includedCategories.Count > 0 ||
            includedSubcategories.Count > 0 ||
            includedTags.Count > 0;

        List<ErrorDefinition> resolvedErrors = [];

        foreach (ErrorDefinition error in errorCatalog.Errors)
        {
            if (IsExplicitlyExcluded(error, excludedErrorIds))
            {
                continue;
            }

            if (HasExcludedTag(error, excludedTags))
            {
                continue;
            }

            if (!hasIncludeFilters || MatchesAnyIncludeFilter(
                    error,
                    includedErrorIds,
                    includedOwners,
                    includedCodeGroups,
                    includedCategories,
                    includedSubcategories,
                    includedTags))
            {
                resolvedErrors.Add(error);
            }
        }

        return resolvedErrors;
    }

    private static bool MatchesAnyIncludeFilter(
        ErrorDefinition error,
        IReadOnlySet<string> includedErrorIds,
        IReadOnlySet<string> includedOwners,
        IReadOnlySet<string> includedCodeGroups,
        IReadOnlySet<string> includedCategories,
        IReadOnlySet<string> includedSubcategories,
        IReadOnlySet<string> includedTags)
    {
        return MatchesValue(error.Id, includedErrorIds) ||
               MatchesValue(error.Owner, includedOwners) ||
               MatchesValue(error.CodeGroup, includedCodeGroups) ||
               MatchesCategory(error, includedCategories) ||
               MatchesAnyValue(error.Subcategories, includedSubcategories) ||
               MatchesAnyValue(error.Tags, includedTags);
    }

    private static bool MatchesCategory(
        ErrorDefinition error,
        IReadOnlySet<string> includedCategories)
    {
        if (includedCategories.Count == 0)
        {
            return false;
        }

        if (MatchesValue(error.PrimaryCategory, includedCategories))
        {
            return true;
        }

        return MatchesAnyValue(error.Categories, includedCategories);
    }

    private static bool IsExplicitlyExcluded(
        ErrorDefinition error,
        IReadOnlySet<string> excludedErrorIds)
    {
        return MatchesValue(error.Id, excludedErrorIds);
    }

    private static bool HasExcludedTag(
        ErrorDefinition error,
        IReadOnlySet<string> excludedTags)
    {
        return MatchesAnyValue(error.Tags, excludedTags);
    }

    private static bool MatchesValue(
        string? value,
        IReadOnlySet<string> normalizedValues)
    {
        if (normalizedValues.Count == 0 ||
            string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string normalizedValue = TextKeyNormalizer.NormalizeKey(value);

        return normalizedValues.Contains(normalizedValue);
    }

    private static bool MatchesAnyValue(
        IEnumerable<string> values,
        IReadOnlySet<string> normalizedValues)
    {
        if (normalizedValues.Count == 0)
        {
            return false;
        }

        foreach (string value in values)
        {
            if (MatchesValue(value, normalizedValues))
            {
                return true;
            }
        }

        return false;
    }

    private static HashSet<string> CreateNormalizedSet(
        IEnumerable<string> values)
    {
        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(TextKeyNormalizer.NormalizeKey)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}

