using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'set-code-group' command.
/// </summary>
internal static class SetCodeGroupCommand
{
    private const string Usage =
        "set-code-group <path> <id|code|name> <group-name|prefix>";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                "MissingSetCodeGroupPath",
                "The set-code-group command requires a project root or Jsons/WhenItFails directory path.",
                Usage);
            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            CommandInputError.Show(
                "MissingSetCodeGroupLookup",
                "The set-code-group command requires an error id, code, or name.",
                Usage);
            return 1;
        }

        if (args.Length < 4 || string.IsNullOrWhiteSpace(args[3]))
        {
            CommandInputError.Show(
                "MissingSetCodeGroupName",
                "The set-code-group command requires a code group name or prefix.",
                Usage);
            return 1;
        }

        if (args.Length > 4)
        {
            CommandInputError.Show(
                "InvalidSetCodeGroupArguments",
                "The set-code-group command accepts only a path, error lookup, and code group name or prefix.",
                Usage);
            return 1;
        }

        string inputPath = args[1];
        string lookupValue = args[2];
        string codeGroupName = args[3];

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().SetCodeGroupAsync(
                inputPath,
                lookupValue,
                codeGroupName);

        if (!response.IsSuccess || response.Data is null)
        {
            ShowFailure(response, inputPath, lookupValue);
            return 2;
        }

        AnsiConsole.MarkupLine(
            "[green]Updated error:[/] {0}",
            Markup.Escape(response.Data.Id));
        AnsiConsole.MarkupLine(
            "[bold]Code group:[/] {0} ({1})",
            Markup.Escape(response.Data.CodeGroup),
            Markup.Escape(response.Data.CodePrefix));
        AnsiConsole.MarkupLine(
            "[bold]Numeric code:[/] {0}",
            response.Data.Code);

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
            : "SetCodeGroupFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The error code group could not be changed."
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
