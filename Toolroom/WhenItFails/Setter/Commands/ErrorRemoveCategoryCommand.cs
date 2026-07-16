using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

internal static class ErrorRemoveCategoryCommand
{
    private const string Usage =
        "error-remove-category <path> <id|code|name> <category-name|alias> [--json]";

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
            ShowInvalidArguments();
            return 1;
        }

        string inputPath = args[1];
        string lookupValue = args[2];
        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorRemoveCategoryAsync(
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

        string removedCategory = categoryName.Trim();

        if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "error-remove-category",
                new ErrorRemoveCategoryResult(
                    Updated: true,
                    Error: response.Data,
                    RemovedCategory: removedCategory,
                    FailureCode: null,
                    FailureMessage: null));
        }
        else
        {
            AnsiConsole.MarkupLine(
                "[green]Updated error:[/] {0}",
                Markup.Escape(response.Data.Id));
            AnsiConsole.MarkupLine(
                "[bold]Removed category:[/] {0}",
                Markup.Escape(removedCategory));

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
            "InvalidErrorRemoveCategoryArguments",
            "The error-remove-category command requires a path, error lookup, category name or alias, and an optional --json switch.",
            Usage);
    }

    private static void ShowJsonFailure(Response<ErrorDefinition> response)
    {
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "ErrorRemoveCategoryFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The category could not be removed from the error definition."
            : response.Message;

        CommandJsonOutput.Write(
            "error-remove-category",
            new ErrorRemoveCategoryResult(
                Updated: false,
                Error: null,
                RemovedCategory: null,
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
            response.Issues.Count > 0
                ? response.Issues[0].Code
                : "ErrorRemoveCategoryFailed",
            string.IsNullOrWhiteSpace(response.Message)
                ? "The category could not be removed from the error definition."
                : response.Message,
            path: lookupValue);

        new ConsoleValidationResultShow().Show(
            result,
            new ConsoleShowOptions { SourcePath = inputPath });
    }

    private sealed record ErrorRemoveCategoryResult(
        bool Updated,
        ErrorDefinition? Error,
        string? RemovedCategory,
        string? FailureCode,
        string? FailureMessage);
}
