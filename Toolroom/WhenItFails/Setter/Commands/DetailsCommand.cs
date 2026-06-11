using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'details' (and 'detail') command: shows one error definition in detail.
/// </summary>
internal static class DetailsCommand
{
    /// <summary>
    /// Executes the details command.
    /// </summary>
    /// <param name="args">The full command-line arguments (args[0] is "details" or "detail").</param>
    /// <returns>Exit code: 0 on success, 1 on missing arguments or not found, 2 on validation errors.</returns>
    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 3
            || string.IsNullOrWhiteSpace(args[1])
            || string.IsNullOrWhiteSpace(args[2]))
        {
            ErrorCatalogValidationResult missingDetailsArgumentsResult = new();

            missingDetailsArgumentsResult.AddError(
                code: "MissingDetailsArguments",
                message: "The details command requires a project root or Jsons/WhenItFails directory path and an error id, code or name.",
                path: "details <path> <id|code|name>");

            new ConsoleValidationResultShow().Show(
                missingDetailsArgumentsResult,
                new ConsoleShowOptions
                {
                    SourcePath = "command line"
                });

            return 1;
        }

        string inputPath = args[1];
        string lookupValue = args[2];
        bool usePlainOutput = HasSwitch(args, "--plain");

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

        ErrorDefinition? errorDefinition = ErrorsCommand.FindErrorDefinition(
            summary,
            lookupValue);

        if (errorDefinition is null)
        {
            ErrorCatalogValidationResult notFoundResult = new();

            notFoundResult.AddError(
                code: "ErrorDefinitionNotFound",
                message: $"No error definition was found for '{lookupValue}'. Search by Id, Code or Name.",
                path: lookupValue);

            new ConsoleValidationResultShow().Show(
                notFoundResult,
                new ConsoleShowOptions
                {
                    SourcePath = summary.DisplayPath
                });

            return 1;
        }

        if (usePlainOutput)
        {
            DetailsView.ShowPlain(
                summary,
                errorDefinition);
        }
        else
        {
            DetailsView.Show(
                summary,
                errorDefinition);
        }

        return 0;
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
}
