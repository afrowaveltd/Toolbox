using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'set-name' command.
/// </summary>
internal static class SetNameCommand
{
    private const string Usage = "set-name <path> <id|code|name> <new-name>";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 4
            || string.IsNullOrWhiteSpace(args[1])
            || string.IsNullOrWhiteSpace(args[2]))
        {
            CommandInputError.Show(
                "InvalidSetNameArguments",
                "The set-name command requires a path, an error id/code/name, and a new machine-friendly name.",
                Usage);
            return 1;
        }

        string newName = string.Join(" ", args.Skip(3));
        if (string.IsNullOrWhiteSpace(newName))
        {
            CommandInputError.Show(
                "EmptySetNameValue",
                "The new error name cannot be empty.",
                Usage);
            return 1;
        }

        string inputPath = args[1];
        string lookupValue = args[2];
        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().SetNameAsync(
                inputPath,
                lookupValue,
                newName);

        if (!response.IsSuccess || response.Data is null)
        {
            ShowFailure(response, inputPath, lookupValue);
            return 2;
        }

        AnsiConsole.MarkupLine(
            "[green]Updated error name:[/] {0}",
            Markup.Escape(response.Data.Id));
        AnsiConsole.MarkupLine(
            "[bold]New name:[/] {0}",
            Markup.Escape(response.Data.Name));

        if (!string.IsNullOrWhiteSpace(response.Message))
        {
            AnsiConsole.MarkupLine("[grey]{0}[/]", Markup.Escape(response.Message));
        }

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
            : "SetNameFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The error name could not be changed."
            : response.Message;

        result.AddError(failureCode, failureMessage, lookupValue);
        new ConsoleValidationResultShow().Show(
            result,
            new ConsoleShowOptions { SourcePath = inputPath });
    }
}
