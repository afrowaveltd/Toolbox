using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'profile-add-excluded-error' command.
/// </summary>
internal static class ProfileAddExcludedErrorCommand
{
    private const string Usage =
        "profile-add-excluded-error <path> <profile-name> <id|code|name>";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                code: "MissingProfileAddExcludedErrorPath",
                message: "The profile-add-excluded-error command requires a project root or Jsons/WhenItFails directory path.",
                path: Usage);
            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            CommandInputError.Show(
                code: "MissingProfileAddExcludedErrorProfileName",
                message: "The profile-add-excluded-error command requires a profile name.",
                path: Usage);
            return 1;
        }

        if (args.Length < 4 || string.IsNullOrWhiteSpace(args[3]))
        {
            CommandInputError.Show(
                code: "MissingProfileAddExcludedErrorLookup",
                message: "The profile-add-excluded-error command requires an error id, code, or name.",
                path: Usage);
            return 1;
        }

        if (args.Length > 4)
        {
            CommandInputError.Show(
                code: "InvalidProfileAddExcludedErrorArguments",
                message: "The profile-add-excluded-error command accepts only a path, profile name, and error id, code, or name.",
                path: Usage);
            return 1;
        }

        string inputPath = args[1];
        string profileName = args[2];
        string errorLookup = args[3];

        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().ProfileAddExcludedErrorAsync(
                inputPath,
                profileName,
                errorLookup);

        if (!response.IsSuccess || response.Data is null)
        {
            ShowFailure(response, inputPath, profileName);
            return 2;
        }

        string canonicalErrorId = response.Data.ExcludeErrors.First(value =>
            string.Equals(value, errorLookup, StringComparison.OrdinalIgnoreCase)
            || response.Message.Contains(value, StringComparison.OrdinalIgnoreCase));

        AnsiConsole.MarkupLine(
            "[green]Updated profile:[/] {0}",
            Markup.Escape(response.Data.Name));
        AnsiConsole.MarkupLine(
            "[bold]Excluded error:[/] {0}",
            Markup.Escape(canonicalErrorId));

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
            : "ProfileAddExcludedErrorFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The error could not be added to the profile's excluded errors."
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
