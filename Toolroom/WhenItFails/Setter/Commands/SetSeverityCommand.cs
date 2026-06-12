using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'set-severity' command: changes the severity of an error definition with safe JSON writing and backup creation.
/// </summary>
internal static class SetSeverityCommand
{
    /// <summary>
    /// Executes the set-severity command.
    /// </summary>
    /// <param name="args">The full command-line arguments (args[0] is "set-severity").</param>
    /// <returns>Exit code: 0 on success, 1 on missing arguments, 2 on failure.</returns>
    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 4
            || string.IsNullOrWhiteSpace(args[1])
            || string.IsNullOrWhiteSpace(args[2])
            || string.IsNullOrWhiteSpace(args[3]))
        {
            ErrorCatalogValidationResult missingSetSeverityArgumentsResult = new();

            missingSetSeverityArgumentsResult.AddError(
                code: "MissingSetSeverityArguments",
                message: "The set-severity command requires a project root or Jsons/WhenItFails directory path, an error id/code/name, and a severity.",
                path: "set-severity <path> <id|code|name> <severity>");

            new ConsoleValidationResultShow().Show(
                missingSetSeverityArgumentsResult,
                new ConsoleShowOptions
                {
                    SourcePath = "command line"
                });

            return 1;
        }

        string inputPath = args[1];
        string lookupValue = args[2];
        string newSeverity = args[3];

        WhenItFailsWorkspaceEditor editor = new();

        Response<ErrorDefinition> response =
            await editor.SetErrorSeverityAsync(
                inputPath,
                lookupValue,
                newSeverity);

        if (!response.IsSuccess || response.Data is null)
        {
            ErrorCatalogValidationResult editFailureResult = new();

            string failureCode = response.Issues.Count > 0
                ? response.Issues[0].Code
                : "SetSeverityFailed";

            string failureMessage = string.IsNullOrWhiteSpace(response.Message)
                ? "Error severity could not be changed."
                : response.Message;

            editFailureResult.AddError(
                code: failureCode,
                message: failureMessage,
                path: lookupValue);

            new ConsoleValidationResultShow().Show(
                editFailureResult,
                new ConsoleShowOptions
                {
                    SourcePath = inputPath
                });

            return 2;
        }

        AnsiConsole.MarkupLine(
            "[green]Updated severity:[/] {0}",
            Markup.Escape(response.Data.Id));

        AnsiConsole.MarkupLine(
            "[bold]New severity:[/] {0}",
            Markup.Escape(response.Data.DefaultSeverity ?? string.Empty));

        if (!string.IsNullOrWhiteSpace(response.Message))
        {
            AnsiConsole.MarkupLine(
                "[grey]{0}[/]",
                Markup.Escape(response.Message));
        }

        return 0;
    }
}