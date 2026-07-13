using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'profile-add-category' command.
/// </summary>
internal static class ProfileAddCategoryCommand
{
    private const string Usage =
        "profile-add-category <path> <profile-name> <category-name|alias>";

    /// <summary>
    /// Executes the profile-add-category command.
    /// </summary>
    /// <param name="args">The full command-line arguments.</param>
    /// <returns>Exit code: 0 on success, 1 on invalid command input, 2 on edit failure.</returns>
    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                code: "MissingProfileAddCategoryPath",
                message: "The profile-add-category command requires a project root or Jsons/WhenItFails directory path.",
                path: Usage);

            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            CommandInputError.Show(
                code: "MissingProfileAddCategoryProfileName",
                message: "The profile-add-category command requires a profile name.",
                path: Usage);

            return 1;
        }

        if (args.Length < 4 || string.IsNullOrWhiteSpace(args[3]))
        {
            CommandInputError.Show(
                code: "MissingProfileAddCategoryCategoryName",
                message: "The profile-add-category command requires a category name or alias.",
                path: Usage);

            return 1;
        }

        if (args.Length > 4)
        {
            CommandInputError.Show(
                code: "InvalidProfileAddCategoryArguments",
                message: "The profile-add-category command accepts only a path, profile name, and category name or alias.",
                path: Usage);

            return 1;
        }

        string inputPath = args[1];
        string profileName = args[2];
        string categoryName = args[3];

        WhenItFailsProfileWorkspaceEditor editor = new();
        Response<ErrorProfileDefinition> response =
            await editor.ProfileAddCategoryAsync(
                inputPath,
                profileName,
                categoryName);

        if (!response.IsSuccess || response.Data is null)
        {
            ShowFailure(response, inputPath, profileName);
            return 2;
        }

        string canonicalCategoryName = response.Data.IncludeCategories.First(category =>
            string.Equals(category, categoryName, StringComparison.OrdinalIgnoreCase)
            || response.Message?.Contains($"'{category}'", StringComparison.Ordinal) == true);

        AnsiConsole.MarkupLine(
            "[green]Updated profile:[/] {0}",
            Markup.Escape(response.Data.Name));
        AnsiConsole.MarkupLine(
            "[bold]Added category:[/] {0}",
            Markup.Escape(canonicalCategoryName));

        if (!string.IsNullOrWhiteSpace(response.Message))
        {
            AnsiConsole.MarkupLine(
                "[grey]{0}[/]",
                Markup.Escape(response.Message));
        }

        return 0;
    }

    private static void ShowFailure(
        Response<ErrorProfileDefinition> response,
        string inputPath,
        string profileName)
    {
        ErrorCatalogValidationResult result = new();
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "ProfileAddCategoryFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The category could not be added to the profile."
            : response.Message;

        result.AddError(
            code: failureCode,
            message: failureMessage,
            path: profileName);

        new ConsoleValidationResultShow().Show(
            result,
            new ConsoleShowOptions
            {
                SourcePath = inputPath
            });
    }
}
