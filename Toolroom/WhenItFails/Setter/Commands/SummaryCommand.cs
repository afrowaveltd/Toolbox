using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.WhenItFails.Validation;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'summary' (and 'inspect') command: shows a read-only summary of a WhenItFails JSON workspace.
/// </summary>
internal static class SummaryCommand
{
    private const string Usage = "summary <path> [--json]";

    /// <summary>
    /// Executes the summary command.
    /// </summary>
    /// <param name="args">The full command-line arguments (args[0] is "summary" or "inspect").</param>
    /// <returns>Exit code: 0 on success, 1 on invalid command input, 2 on validation errors.</returns>
    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            ErrorCatalogValidationResult missingPathResult = new();
            missingPathResult.AddError(
                code: "MissingSummaryPath",
                message: "The summary command requires a project root or Jsons/WhenItFails directory path.",
                path: Usage);

            new ConsoleValidationResultShow().Show(
                missingPathResult,
                new ConsoleShowOptions { SourcePath = "command line" });
            return 1;
        }

        if (!TryParseOptions(args, out bool useJsonOutput))
        {
            CommandInputError.Show(
                code: "InvalidSummaryArguments",
                message: "The summary command accepts only a path and the optional --json switch, which may be specified only once.",
                path: Usage);
            return 1;
        }

        string inputPath = args[1];
        WhenItFailsWorkspaceValidationOutcome validationOutcome =
            await new WhenItFailsWorkspaceValidator().ValidateAsync(inputPath);

        if (!validationOutcome.ValidationResult.IsValid)
        {
            if (useJsonOutput)
            {
                CommandJsonOutput.Write(
                    "summary",
                    new SummaryResult(
                        Loaded: false,
                        Summary: null,
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

        if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "summary",
                new SummaryResult(
                    Loaded: true,
                    Summary: summary,
                    Validation: null));
        }
        else
        {
            SummaryView.Show(summary);
        }

        return 0;
    }

    public static bool TryParseOptions(string[] args, out bool useJsonOutput)
    {
        useJsonOutput = false;

        for (int index = 2; index < args.Length; index++)
        {
            if (!string.Equals(args[index], "--json", StringComparison.OrdinalIgnoreCase)
                || useJsonOutput)
            {
                return false;
            }

            useJsonOutput = true;
        }

        return true;
    }

    private sealed record SummaryResult(
        bool Loaded,
        WhenItFailsWorkspaceSummary? Summary,
        ErrorCatalogValidationResult? Validation);
}
