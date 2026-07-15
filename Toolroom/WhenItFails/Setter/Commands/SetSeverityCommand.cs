using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'set-severity' command.
/// </summary>
internal static class SetSeverityCommand
{
    private const string Usage = "set-severity <path> <id|code|name> <severity> [--json]";

    /// <summary>
    /// Executes the set-severity command.
    /// </summary>
    /// <param name="args">The full command-line arguments.</param>
    /// <returns>Exit code: 0 on success, 1 on invalid input, 2 on domain failure.</returns>
    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length is < 4 or > 5
            || string.IsNullOrWhiteSpace(args[1])
            || string.IsNullOrWhiteSpace(args[2]))
        {
            ShowInvalidArguments();
            return 1;
        }

        string? severity = null;
        bool useJsonOutput = false;

        for (int index = 3; index < args.Length; index++)
        {
            if (string.Equals(args[index], "--json", StringComparison.OrdinalIgnoreCase))
            {
                if (useJsonOutput)
                {
                    ShowInvalidArguments();
                    return 1;
                }

                useJsonOutput = true;
                continue;
            }

            if (severity is not null)
            {
                ShowInvalidArguments();
                return 1;
            }

            severity = args[index];
        }

        if (string.IsNullOrWhiteSpace(severity))
        {
            ShowInvalidArguments();
            return 1;
        }

        string inputPath = args[1];
        string lookupValue = args[2];
        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().SetErrorSeverityAsync(
                inputPath,
                lookupValue,
                severity);

        if (!response.IsSuccess || response.Data is null)
        {
            if (useJsonOutput)
            {
                ShowJsonFailure(response);
            }
            else
            {
                ShowFailure(response, inputPath, lookupValue);
            }

            return 2;
        }

        if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "set-severity",
                new SetSeverityResult(
                    Updated: true,
                    Error: response.Data,
                    FailureCode: null,
                    FailureMessage: null));
        }
        else
        {
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
        }

        return 0;
    }

    private static void ShowInvalidArguments()
    {
        CommandInputError.Show(
            "InvalidSetSeverityArguments",
            "The set-severity command requires a path, an error id/code/name, one severity, and an optional --json switch.",
            Usage);
    }

    private static void ShowJsonFailure(Response<ErrorDefinition> response)
    {
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "SetSeverityFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "Error severity could not be changed."
            : response.Message;

        CommandJsonOutput.Write(
            "set-severity",
            new SetSeverityResult(
                Updated: false,
                Error: null,
                FailureCode: failureCode,
                FailureMessage: failureMessage));
    }

    private static void ShowFailure(
        Response<ErrorDefinition> response,
        string inputPath,
        string lookupValue)
    {
        ErrorCatalogValidationResult result = new();
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "SetSeverityFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "Error severity could not be changed."
            : response.Message;

        result.AddError(failureCode, failureMessage, lookupValue);
        new ConsoleValidationResultShow().Show(
            result,
            new ConsoleShowOptions { SourcePath = inputPath });
    }

    private sealed record SetSeverityResult(
        bool Updated,
        ErrorDefinition? Error,
        string? FailureCode,
        string? FailureMessage);
}
