using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'set-developer-hint' command: changes the developer hint of an error definition with safe JSON writing and backup creation.
/// </summary>
internal static class SetDeveloperHintCommand
{
    /// <summary>
    /// Executes the set-developer-hint command.
    /// </summary>
    /// <param name="args">The full command-line arguments (args[0] is "set-developer-hint").</param>
    /// <returns>Exit code: 0 on success, 1 on missing arguments, 2 on failure.</returns>
    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 4
            || string.IsNullOrWhiteSpace(args[1])
            || string.IsNullOrWhiteSpace(args[2])
            || string.IsNullOrWhiteSpace(args[3]))
        {
            ErrorCatalogValidationResult missingSetDeveloperHintArgumentsResult = new();

            missingSetDeveloperHintArgumentsResult.AddError(
                code: "MissingSetDeveloperHintArguments",
                message: "The set-developer-hint command requires a project root or Jsons/WhenItFails directory path, an error id/code/name, and a new developer hint.",
                path: "set-developer-hint <path> <id|code|name> <developer-hint>");

            new ConsoleValidationResultShow().Show(
                missingSetDeveloperHintArgumentsResult,
                new ConsoleShowOptions
                {
                    SourcePath = "command line"
                });

            return 1;
        }

        string inputPath = args[1];
        string lookupValue = args[2];

        string newDeveloperHint = string.Join(
            " ",
            args.Skip(3));

        WhenItFailsWorkspaceEditor editor = new();

        Response<ErrorDefinition> response =
            await editor.SetErrorDeveloperHintAsync(
                inputPath,
                lookupValue,
                newDeveloperHint);

        if (!response.IsSuccess || response.Data is null)
        {
            ErrorCatalogValidationResult editFailureResult = new();

            string failureCode = response.Issues.Count > 0
                ? response.Issues[0].Code
                : "SetDeveloperHintFailed";

            string failureMessage = string.IsNullOrWhiteSpace(response.Message)
                ? "Error developer hint could not be changed."
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
            "[green]Updated developer hint:[/] {0}",
            Markup.Escape(response.Data.Id));

        AnsiConsole.MarkupLine(
            "[bold]New developer hint:[/] {0}",
            Markup.Escape(response.Data.DeveloperHint ?? string.Empty));

        if (!string.IsNullOrWhiteSpace(response.Message))
        {
            AnsiConsole.MarkupLine(
                "[grey]{0}[/]",
                Markup.Escape(response.Message));
        }

        return 0;
    }
}