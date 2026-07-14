using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

internal static class ErrorRemoveSubcategoryCommand
{
    private const string Usage = "error-remove-subcategory <path> <id|code|name> <subcategory>";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show("MissingErrorRemoveSubcategoryPath", "The error-remove-subcategory command requires a project root or Jsons/WhenItFails directory path.", Usage);
            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            CommandInputError.Show("MissingErrorRemoveSubcategoryLookup", "The error-remove-subcategory command requires an error id, code, or name.", Usage);
            return 1;
        }

        if (args.Length < 4 || string.IsNullOrWhiteSpace(args[3]))
        {
            CommandInputError.Show("MissingErrorRemoveSubcategoryName", "The error-remove-subcategory command requires a subcategory.", Usage);
            return 1;
        }

        if (args.Length > 4)
        {
            CommandInputError.Show("InvalidErrorRemoveSubcategoryArguments", "The error-remove-subcategory command accepts only a path, error lookup, and subcategory.", Usage);
            return 1;
        }

        string inputPath = args[1];
        string lookupValue = args[2];
        string subcategoryName = args[3];

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorRemoveSubcategoryAsync(inputPath, lookupValue, subcategoryName);

        if (!response.IsSuccess || response.Data is null)
        {
            ShowFailure(response, inputPath, lookupValue);
            return 2;
        }

        AnsiConsole.MarkupLine("[green]Updated error:[/] {0}", Markup.Escape(response.Data.Id));
        AnsiConsole.MarkupLine(
            "[bold]Removed subcategory:[/] {0}",
            Markup.Escape(TextKeyNormalizer.NormalizeKey(subcategoryName)));

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
            response.Issues.Count > 0 ? response.Issues[0].Code : "ErrorRemoveSubcategoryFailed",
            string.IsNullOrWhiteSpace(response.Message) ? "The subcategory could not be removed from the error definition." : response.Message,
            path: lookupValue);

        new ConsoleValidationResultShow().Show(result, new ConsoleShowOptions { SourcePath = inputPath });
    }
}
