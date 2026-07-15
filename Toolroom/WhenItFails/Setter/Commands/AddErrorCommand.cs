using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'add-error' command.
/// </summary>
internal static class AddErrorCommand
{
    private const string Usage =
        "add-error <path> <owner-name|alias> <group-name|prefix> <category-name|alias> <name> <title> <message> [severity] [--json]";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length is < 8 or > 10)
        {
            ShowInvalidArguments();
            return 1;
        }

        for (int index = 1; index <= 7; index++)
        {
            if (!string.IsNullOrWhiteSpace(args[index]))
            {
                continue;
            }

            CommandInputError.Show(
                "EmptyAddErrorArgument",
                "Required add-error arguments cannot be empty.",
                Usage);
            return 1;
        }

        string severity = "Error";
        bool severitySpecified = false;
        bool useJsonOutput = false;

        for (int index = 8; index < args.Length; index++)
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

            if (severitySpecified)
            {
                ShowInvalidArguments();
                return 1;
            }

            severity = args[index];
            severitySpecified = true;
        }

        string inputPath = args[1];
        AddErrorRequest request = new(
            Owner: args[2],
            CodeGroup: args[3],
            PrimaryCategory: args[4],
            Name: args[5],
            Title: args[6],
            Message: args[7],
            DefaultSeverity: severity);

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().AddErrorAsync(inputPath, request);

        if (!response.IsSuccess || response.Data is null)
        {
            if (useJsonOutput)
            {
                ShowJsonFailure(response);
            }
            else
            {
                ShowFailure(response, inputPath, request.Name);
            }

            return 2;
        }

        ErrorDefinition error = response.Data;
        if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "add-error",
                new AddErrorResult(
                    Added: true,
                    Error: error,
                    FailureCode: null,
                    FailureMessage: null));
        }
        else
        {
            AnsiConsole.MarkupLine("[green]Error definition added[/]");
            AnsiConsole.MarkupLine("[bold]Id:[/] {0}", Markup.Escape(error.Id));
            AnsiConsole.MarkupLine("[bold]Code:[/] {0}", error.Code);
            AnsiConsole.MarkupLine("[bold]Name:[/] {0}", Markup.Escape(error.Name));
            AnsiConsole.MarkupLine("[bold]Owner:[/] {0}", Markup.Escape(error.Owner));
            AnsiConsole.MarkupLine("[bold]Code group:[/] {0}", Markup.Escape(error.CodeGroup));
            AnsiConsole.MarkupLine("[bold]Primary category:[/] {0}", Markup.Escape(error.PrimaryCategory));
            AnsiConsole.MarkupLine("[bold]Severity:[/] {0}", Markup.Escape(error.DefaultSeverity));
        }

        return 0;
    }

    private static void ShowInvalidArguments()
    {
        CommandInputError.Show(
            "InvalidAddErrorArguments",
            "The add-error command requires a path, owner, code group, primary category, name, title, message, optional severity, and optional --json switch.",
            Usage);
    }

    private static void ShowJsonFailure(Response<ErrorDefinition> response)
    {
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "AddErrorFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The error definition could not be added."
            : response.Message;

        CommandJsonOutput.Write(
            "add-error",
            new AddErrorResult(
                Added: false,
                Error: null,
                FailureCode: failureCode,
                FailureMessage: failureMessage));
    }

    private static void ShowFailure(
        Response<ErrorDefinition> response,
        string inputPath,
        string errorName)
    {
        ErrorCatalogValidationResult result = new();
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "AddErrorFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The error definition could not be added."
            : response.Message;

        result.AddError(failureCode, failureMessage, errorName);
        new ConsoleValidationResultShow().Show(
            result,
            new ConsoleShowOptions { SourcePath = inputPath });
    }

    private sealed record AddErrorResult(
        bool Added,
        ErrorDefinition? Error,
        string? FailureCode,
        string? FailureMessage);
}
