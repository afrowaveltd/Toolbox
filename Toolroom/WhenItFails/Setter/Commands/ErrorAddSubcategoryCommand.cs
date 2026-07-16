using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

internal static class ErrorAddSubcategoryCommand
{
    private const string Usage =
        "error-add-subcategory <path> <id|code|name> <subcategory> [--json]";

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
        string? subcategoryName = null;

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

            if (subcategoryName is not null || string.IsNullOrWhiteSpace(args[index]))
            {
                ShowInvalidArguments();
                return 1;
            }

            subcategoryName = args[index];
        }

        if (string.IsNullOrWhiteSpace(subcategoryName))
        {
            ShowInvalidArguments();
            return 1;
        }

        string inputPath = args[1];
        string lookupValue = args[2];
        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorAddSubcategoryAsync(
                inputPath,
                lookupValue,
                subcategoryName);

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

        string addedSubcategory = response.Data.Subcategories.Last();

        if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "error-add-subcategory",
                new ErrorAddSubcategoryResult(
                    Updated: true,
                    Error: response.Data,
                    AddedSubcategory: addedSubcategory,
                    FailureCode: null,
                    FailureMessage: null));
        }
        else
        {
            AnsiConsole.MarkupLine(
                "[green]Updated error:[/] {0}",
                Markup.Escape(response.Data.Id));
            AnsiConsole.MarkupLine(
                "[bold]Added subcategory:[/] {0}",
                Markup.Escape(addedSubcategory));

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
            "InvalidErrorAddSubcategoryArguments",
            "The error-add-subcategory command requires a path, error lookup, subcategory, and an optional --json switch.",
            Usage);
    }

    private static void ShowJsonFailure(Response<ErrorDefinition> response)
    {
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "ErrorAddSubcategoryFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The subcategory could not be added to the error definition."
            : response.Message;

        CommandJsonOutput.Write(
            "error-add-subcategory",
            new ErrorAddSubcategoryResult(
                Updated: false,
                Error: null,
                AddedSubcategory: null,
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
            response.Issues.Count > 0 ? response.Issues[0].Code : "ErrorAddSubcategoryFailed",
            string.IsNullOrWhiteSpace(response.Message)
                ? "The subcategory could not be added to the error definition."
                : response.Message,
            path: lookupValue);

        new ConsoleValidationResultShow().Show(
            result,
            new ConsoleShowOptions { SourcePath = inputPath });
    }

    private sealed record ErrorAddSubcategoryResult(
        bool Updated,
        ErrorDefinition? Error,
        string? AddedSubcategory,
        string? FailureCode,
        string? FailureMessage);
}
