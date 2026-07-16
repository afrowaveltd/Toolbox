using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'set-primary-category' command.
/// </summary>
internal static class SetPrimaryCategoryCommand
{
    private const string Usage =
        "set-primary-category <path> <id|code|name> <category-name|alias> [--json]";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                "MissingSetPrimaryCategoryPath",
                "The set-primary-category command requires a project root or Jsons/WhenItFails directory path.",
                Usage);
            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            CommandInputError.Show(
                "MissingSetPrimaryCategoryLookup",
                "The set-primary-category command requires an error id, code, or name.",
                Usage);
            return 1;
        }

        if (args.Length < 4)
        {
            ShowMissingCategory();
            return 1;
        }

        bool useJsonOutput = false;
        string? categoryName = null;

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

            if (categoryName is not null || string.IsNullOrWhiteSpace(args[index]))
            {
                ShowInvalidArguments();
                return 1;
            }

            categoryName = args[index];
        }

        if (string.IsNullOrWhiteSpace(categoryName))
        {
            ShowMissingCategory();
            return 1;
        }

        string inputPath = args[1];
        string lookupValue = args[2];
        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().SetPrimaryCategoryAsync(
                inputPath,
                lookupValue,
                categoryName);

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
                "set-primary-category",
                new SetPrimaryCategoryResult(
                    Updated: true,
                    Error: response.Data,
                    FailureCode: null,
                    FailureMessage: null));
        }
        else
        {
            AnsiConsole.MarkupLine(
                "[green]Updated error:[/] {0}",
                Markup.Escape(response.Data.Id));
            AnsiConsole.MarkupLine(
                "[bold]Primary category:[/] {0}",
                Markup.Escape(response.Data.PrimaryCategory));

            if (!string.IsNullOrWhiteSpace(response.Message))
            {
                AnsiConsole.MarkupLine(
                    "[grey]{0}[/]",
                    Markup.Escape(response.Message));
            }
        }

        return 0;
    }

    private static void ShowMissingCategory()
    {
        CommandInputError.Show(
            "MissingSetPrimaryCategoryName",
            "The set-primary-category command requires a category name or alias.",
            Usage);
    }

    private static void ShowInvalidArguments()
    {
        CommandInputError.Show(
            "InvalidSetPrimaryCategoryArguments",
            "The set-primary-category command accepts a path, error lookup, category name or alias, and an optional --json switch.",
            Usage);
    }

    private static void ShowJsonFailure(Response<ErrorDefinition> response)
    {
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "SetPrimaryCategoryFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The primary category could not be changed."
            : response.Message;

        CommandJsonOutput.Write(
            "set-primary-category",
            new SetPrimaryCategoryResult(
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
            : "SetPrimaryCategoryFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The primary category could not be changed."
            : response.Message;

        result.AddError(
            failureCode,
            failureMessage,
            lookupValue);
        new ConsoleValidationResultShow().Show(
            result,
            new ConsoleShowOptions { SourcePath = inputPath });
    }

    private sealed record SetPrimaryCategoryResult(
        bool Updated,
        ErrorDefinition? Error,
        string? FailureCode,
        string? FailureMessage);
}
