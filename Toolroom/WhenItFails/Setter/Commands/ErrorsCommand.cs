using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Resolution;
using Afrowave.Toolbox.WhenItFails.Validation;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Models;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'errors' command: lists error definitions with optional filters and output formats.
/// </summary>
internal static class ErrorsCommand
{
    private const string Usage =
        "errors <path> [--owner <value>] [--group|--code-group <value>] [--category <value>] [--severity <value>] [--profile <value>] [--search <value>] [--plain|--json]";

    /// <summary>
    /// Executes the errors command.
    /// </summary>
    /// <param name="args">The full command-line arguments (args[0] is "errors").</param>
    /// <returns>Exit code: 0 on success, 1 on input errors or unknown profile, 2 on validation errors.</returns>
    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                "MissingErrorsPath",
                "The errors command requires a project root or Jsons/WhenItFails directory path.",
                Usage);
            return 1;
        }

        if (!TryParseErrorListOptions(args, out ErrorListOptions errorListOptions))
        {
            CommandInputError.Show(
                "InvalidErrorsArguments",
                "The errors command received an unknown, duplicate, conflicting, or incomplete option.",
                Usage);
            return 1;
        }

        string inputPath = args[1];
        WhenItFailsWorkspaceValidationOutcome validationOutcome =
            await new WhenItFailsWorkspaceValidator().ValidateAsync(inputPath);

        if (!validationOutcome.ValidationResult.IsValid)
        {
            if (errorListOptions.UseJsonOutput)
            {
                CommandJsonOutput.Write(
                    "errors",
                    new ErrorsResult(
                        Loaded: false,
                        Errors: null,
                        Options: errorListOptions,
                        FailureCode: null,
                        FailureMessage: null,
                        Validation: validationOutcome.ValidationResult));
            }
            else
            {
                new ConsoleValidationResultShow().Show(
                    validationOutcome.ValidationResult,
                    new ConsoleShowOptions { SourcePath = validationOutcome.DisplayPath });
            }

            return 2;
        }

        WhenItFailsWorkspaceSummary summary =
            await new WhenItFailsWorkspaceSummarizer().LoadAsync(inputPath);

        if (!string.IsNullOrWhiteSpace(errorListOptions.Profile)
            && FindProfile(summary, errorListOptions.Profile) is null)
        {
            const string failureCode = "UnknownProfileFilter";
            string failureMessage =
                $"The selected profile '{errorListOptions.Profile}' does not exist.";

            if (errorListOptions.UseJsonOutput)
            {
                CommandJsonOutput.Write(
                    "errors",
                    new ErrorsResult(
                        Loaded: true,
                        Errors: null,
                        Options: errorListOptions,
                        FailureCode: failureCode,
                        FailureMessage: failureMessage,
                        Validation: null));
            }
            else
            {
                ErrorCatalogValidationResult unknownProfileResult = new();
                unknownProfileResult.AddError(
                    failureCode,
                    failureMessage,
                    "--profile");
                new ConsoleValidationResultShow().Show(
                    unknownProfileResult,
                    new ConsoleShowOptions { SourcePath = summary.DisplayPath });
            }

            return 1;
        }

        IReadOnlyList<ErrorDefinition> filteredErrors =
            ApplyErrorFilters(summary, errorListOptions).ToList();

        if (errorListOptions.UsePlainOutput)
        {
            ErrorsView.ShowPlain(summary, filteredErrors, errorListOptions);
        }
        else if (errorListOptions.UseJsonOutput)
        {
            CommandJsonOutput.Write(
                "errors",
                new ErrorsResult(
                    Loaded: true,
                    Errors: filteredErrors,
                    Options: errorListOptions,
                    FailureCode: null,
                    FailureMessage: null,
                    Validation: null));
        }
        else
        {
            ErrorsView.Show(summary, filteredErrors, errorListOptions);
        }

        return 0;
    }

    /// <summary>
    /// Parses and validates error list filter options from command-line arguments.
    /// </summary>
    public static bool TryParseErrorListOptions(
        string[] args,
        out ErrorListOptions options)
    {
        options = new ErrorListOptions();
        HashSet<string> seenOptions = new(StringComparer.OrdinalIgnoreCase);

        for (int index = 2; index < args.Length; index++)
        {
            string argument = args[index];

            if (string.Equals(argument, "--plain", StringComparison.OrdinalIgnoreCase))
            {
                if (!seenOptions.Add("--plain") || options.UseJsonOutput)
                {
                    return false;
                }

                options.UsePlainOutput = true;
                continue;
            }

            if (string.Equals(argument, "--json", StringComparison.OrdinalIgnoreCase))
            {
                if (!seenOptions.Add("--json") || options.UsePlainOutput)
                {
                    return false;
                }

                options.UseJsonOutput = true;
                continue;
            }

            string normalizedOption = string.Equals(
                argument,
                "--code-group",
                StringComparison.OrdinalIgnoreCase)
                ? "--group"
                : argument;

            if (!IsValueOption(normalizedOption)
                || !seenOptions.Add(normalizedOption)
                || index + 1 >= args.Length
                || args[index + 1].StartsWith("--", StringComparison.Ordinal))
            {
                return false;
            }

            string value = args[++index];
            SetOptionValue(options, normalizedOption, value);
        }

        return true;
    }

    /// <summary>
    /// Parses error list filter options from command-line arguments.
    /// </summary>
    public static ErrorListOptions ParseErrorListOptions(string[] args)
    {
        return TryParseErrorListOptions(args, out ErrorListOptions options)
            ? options
            : new ErrorListOptions();
    }

    /// <summary>
    /// Applies all active filters to the error definitions.
    /// </summary>
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
            errors = new ErrorProfileResolver().Resolve(
                summary.ErrorCatalog,
                profile);
        }

        return errors.Where(errorDefinition =>
            MatchesOptionalFilter(errorDefinition.Owner, errorListOptions.Owner)
            && MatchesOptionalFilter(errorDefinition.CodeGroup, errorListOptions.CodeGroup)
            && MatchesOptionalFilter(errorDefinition.PrimaryCategory, errorListOptions.Category)
            && MatchesOptionalFilter(errorDefinition.DefaultSeverity, errorListOptions.Severity)
            && MatchesSearchText(errorDefinition, errorListOptions.SearchText));
    }

    /// <summary>
    /// Finds an error definition by id, numeric code, or name.
    /// </summary>
    public static ErrorDefinition? FindErrorDefinition(
        WhenItFailsWorkspaceSummary summary,
        string lookupValue)
    {
        if (int.TryParse(lookupValue, out int numericCode))
        {
            ErrorDefinition? byCode = summary.ErrorCatalog.Errors.FirstOrDefault(
                errorDefinition => errorDefinition.Code == numericCode);

            if (byCode is not null)
            {
                return byCode;
            }
        }

        return summary.ErrorCatalog.Errors.FirstOrDefault(errorDefinition =>
            string.Equals(errorDefinition.Id, lookupValue, StringComparison.OrdinalIgnoreCase)
            || string.Equals(errorDefinition.Name, lookupValue, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Finds a profile by name or display name.
    /// </summary>
    public static ErrorProfileDefinition? FindProfile(
        WhenItFailsWorkspaceSummary summary,
        string profileName)
    {
        return summary.ProfileCatalog.Profiles.FirstOrDefault(profile =>
            string.Equals(profile.Name, profileName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(profile.DisplayName, profileName, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsValueOption(string option)
    {
        return option.Equals("--owner", StringComparison.OrdinalIgnoreCase)
               || option.Equals("--group", StringComparison.OrdinalIgnoreCase)
               || option.Equals("--category", StringComparison.OrdinalIgnoreCase)
               || option.Equals("--severity", StringComparison.OrdinalIgnoreCase)
               || option.Equals("--profile", StringComparison.OrdinalIgnoreCase)
               || option.Equals("--search", StringComparison.OrdinalIgnoreCase);
    }

    private static void SetOptionValue(
        ErrorListOptions options,
        string option,
        string value)
    {
        if (option.Equals("--owner", StringComparison.OrdinalIgnoreCase))
        {
            options.Owner = value;
        }
        else if (option.Equals("--group", StringComparison.OrdinalIgnoreCase))
        {
            options.CodeGroup = value;
        }
        else if (option.Equals("--category", StringComparison.OrdinalIgnoreCase))
        {
            options.Category = value;
        }
        else if (option.Equals("--severity", StringComparison.OrdinalIgnoreCase))
        {
            options.Severity = value;
        }
        else if (option.Equals("--profile", StringComparison.OrdinalIgnoreCase))
        {
            options.Profile = value;
        }
        else if (option.Equals("--search", StringComparison.OrdinalIgnoreCase))
        {
            options.SearchText = value;
        }
    }

    private static bool MatchesOptionalFilter(string value, string? filter)
    {
        return string.IsNullOrWhiteSpace(filter)
               || string.Equals(value, filter, StringComparison.OrdinalIgnoreCase);
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

    private static bool ContainsText(string? value, string searchText)
    {
        return !string.IsNullOrWhiteSpace(value)
               && value.Contains(searchText, StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsAnyText(
        IReadOnlyCollection<string> values,
        string searchText)
    {
        return values.Any(value => ContainsText(value, searchText));
    }

    private sealed record ErrorsResult(
        bool Loaded,
        IReadOnlyList<ErrorDefinition>? Errors,
        ErrorListOptions Options,
        string? FailureCode,
        string? FailureMessage,
        ErrorCatalogValidationResult? Validation);
}
