using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'error-add-tag' command.
/// </summary>
internal static class ErrorAddTagCommand
{
    private const string Usage = "error-add-tag <path> <id|code|name> <tag>";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                "MissingErrorAddTagPath",
                "The error-add-tag command requires a project root or Jsons/WhenItFails directory path.",
                Usage);
            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            CommandInputError.Show(
                "MissingErrorAddTagLookup",
                "The error-add-tag command requires an error id, code, or name.",
                Usage);
            return 1;
        }

        if (args.Length < 4 || string.IsNullOrWhiteSpace(args[3]))
        {
            CommandInputError.Show(
                "MissingErrorAddTagName",
                "The error-add-tag command requires a tag.",
                Usage);
            return 1;
        }

        if (args.Length > 4)
        {
            CommandInputError.Show(
                "InvalidErrorAddTagArguments",
                "The error-add-tag command accepts only a path, error lookup, and tag.",
                Usage);
            return 1;
        }

        string inputPath = args[1];
        string lookupValue = args[2];
        string tagName = args[3];

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorAddTagAsync(
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
            "[bold]Added tag:[/] {0}",
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
            response.Issues.Count > 0 ? response.Issues[0].Code : "ErrorAddTagFailed",
            string.IsNullOrWhiteSpace(response.Message)
                ? "The tag could not be added to the error definition."
                : response.Message,
            path: lookupValue);

        new ConsoleValidationResultShow().Show(
            result,
            new ConsoleShowOptions { SourcePath = inputPath });
    }
}
