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
    private const string Usage = "error-add-tag <path> <id|code|name> <tag> [--json]";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 4
            || args.Length > 5
            || string.IsNullOrWhiteSpace(args[1])
            || string.IsNullOrWhiteSpace(args[2]))
        {
            ShowInvalidArguments();
            return 1;
        }

        bool useJsonOutput = false;
        string? tagName = null;

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

            if (tagName is not null || string.IsNullOrWhiteSpace(args[index]))
            {
                ShowInvalidArguments();
                return 1;
            }

            tagName = args[index];
        }

        if (string.IsNullOrWhiteSpace(tagName))
        {
            ShowInvalidArguments();
            return 1;
        }

        string inputPath = args[1];
        string lookupValue = args[2];
        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorAddTagAsync(
                inputPath,
                lookupValue,
                tagName);

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
                "error-add-tag",
                new ErrorAddTagResult(
                    Updated: true,
                    Error: response.Data,
                    AddedTag: TextKeyNormalizer.NormalizeKey(tagName),
                    FailureCode: null,
                    FailureMessage: null));
        }
        else
        {
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
        }

        return 0;
    }

    private static void ShowInvalidArguments()
    {
        CommandInputError.Show(
            "InvalidErrorAddTagArguments",
            "The error-add-tag command requires a path, error lookup, tag, and an optional --json switch.",
            Usage);
    }

    private static void ShowJsonFailure(Response<ErrorDefinition> response)
    {
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "ErrorAddTagFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The tag could not be added to the error definition."
            : response.Message;

        CommandJsonOutput.Write(
            "error-add-tag",
            new ErrorAddTagResult(
                Updated: false,
                Error: null,
                AddedTag: null,
                FailureCode: failureCode,
                FailureMessage: failureMessage));
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

    private sealed record ErrorAddTagResult(
        bool Updated,
        ErrorDefinition? Error,
        string? AddedTag,
        string? FailureCode,
        string? FailureMessage);
}
