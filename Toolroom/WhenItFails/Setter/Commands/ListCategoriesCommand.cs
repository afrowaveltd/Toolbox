using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.WhenItFails.Validation;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'list-categories' command: lists categories from a validated WhenItFails workspace.
/// </summary>
internal static class ListCategoriesCommand
{
    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                code: "MissingListCategoriesPath",
                message: "The list-categories command requires a project root or Jsons/WhenItFails directory path.",
                path: "list-categories <path> [--plain]");
            return 1;
        }

        if (!TryParseOptions(args, out bool usePlainOutput))
        {
            CommandInputError.Show(
                code: "InvalidListCategoriesArguments",
                message: "The list-categories command accepts only a path and the optional --plain switch.",
                path: "list-categories <path> [--plain]");
            return 1;
        }

        string inputPath = args[1];
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

        if (usePlainOutput)
        {
            CategoriesView.ShowPlain(summary);
        }
        else
        {
            CategoriesView.Show(summary);
        }

        return 0;
    }

    public static bool TryParseOptions(string[] args, out bool usePlainOutput)
    {
        return PlainOutputOptionParser.TryParse(args, optionStartIndex: 2, out usePlainOutput);
    }
}
