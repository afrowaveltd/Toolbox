using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Models;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'errors' command: lists error definitions with optional filters and output formats.
/// </summary>
internal static class ErrorsCommand
{
    /// <summary>
    /// Executes the errors command.
    /// </summary>
    /// <param name="args">The full command-line arguments (args[0] is "errors").</param>
    /// <returns>Exit code: 0 on success, 1 on missing path or unknown profile, 2 on validation errors.</returns>
    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            ErrorCatalogValidationResult missingPathResult = new();

            missingPathResult.AddError(
                code: "MissingErrorsPath",
                message: "The errors command requires a project root or Jsons/WhenItFails directory path.",
                path: "errors <path>");

            new ConsoleValidationResultShow().Show(
                missingPathResult,
                new ConsoleShowOptions
                {
                    SourcePath = "command line"
                });

            return 1;
        }

        string inputPath = args[1];
        ErrorListOptions errorListOptions = ParseErrorListOptions(args);

        WhenItFailsWorkspaceValidator validator = new();

        WhenItFailsWorkspaceValidationOutcome validationOutcome =
            await validator.ValidateAsync(inputPath);

        if (!validationOutcome.ValidationResult.IsValid)
        {
            new ConsoleValidationResultShow().Show(
                validationOutcome.ValidationResult,
                new ConsoleShowOptions
                {
                    SourcePath = validationOutcome.DisplayPath
                });

            return 2;
        }

        WhenItFailsWorkspaceSummarizer summarizer = new();

        WhenItFailsWorkspaceSummary summary =
            await summarizer.LoadAsync(inputPath);

        if (!string.IsNullOrWhiteSpace(errorListOptions.Profile)
            && FindProfile(summary, errorListOptions.Profile) is null)
        {
            ErrorCatalogValidationResult unknownProfileResult = new();

            unknownProfileResult.AddError(
                code: "UnknownProfileFilter",
                message: $"The selected profile '{errorListOptions.Profile}' does not exist.",
                path: "--profile");

            new ConsoleValidationResultShow().Show(
                unknownProfileResult,
                new ConsoleShowOptions
                {
                    SourcePath = summary.DisplayPath
                });

            return 1;
        }

        IReadOnlyList<ErrorDefinition> filteredErrors =
            ApplyErrorFilters(summary, errorListOptions).ToList();

        if (errorListOptions.UsePlainOutput)
        {
            ErrorsView.ShowPlain(
                summary,
                filteredErrors,
                errorListOptions);
        }
        else
        {
            ErrorsView.Show(
                summary,
                filteredErrors,
                errorListOptions);
        }

        return 0;
    }

    /// <summary>
    /// Parses error list filter options from command-line arguments.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>The parsed error list options.</returns>
    public static ErrorListOptions ParseErrorListOptions(string[] args)
    {
        return new ErrorListOptions
        {
            UsePlainOutput = HasSwitch(args, "--plain"),
            Owner = ReadOptionValue(args, "--owner"),
            CodeGroup = ReadOptionValue(args, "--group")
                        ?? ReadOptionValue(args, "--code-group"),
            Category = ReadOptionValue(args, "--category"),
            Severity = ReadOptionValue(args, "--severity"),
            Profile = ReadOptionValue(args, "--profile"),
            SearchText = ReadOptionValue(args, "--search")
        };
    }

    /// <summary>
    /// Applies all active filters to the error definitions.
    /// </summary>
    /// <param name="summary">The workspace summary.</param>
    /// <param name="errorListOptions">The filter options.</param>
    /// <returns>The filtered error definitions.</returns>
    public static IEnumerable<ErrorDefinition> ApplyErrorFilters(
        WhenItFailsWorkspaceSummary summary,
        ErrorListOptions errorListOptions)
    {
        IEnumerable<ErrorDefinition> errors = summary.ErrorCatalog.Errors;

        ErrorProfileDefinition? profile = string.IsNullOrWhiteSpace(errorListOptions.Profile)
            ? null
            : FindProfile(summary, errorListOptions.Profile);

        if (profile is not null)
        {
            errors = errors.Where(errorDefinition =>
                MatchesProfile(errorDefinition, profile));
        }

        errors = errors.Where(errorDefinition =>
            MatchesOptionalFilter(errorDefinition.Owner, errorListOptions.Owner)
            && MatchesOptionalFilter(errorDefinition.CodeGroup, errorListOptions.CodeGroup)
            && MatchesOptionalFilter(errorDefinition.PrimaryCategory, errorListOptions.Category)
            && MatchesOptionalFilter(errorDefinition.DefaultSeverity, errorListOptions.Severity)
            && MatchesSearchText(errorDefinition, errorListOptions.SearchText));

        return errors;
    }

    /// <summary>
    /// Finds an error definition by id, numeric code, or name.
    /// </summary>
    /// <param name="summary">The workspace summary.</param>
    /// <param name="lookupValue">The id, code, or name to search for.</param>
    /// <returns>The matching error definition, or null if not found.</returns>
    public static ErrorDefinition? FindErrorDefinition(
        WhenItFailsWorkspaceSummary summary,
        string lookupValue)
    {
        if (int.TryParse(lookupValue, out int numericCode))
        {
            ErrorDefinition? byCode = summary.ErrorCatalog.Errors.FirstOrDefault(errorDefinition =>
                errorDefinition.Code == numericCode);

            if (byCode is not null)
            {
                return byCode;
            }
        }

        return summary.ErrorCatalog.Errors.FirstOrDefault(errorDefinition =>
            string.Equals(
                errorDefinition.Id,
                lookupValue,
                StringComparison.OrdinalIgnoreCase)
            || string.Equals(
                errorDefinition.Name,
                lookupValue,
                StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Finds a profile by name or display name.
    /// </summary>
    /// <param name="summary">The workspace summary.</param>
    /// <param name="profileName">The profile name or display name.</param>
    /// <returns>The matching profile, or null if not found.</returns>
    public static ErrorProfileDefinition? FindProfile(
        WhenItFailsWorkspaceSummary summary,
        string profileName)
    {
        return summary.ProfileCatalog.Profiles.FirstOrDefault(profile =>
            string.Equals(
                profile.Name,
                profileName,
                StringComparison.OrdinalIgnoreCase)
            || string.Equals(
                profile.DisplayName,
                profileName,
                StringComparison.OrdinalIgnoreCase));
    }

    private static bool MatchesProfile(
        ErrorDefinition errorDefinition,
        ErrorProfileDefinition profile)
    {
        bool ownerMatches = profile.IncludeOwners.Count == 0
            || ContainsExactText(profile.IncludeOwners, errorDefinition.Owner);

        bool codeGroupMatches = profile.IncludeCodeGroups.Count == 0
            || ContainsExactText(profile.IncludeCodeGroups, errorDefinition.CodeGroup);

        bool categoryMatches = profile.IncludeCategories.Count == 0
            || ContainsExactText(profile.IncludeCategories, errorDefinition.PrimaryCategory);

        return ownerMatches
               && codeGroupMatches
               && categoryMatches;
    }

    private static bool MatchesOptionalFilter(
        string value,
        string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return true;
        }

        return string.Equals(
            value,
            filter,
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool MatchesSearchText(
        ErrorDefinition errorDefinition,
        string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return true;
        }

        return ContainsText(errorDefinition.Id, searchText)
               || ContainsText(errorDefinition.Name, searchText)
               || ContainsText(errorDefinition.Title, searchText)
               || ContainsText(errorDefinition.Message, searchText)
               || ContainsText(errorDefinition.DeveloperHint, searchText)
               || ContainsText(errorDefinition.DocumentationKey, searchText)
               || ContainsText(errorDefinition.Code.ToString(), searchText)
               || ContainsText(errorDefinition.Owner, searchText)
               || ContainsText(errorDefinition.CodeGroup, searchText)
               || ContainsText(errorDefinition.PrimaryCategory, searchText)
               || ContainsAnyText(errorDefinition.Categories, searchText)
               || ContainsAnyText(errorDefinition.Subcategories, searchText)
               || ContainsAnyText(errorDefinition.Tags, searchText);
    }

    private static bool ContainsText(
        string? value,
        string searchText)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.Contains(
            searchText,
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsAnyText(
        IReadOnlyCollection<string> values,
        string searchText)
    {
        return values.Any(value =>
            ContainsText(
                value,
                searchText));
    }

    private static bool ContainsExactText(
        IReadOnlyCollection<string> values,
        string searchedValue)
    {
        return values.Any(value =>
            string.Equals(
                value,
                searchedValue,
                StringComparison.OrdinalIgnoreCase));
    }

    private static bool HasSwitch(
        string[] args,
        string switchName)
    {
        return args.Any(argument =>
            string.Equals(
                argument,
                switchName,
                StringComparison.OrdinalIgnoreCase));
    }

    private static string? ReadOptionValue(
        string[] args,
        string optionName)
    {
        for (int index = 0; index < args.Length; index++)
        {
            if (!string.Equals(
                    args[index],
                    optionName,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            int valueIndex = index + 1;

            if (valueIndex >= args.Length)
            {
                return null;
            }

            string value = args[valueIndex];

            if (value.StartsWith("--", StringComparison.Ordinal))
            {
                return null;
            }

            return value;
        }

        return null;
    }
}
