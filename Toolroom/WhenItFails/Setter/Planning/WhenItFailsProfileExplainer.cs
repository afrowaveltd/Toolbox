using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Resolution;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;

/// <summary>
/// Explains why individual errors are included in or excluded from a profile.
/// </summary>
internal sealed class WhenItFailsProfileExplainer
{
    /// <summary>
    /// Loads a workspace and explains one profile using the same semantics as <see cref="ErrorProfileResolver"/>.
    /// </summary>
    public async Task<Response<ProfileExplanation>> ExplainAsync(
        string inputPath,
        string profileName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(inputPath))
        {
            return Response<ProfileExplanation>.Invalid(
                code: "ProfileExplanationPathIsEmpty",
                message: "Project root or Jsons/WhenItFails path cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(profileName))
        {
            return Response<ProfileExplanation>.Invalid(
                code: "ProfileNameIsEmpty",
                message: "Profile name or display name cannot be empty.");
        }

        WhenItFailsWorkspaceValidationOutcome validationOutcome =
            await new WhenItFailsWorkspaceValidator().ValidateAsync(inputPath, cancellationToken);
        if (!validationOutcome.ValidationResult.IsValid)
        {
            return Response<ProfileExplanation>.Invalid(
                code: "WhenItFailsWorkspaceIsInvalid",
                message: "The WhenItFails workspace is invalid and cannot be explained safely.");
        }

        WhenItFailsWorkspaceSummary summary =
            await new WhenItFailsWorkspaceSummarizer().LoadAsync(inputPath, cancellationToken);
        ErrorProfileDefinition? profile = ErrorsCommand.FindProfile(summary, profileName.Trim());
        if (profile is null)
        {
            return Response<ProfileExplanation>.NotFound(
                code: "ProfileNotFound",
                message: $"Profile '{profileName}' was not found.");
        }

        HashSet<string> resolvedIds = new ErrorProfileResolver()
            .Resolve(summary.ErrorCatalog, profile)
            .Select(error => Normalize(error.Id))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        IReadOnlyList<ProfileErrorExplanation> errors = summary.ErrorCatalog.Errors
            .Select(error => ExplainError(profile, error, resolvedIds.Contains(Normalize(error.Id))))
            .ToList();

        ProfileExplanation explanation = new(
            ProfileName: profile.Name,
            DisplayName: profile.DisplayName,
            TotalErrors: errors.Count,
            IncludedErrors: errors.Count(error => error.IsIncluded),
            ExcludedErrors: errors.Count(error => !error.IsIncluded),
            Errors: errors);

        return Response<ProfileExplanation>.Ok(
            explanation,
            $"Profile '{profile.Name}' includes {explanation.IncludedErrors} of {explanation.TotalErrors} error(s).");
    }

    private static ProfileErrorExplanation ExplainError(
        ErrorProfileDefinition profile,
        ErrorDefinition error,
        bool isIncluded)
    {
        List<string> includeReasons = [];
        List<string> exclusionReasons = [];

        AddValueMatch(includeReasons, "explicit-error", error.Id, profile.IncludeErrors);
        AddValueMatch(includeReasons, "owner", error.Owner, profile.IncludeOwners);
        AddValueMatch(includeReasons, "code-group", error.CodeGroup, profile.IncludeCodeGroups);
        AddValueMatch(includeReasons, "primary-category", error.PrimaryCategory, profile.IncludeCategories);
        AddCollectionMatches(includeReasons, "category", error.Categories, profile.IncludeCategories);
        AddCollectionMatches(includeReasons, "subcategory", error.Subcategories, profile.IncludeSubcategories);
        AddCollectionMatches(includeReasons, "tag", error.Tags, profile.IncludeTags);

        AddValueMatch(exclusionReasons, "explicit-error", error.Id, profile.ExcludeErrors);
        AddCollectionMatches(exclusionReasons, "tag", error.Tags, profile.ExcludeTags);

        bool hasIncludeFilters =
            profile.IncludeErrors.Count > 0
            || profile.IncludeOwners.Count > 0
            || profile.IncludeCodeGroups.Count > 0
            || profile.IncludeCategories.Count > 0
            || profile.IncludeSubcategories.Count > 0
            || profile.IncludeTags.Count > 0;

        if (!hasIncludeFilters)
        {
            includeReasons.Add("default:profile-has-no-include-filters");
        }
        else if (includeReasons.Count == 0)
        {
            includeReasons.Add("none:no-include-filter-matched");
        }

        return new ProfileErrorExplanation(
            Id: error.Id,
            Code: error.Code,
            Name: error.Name,
            IsIncluded: isIncluded,
            IncludeReasons: includeReasons,
            ExclusionReasons: exclusionReasons);
    }

    private static void AddValueMatch(
        ICollection<string> reasons,
        string reasonKind,
        string? value,
        IEnumerable<string> filters)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        string normalizedValue = Normalize(value);
        if (filters.Any(filter => string.Equals(
                Normalize(filter),
                normalizedValue,
                StringComparison.OrdinalIgnoreCase)))
        {
            reasons.Add($"{reasonKind}:{normalizedValue}");
        }
    }

    private static void AddCollectionMatches(
        ICollection<string> reasons,
        string reasonKind,
        IEnumerable<string> values,
        IEnumerable<string> filters)
    {
        HashSet<string> normalizedFilters = filters
            .Where(filter => !string.IsNullOrWhiteSpace(filter))
            .Select(Normalize)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (string value in values)
        {
            string normalizedValue = Normalize(value);
            if (normalizedFilters.Contains(normalizedValue))
            {
                reasons.Add($"{reasonKind}:{normalizedValue}");
            }
        }
    }

    private static string Normalize(string value) => TextKeyNormalizer.NormalizeKey(value);
}

internal sealed record ProfileExplanation(
    string ProfileName,
    string DisplayName,
    int TotalErrors,
    int IncludedErrors,
    int ExcludedErrors,
    IReadOnlyList<ProfileErrorExplanation> Errors);

internal sealed record ProfileErrorExplanation(
    string Id,
    int Code,
    string Name,
    bool IsIncluded,
    IReadOnlyList<string> IncludeReasons,
    IReadOnlyList<string> ExclusionReasons);