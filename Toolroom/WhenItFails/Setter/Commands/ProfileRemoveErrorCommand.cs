using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'profile-remove-error' command.
/// </summary>
internal static class ProfileRemoveErrorCommand
{
    private const string Usage =
        "profile-remove-error <path> <profile-name> <id|code|name>";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                code: "MissingProfileRemoveErrorPath",
                message: "The profile-remove-error command requires a project root or Jsons/WhenItFails directory path.",
                path: Usage);
            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            CommandInputError.Show(
                code: "MissingProfileRemoveErrorProfileName",
                message: "The profile-remove-error command requires a profile name.",
                path: Usage);
            return 1;
        }

        if (args.Length < 4 || string.IsNullOrWhiteSpace(args[3]))
        {
            CommandInputError.Show(
                code: "MissingProfileRemoveErrorLookup",
                message: "The profile-remove-error command requires an error id, code, or name.",
                path: Usage);
            return 1;
        }

        if (args.Length > 4)
        {
            CommandInputError.Show(
                code: "InvalidProfileRemoveErrorArguments",
                message: "The profile-remove-error command accepts only a path, profile name, and error id, code, or name.",
                path: Usage);
            return 1;
        }

        string inputPath = args[1];
        string profileName = args[2];
        string errorLookup = args[3];

        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().ProfileRemoveErrorAsync(
                inputPath,
                profileName,
                errorLookup);

        if (!response.IsSuccess || response.Data is null)
        {
            ShowFailure(response, inputPath, profileName);
            return 2;
        }

        AnsiConsole.MarkupLine(
            "[green]Updated profile:[/] {0}",
            Markup.Escape(response.Data.Name));
        AnsiConsole.MarkupLine(
            "[bold]Removed error lookup:[/] {0}",
            Markup.Escape(TextKeyNormalizer.NormalizeKey(errorLookup)));

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
            : "ProfileRemoveErrorFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The error could not be removed from the profile."
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
