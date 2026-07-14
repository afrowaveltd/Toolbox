using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

internal static class ErrorAddCategoryCommand
{
    private const string Usage = "error-add-category <path> <id|code|name> <category-name|alias>";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show("MissingErrorAddCategoryPath", "The error-add-category command requires a project root or Jsons/WhenItFails directory path.", Usage);
            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            CommandInputError.Show("MissingErrorAddCategoryLookup", "The error-add-category command requires an error id, code, or name.", Usage);
            return 1;
        }

        if (args.Length < 4 || string.IsNullOrWhiteSpace(args[3]))
        {
            CommandInputError.Show("MissingErrorAddCategoryName", "The error-add-category command requires a category name or alias.", Usage);
            return 1;
        }

        if (args.Length > 4)
        {
            CommandInputError.Show("InvalidErrorAddCategoryArguments", "The error-add-category command accepts only a path, error lookup, and category name or alias.", Usage);
            return 1;
        }

        string inputPath = args[1];
        string lookupValue = args[2];
        string categoryName = args[3];

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorAddCategoryAsync(inputPath, lookupValue, categoryName);

        if (!response.IsSuccess || response.Data is null)
        {
            ShowFailure(response, inputPath, lookupValue);
            return 2;
        }

        string addedCategory = response.Data.Categories.Last();
        AnsiConsole.MarkupLine("[green]Updated error:[/] {0}", Markup.Escape(response.Data.Id));
        AnsiConsole.MarkupLine("[bold]Added category:[/] {0}", Markup.Escape(addedCategory));

        if (!string.IsNullOrWhiteSpace(response.Message))
        {
            AnsiConsole.MarkupLine("[grey]{0}[/]", Markup.Escape(response.Message));
        }

        return 0;
    }

    private static void ShowFailure(Response<ErrorDefinition> response, string inputPath, string lookupValue)
    {
        ErrorCatalogValidationResult result = new();
        result.AddError(
            response.Issues.Count > 0 ? response.Issues[0].Code : "ErrorAddCategoryFailed",
            string.IsNullOrWhiteSpace(response.Message) ? "The category could not be added to the error definition." : response.Message,
            path: lookupValue);

        new ConsoleValidationResultShow().Show(result, new ConsoleShowOptions { SourcePath = inputPath });
    }
}
