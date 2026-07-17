using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'list-categories' command: lists categories from a validated WhenItFails workspace.
/// </summary>
internal static class ListCategoriesCommand
{
    private const string Usage = "list-categories <path> [--plain|--json]";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                code: "MissingListCategoriesPath",
                message: "The list-categories command requires a project root or Jsons/WhenItFails directory path.",
                path: Usage);
            return 1;
        }

        if (!TryParseOptions(args, out bool usePlainOutput, out bool useJsonOutput))
        {
            CommandInputError.Show(
                code: "InvalidListCategoriesOutputArguments",
                message: "The --plain and --json switches are mutually exclusive and may be specified only once.",
                path: Usage);
            return 1;
        }

        string inputPath = args[1];
        WhenItFailsWorkspaceValidationOutcome validationOutcome =
            await new WhenItFailsWorkspaceValidator().ValidateAsync(inputPath);

        if (!validationOutcome.ValidationResult.IsValid)
        {
            if (useJsonOutput)
            {
                CommandJsonOutput.Write(
                    "list-categories",
                    new ListCategoriesResult(
                        Loaded: false,
                        Categories: null,
                        Validation: validationOutcome.ValidationResult));
            }
            else
            {
                ShowValidationFailure(validationOutcome);
            }

            return 2;
        }

        WhenItFailsWorkspaceSummary summary =
            await new WhenItFailsWorkspaceSummarizer().LoadAsync(inputPath);
        IReadOnlyList<ErrorCategoryDefinition> categories = summary.CategoryCatalog.Categories;

        if (usePlainOutput)
        {
            CategoriesView.ShowPlain(summary);
        }
        else if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "list-categories",
                new ListCategoriesResult(
                    Loaded: true,
                    Categories: categories,
                    Validation: null));
        }
        else
        {
            CategoriesView.Show(summary);
        }

        return 0;
    }

    public static bool TryParseOptions(
        string[] args,
        out bool usePlainOutput,
        out bool useJsonOutput)
    {
        usePlainOutput = false;
        useJsonOutput = false;

        for (int index = 2; index < args.Length; index++)
        {
            if (string.Equals(args[index], "--plain", StringComparison.OrdinalIgnoreCase))
            {
                if (usePlainOutput || useJsonOutput)
                {
                    return false;
                }

                usePlainOutput = true;
                continue;
            }

            if (string.Equals(args[index], "--json", StringComparison.OrdinalIgnoreCase))
            {
                if (useJsonOutput || usePlainOutput)
                {
                    return false;
                }

                useJsonOutput = true;
                continue;
            }

            return false;
        }

        return true;
    }

    public static bool TryParseOptions(string[] args, out bool usePlainOutput)
    {
        return TryParseOptions(args, out usePlainOutput, out _);
    }

    private static void ShowValidationFailure(
        WhenItFailsWorkspaceValidationOutcome validationOutcome)
    {
        new ConsoleValidationResultShow().Show(
            validationOutcome.ValidationResult,
            new ConsoleShowOptions
            {
                SourcePath = validationOutcome.DisplayPath
            });
    }

    private sealed record ListCategoriesResult(
        bool Loaded,
        IReadOnlyList<ErrorCategoryDefinition>? Categories,
        ErrorCatalogValidationResult? Validation);
}
