using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'set-documentation-key' command: changes the documentation key of an error definition with safe JSON writing and backup creation.
/// </summary>
internal static class SetDocumentationKeyCommand
{
    /// <summary>
    /// Executes the set-documentation-key command.
    /// </summary>
    /// <param name="args">The full command-line arguments (args[0] is "set-documentation-key").</param>
    /// <returns>Exit code: 0 on success, 1 on missing arguments, 2 on failure.</returns>
    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 4
            || string.IsNullOrWhiteSpace(args[1])
            || string.IsNullOrWhiteSpace(args[2])
            || string.IsNullOrWhiteSpace(args[3]))
        {
            ErrorCatalogValidationResult missingSetDocumentationKeyArgumentsResult = new();

            missingSetDocumentationKeyArgumentsResult.AddError(
                code: "MissingSetDocumentationKeyArguments",
                message: "The set-documentation-key command requires a project root or Jsons/WhenItFails directory path, an error id/code/name, and a new documentation key.",
                path: "set-documentation-key <path> <id|code|name> <documentation-key>");

            new ConsoleValidationResultShow().Show(
                missingSetDocumentationKeyArgumentsResult,
                new ConsoleShowOptions
                {
                    SourcePath = "command line"
                });

            return 1;
        }

        string inputPath = args[1];
        string lookupValue = args[2];

        string newDocumentationKey = string.Join(
            " ",
            args.Skip(3));

        WhenItFailsWorkspaceEditor editor = new();

        Response<ErrorDefinition> response =
            await editor.SetErrorDocumentationKeyAsync(
                inputPath,
                lookupValue,
                newDocumentationKey);

        if (!response.IsSuccess || response.Data is null)
        {
            ErrorCatalogValidationResult editFailureResult = new();

            string failureCode = response.Issues.Count > 0
                ? response.Issues[0].Code
                : "SetDocumentationKeyFailed";

            string failureMessage = string.IsNullOrWhiteSpace(response.Message)
                ? "Error documentation key could not be changed."
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
            "[green]Updated documentation key:[/] {0}",
            Markup.Escape(response.Data.Id));

        AnsiConsole.MarkupLine(
            "[bold]New documentation key:[/] {0}",
            Markup.Escape(response.Data.DocumentationKey ?? string.Empty));

        if (!string.IsNullOrWhiteSpace(response.Message))
        {
            AnsiConsole.MarkupLine(
                "[grey]{0}[/]",
                Markup.Escape(response.Message));
        }

        return 0;
    }
}