using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'profile-remove-category' command.
/// </summary>
internal static class ProfileRemoveCategoryCommand
{
    private const string Usage =
        "profile-remove-category <path> <profile-name> <category-name|alias> [--json]";

    /// <summary>
    /// Executes the profile-remove-category command.
    /// </summary>
    /// <param name="args">The full command-line arguments.</param>
    /// <returns>Exit code: 0 on success, 1 on invalid command input, 2 on edit failure.</returns>
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
        string profileName = args[2];

        WhenItFailsProfileWorkspaceEditor editor = new();
        Response<ErrorProfileDefinition> response =
            await editor.ProfileRemoveCategoryAsync(
                inputPath,
                profileName,
                categoryName);

        if (!response.IsSuccess || response.Data is null)
        {
            if (useJsonOutput)
            {
                ShowJsonFailure(response);
            }
            else
            {
                ShowFailure(response, inputPath, profileName);
            }

            return 2;
        }

        string canonicalCategoryName = ExtractCanonicalValue(response.Message, categoryName);

        if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "profile-remove-category",
                new ProfileRemoveCategoryResult(
                    Updated: true,
                    Profile: response.Data,
                    RemovedCategory: canonicalCategoryName,
                    FailureCode: null,
                    FailureMessage: null));
        }
        else
        {
            AnsiConsole.MarkupLine(
                "[green]Updated profile:[/] {0}",
                Markup.Escape(response.Data.Name));
            AnsiConsole.MarkupLine(
                "[bold]Removed category:[/] {0}",
                Markup.Escape(canonicalCategoryName));

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
            code: "InvalidProfileRemoveCategoryArguments",
            message: "The profile-remove-category command requires a path, profile name, category name or alias, and an optional --json switch.",
            path: Usage);
    }

    private static void ShowJsonFailure(Response<ErrorProfileDefinition> response)
    {
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "ProfileRemoveCategoryFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The category could not be removed from the profile."
            : response.Message;

        CommandJsonOutput.Write(
            "profile-remove-category",
            new ProfileRemoveCategoryResult(
                Updated: false,
                Profile: null,
                RemovedCategory: null,
                FailureCode: failureCode,
                FailureMessage: failureMessage));
    }

    private static string ExtractCanonicalValue(string? message, string fallback)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            int firstQuote = message.IndexOf('\'');
            if (firstQuote >= 0)
            {
                int secondQuote = message.IndexOf('\'', firstQuote + 1);
                if (secondQuote > firstQuote + 1)
                {
                    return message[(firstQuote + 1)..secondQuote];
                }
            }
        }

        return fallback.Trim();
    }

    private static void ShowFailure(
        Response<ErrorProfileDefinition> response,
        string inputPath,
        string profileName)
    {
        ErrorCatalogValidationResult result = new();
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "ProfileRemoveCategoryFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The category could not be removed from the profile."
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

    private sealed record ProfileRemoveCategoryResult(
        bool Updated,
        ErrorProfileDefinition? Profile,
        string? RemovedCategory,
        string? FailureCode,
        string? FailureMessage);
}
