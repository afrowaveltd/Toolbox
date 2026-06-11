using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.WhenItFails.Validation;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'summary' (and 'inspect') command: shows a read-only summary of a WhenItFails JSON workspace.
/// </summary>
internal static class SummaryCommand
{
    /// <summary>
    /// Executes the summary command.
    /// </summary>
    /// <param name="args">The full command-line arguments (args[0] is "summary" or "inspect").</param>
    /// <returns>Exit code: 0 on success, 1 on missing path, 2 on validation errors.</returns>
    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            ErrorCatalogValidationResult missingPathResult = new();

            missingPathResult.AddError(
                code: "MissingSummaryPath",
                message: "The summary command requires a project root or Jsons/WhenItFails directory path.",
                path: "summary <path>");

            new ConsoleValidationResultShow().Show(
                missingPathResult,
                new ConsoleShowOptions
                {
                    SourcePath = "command line"
                });

            return 1;
        }

        string inputPath = args[1];

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

        SummaryView.Show(summary);

        return 0;
    }
}
