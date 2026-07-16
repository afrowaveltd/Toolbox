using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'profile-add-subcategory' command.
/// </summary>
internal static class ProfileAddSubcategoryCommand
{
    private const string Usage =
        "profile-add-subcategory <path> <profile-name> <subcategory> [--json]";

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
        string profileName = args[2];

        WhenItFailsProfileWorkspaceEditor editor = new();
        Response<ErrorProfileDefinition> response =
            await editor.ProfileAddSubcategoryAsync(
                inputPath,
                profileName,
                subcategoryName);

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

        string canonicalSubcategoryName =
            TextKeyNormalizer.NormalizeKey(subcategoryName);

        if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "profile-add-subcategory",
                new ProfileAddSubcategoryResult(
                    Updated: true,
                    Profile: response.Data,
                    AddedSubcategory: canonicalSubcategoryName,
                    FailureCode: null,
                    FailureMessage: null));
        }
        else
        {
            AnsiConsole.MarkupLine(
                "[green]Updated profile:[/] {0}",
                Markup.Escape(response.Data.Name));
            AnsiConsole.MarkupLine(
                "[bold]Added subcategory:[/] {0}",
                Markup.Escape(canonicalSubcategoryName));

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
            code: "InvalidProfileAddSubcategoryArguments",
            message: "The profile-add-subcategory command requires a path, profile name, subcategory, and an optional --json switch.",
            path: Usage);
    }

    private static void ShowJsonFailure(Response<ErrorProfileDefinition> response)
    {
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "ProfileAddSubcategoryFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The subcategory could not be added to the profile."
            : response.Message;

        CommandJsonOutput.Write(
            "profile-add-subcategory",
            new ProfileAddSubcategoryResult(
                Updated: false,
                Profile: null,
                AddedSubcategory: null,
                FailureCode: failureCode,
                FailureMessage: failureMessage));
    }

    private static void ShowFailure(
        Response<ErrorProfileDefinition> response,
        string inputPath,
        string profileName)
    {
        ErrorCatalogValidationResult result = new();
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "ProfileAddSubcategoryFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The subcategory could not be added to the profile."
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

    private sealed record ProfileAddSubcategoryResult(
        bool Updated,
        ErrorProfileDefinition? Profile,
        string? AddedSubcategory,
        string? FailureCode,
        string? FailureMessage);
}
