using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'remove-error' command.
/// </summary>
internal static class RemoveErrorCommand
{
    private const string Usage = "remove-error <path> <id|code|name>";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length != 3
            || string.IsNullOrWhiteSpace(args[1])
            || string.IsNullOrWhiteSpace(args[2]))
        {
            CommandInputError.Show(
                "InvalidRemoveErrorArguments",
                "The remove-error command requires a project root or Jsons/WhenItFails path and one error id, code, or name.",
                Usage);
            return 1;
        }

        string inputPath = args[1];
        string lookupValue = args[2];
        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().RemoveErrorAsync(inputPath, lookupValue);

        if (!response.IsSuccess || response.Data is null)
        {
            ShowFailure(response, inputPath, lookupValue);
            return 2;
        }

        ErrorDefinition error = response.Data;
        AnsiConsole.MarkupLine("[green]Error definition removed[/]");
        AnsiConsole.MarkupLine("[bold]Id:[/] {0}", Markup.Escape(error.Id));
        AnsiConsole.MarkupLine("[bold]Code:[/] {0}", error.Code);
        AnsiConsole.MarkupLine("[bold]Name:[/] {0}", Markup.Escape(error.Name));

        return 0;
    }

    private static void ShowFailure(
        Response<ErrorDefinition> response,
        string inputPath,
        string lookupValue)
    {
        ErrorCatalogValidationResult result = new();
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "RemoveErrorFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The error definition could not be removed."
            : response.Message;

        result.AddError(failureCode, failureMessage, lookupValue);
        new ConsoleValidationResultShow().Show(
            result,
            new ConsoleShowOptions { SourcePath = inputPath });
    }
}
