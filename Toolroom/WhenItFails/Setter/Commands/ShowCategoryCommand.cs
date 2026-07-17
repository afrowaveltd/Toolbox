using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'show-category' command: shows one category from a validated WhenItFails workspace.
/// </summary>
internal static class ShowCategoryCommand
{
    private const string Usage =
        "show-category <path> <category-name> [--plain|--json]";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show("MissingShowCategoryPath", "The show-category command requires a project root or Jsons/WhenItFails directory path.", Usage);
            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            CommandInputError.Show("MissingShowCategoryName", "The show-category command requires a category name.", Usage);
            return 1;
        }

        if (!TryParseOptions(args, out bool usePlainOutput, out bool useJsonOutput))
        {
            CommandInputError.Show(
                "InvalidShowCategoryOutputArguments",
                "The --plain and --json switches are mutually exclusive and may be specified only once.",
                Usage);
            return 1;
        }

        string inputPath = args[1];
        string categoryName = args[2];
        WhenItFailsWorkspaceValidationOutcome validationOutcome =
            await new WhenItFailsWorkspaceValidator().ValidateAsync(inputPath);

        if (!validationOutcome.ValidationResult.IsValid)
        {
            if (useJsonOutput)
            {
                CommandJsonOutput.Write(
                    "show-category",
                    new ShowCategoryResult(
                        Found: false,
                        Category: null,
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
        ErrorCategoryDefinition? category = FindCategory(summary, categoryName);

        if (category is null)
        {
            const string failureCode = "UnknownCategory";
            string failureMessage = $"The category '{categoryName}' does not exist.";

            if (useJsonOutput)
            {
                CommandJsonOutput.Write(
                    "show-category",
                    new ShowCategoryResult(
                        Found: false,
                        Category: null,
                        FailureCode: failureCode,
                        FailureMessage: failureMessage,
                        Validation: null));
            }
            else
            {
                ErrorCatalogValidationResult result = new();
                result.AddError(failureCode, failureMessage, categoryName);
                new ConsoleValidationResultShow().Show(
                    result,
                    new ConsoleShowOptions { SourcePath = summary.DisplayPath });
            }

            return 2;
        }

        if (usePlainOutput)
        {
            CategoryView.ShowPlain(summary, category);
        }
        else if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "show-category",
                new ShowCategoryResult(
                    Found: true,
                    Category: category,
                    FailureCode: null,
                    FailureMessage: null,
                    Validation: null));
        }
        else
        {
            CategoryView.Show(summary, category);
        }

        return 0;
    }

    public static bool TryParseOptions(
        string[] args,
        out bool usePlainOutput,
        out bool useJsonOutput)
    {
        usePlainOutput = false;
        useJsonOutput = false;

        for (int index = 3; index < args.Length; index++)
        {
            if (string.Equals(args[index], "--plain", StringComparison.OrdinalIgnoreCase))
            {
                if (usePlainOutput || useJsonOutput)
                {
                    return false;
                }

                usePlainOutput = true;
                continue;
            }

            if (string.Equals(args[index], "--json", StringComparison.OrdinalIgnoreCase))
            {
                if (useJsonOutput || usePlainOutput)
                {
                    return false;
                }

                useJsonOutput = true;
                continue;
            }

            return false;
        }

        return true;
    }

    public static bool TryParseOptions(string[] args, out bool usePlainOutput)
    {
        return TryParseOptions(args, out usePlainOutput, out _);
    }

    public static ErrorCategoryDefinition? FindCategory(WhenItFailsWorkspaceSummary summary, string categoryName)
    {
        return summary.CategoryCatalog.Categories.FirstOrDefault(category =>
            string.Equals(category.Name, categoryName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(category.DisplayName, categoryName, StringComparison.OrdinalIgnoreCase)
            || category.Aliases.Any(alias => string.Equals(alias, categoryName, StringComparison.OrdinalIgnoreCase)));
    }

    private sealed record ShowCategoryResult(
        bool Found,
        ErrorCategoryDefinition? Category,
        string? FailureCode,
        string? FailureMessage,
        ErrorCatalogValidationResult? Validation);
}
