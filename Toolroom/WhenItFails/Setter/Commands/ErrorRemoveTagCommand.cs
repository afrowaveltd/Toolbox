using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'error-remove-tag' command.
/// </summary>
internal static class ErrorRemoveTagCommand
{
    private const string Usage = "error-remove-tag <path> <id|code|name> <tag>";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                "MissingErrorRemoveTagPath",
                "The error-remove-tag command requires a project root or Jsons/WhenItFails directory path.",
                Usage);
            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            CommandInputError.Show(
                "MissingErrorRemoveTagLookup",
                "The error-remove-tag command requires an error id, code, or name.",
                Usage);
            return 1;
        }

        if (args.Length < 4 || string.IsNullOrWhiteSpace(args[3]))
        {
            CommandInputError.Show(
                "MissingErrorRemoveTagName",
                "The error-remove-tag command requires a tag.",
                Usage);
            return 1;
        }

        if (args.Length > 4)
        {
            CommandInputError.Show(
                "InvalidErrorRemoveTagArguments",
                "The error-remove-tag command accepts only a path, error lookup, and tag.",
                Usage);
            return 1;
        }

        string inputPath = args[1];
        string lookupValue = args[2];
        string tagName = args[3];

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorRemoveTagAsync(
                inputPath,
                lookupValue,
                tagName);

        if (!response.IsSuccess || response.Data is null)
        {
            ShowFailure(response, inputPath, lookupValue);
            return 2;
        }

        AnsiConsole.MarkupLine(
            "[green]Updated error:[/] {0}",
            Markup.Escape(response.Data.Id));
        AnsiConsole.MarkupLine(
            "[bold]Removed tag:[/] {0}",
            Markup.Escape(TextKeyNormalizer.NormalizeKey(tagName)));

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
        result.AddError(
            response.Issues.Count > 0 ? response.Issues[0].Code : "ErrorRemoveTagFailed",
            string.IsNullOrWhiteSpace(response.Message)
                ? "The tag could not be removed from the error definition."
                : response.Message,
            path: lookupValue);

        new ConsoleValidationResultShow().Show(
            result,
            new ConsoleShowOptions { SourcePath = inputPath });
    }
}
