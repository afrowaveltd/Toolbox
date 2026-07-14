using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'set-owner' command.
/// </summary>
internal static class SetOwnerCommand
{
    private const string Usage =
        "set-owner <path> <id|code|name> <owner-name|alias>";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                "MissingSetOwnerPath",
                "The set-owner command requires a project root or Jsons/WhenItFails directory path.",
                Usage);
            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            CommandInputError.Show(
                "MissingSetOwnerLookup",
                "The set-owner command requires an error id, code, or name.",
                Usage);
            return 1;
        }

        if (args.Length < 4 || string.IsNullOrWhiteSpace(args[3]))
        {
            CommandInputError.Show(
                "MissingSetOwnerName",
                "The set-owner command requires an owner name or alias.",
                Usage);
            return 1;
        }

        if (args.Length > 4)
        {
            CommandInputError.Show(
                "InvalidSetOwnerArguments",
                "The set-owner command accepts only a path, error lookup, and owner name or alias.",
                Usage);
            return 1;
        }

        string inputPath = args[1];
        string lookupValue = args[2];
        string ownerName = args[3];

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().SetOwnerAsync(
                inputPath,
                lookupValue,
                ownerName);

        if (!response.IsSuccess || response.Data is null)
        {
            ShowFailure(response, inputPath, lookupValue);
            return 2;
        }

        AnsiConsole.MarkupLine(
            "[green]Updated error:[/] {0}",
            Markup.Escape(response.Data.Id));
        AnsiConsole.MarkupLine(
            "[bold]Owner:[/] {0}",
            Markup.Escape(response.Data.Owner));

        if (!string.IsNullOrWhiteSpace(response.Message))
        {
            AnsiConsole.MarkupLine(
                "[grey]{0}[/]",
                Markup.Escape(response.Message));
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
            : "SetOwnerFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The error owner could not be changed."
            : response.Message;

        result.AddError(
            failureCode,
            failureMessage,
            lookupValue);
        new ConsoleValidationResultShow().Show(
            result,
            new ConsoleShowOptions { SourcePath = inputPath });
    }
}
