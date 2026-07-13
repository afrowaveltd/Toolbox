using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'show-category' command: shows one category from a validated WhenItFails workspace.
/// </summary>
internal static class ShowCategoryCommand
{
    public static async Task<int> ExecuteAsync(string[] args)
    {
        const string usage = "show-category <path> <category-name> [--plain]";

        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            ShowCommandInputError("MissingShowCategoryPath", "The show-category command requires a project root or Jsons/WhenItFails directory path.", usage);
            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            ShowCommandInputError("MissingShowCategoryName", "The show-category command requires a category name.", usage);
            return 1;
        }

        if (!TryParseOptions(args, out bool usePlainOutput))
        {
            ShowCommandInputError("InvalidShowCategoryArguments", "The show-category command accepts only a path, a category name, and the optional --plain switch.", usage);
            return 1;
        }

        string inputPath = args[1];
        string categoryName = args[2];
        WhenItFailsWorkspaceValidator validator = new();
        WhenItFailsWorkspaceValidationOutcome validationOutcome = await validator.ValidateAsync(inputPath);

        if (!validationOutcome.ValidationResult.IsValid)
        {
            new ConsoleValidationResultShow().Show(
                validationOutcome.ValidationResult,
                new ConsoleShowOptions { SourcePath = validationOutcome.DisplayPath });
            return 2;
        }

        WhenItFailsWorkspaceSummary summary = await new WhenItFailsWorkspaceSummarizer().LoadAsync(inputPath);
        ErrorCategoryDefinition? category = FindCategory(summary, categoryName);

        if (category is null)
        {
            ErrorCatalogValidationResult result = new();
            result.AddError("UnknownCategory", $"The category '{categoryName}' does not exist.", categoryName);
            new ConsoleValidationResultShow().Show(result, new ConsoleShowOptions { SourcePath = summary.DisplayPath });
            return 2;
        }

        if (usePlainOutput)
        {
            CategoryView.ShowPlain(summary, category);
        }
        else
        {
            CategoryView.Show(summary, category);
        }

        return 0;
    }

    public static bool TryParseOptions(string[] args, out bool usePlainOutput)
    {
        return PlainOutputOptionParser.TryParse(
            args,
            3,
            out usePlainOutput);
    }

    public static ErrorCategoryDefinition? FindCategory(WhenItFailsWorkspaceSummary summary, string categoryName)
    {
        return summary.CategoryCatalog.Categories.FirstOrDefault(category =>
            string.Equals(category.Name, categoryName, StringComparison.OrdinalIgnoreCase)
            || string.Equals(category.DisplayName, categoryName, StringComparison.OrdinalIgnoreCase)
            || category.Aliases.Any(alias => string.Equals(alias, categoryName, StringComparison.OrdinalIgnoreCase)));
    }

    private static void ShowCommandInputError(string code, string message, string path)
    {
        ErrorCatalogValidationResult result = new();
        result.AddError(code, message, path);
        new ConsoleValidationResultShow().Show(result, new ConsoleShowOptions { SourcePath = "command line" });
    }
}
