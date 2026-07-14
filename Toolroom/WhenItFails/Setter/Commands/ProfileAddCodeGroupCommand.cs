using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'profile-add-code-group' command.
/// </summary>
internal static class ProfileAddCodeGroupCommand
{
    private const string Usage =
        "profile-add-code-group <path> <profile-name> <code-group-name|prefix>";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                code: "MissingProfileAddCodeGroupPath",
                message: "The profile-add-code-group command requires a project root or Jsons/WhenItFails directory path.",
                path: Usage);

            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            CommandInputError.Show(
                code: "MissingProfileAddCodeGroupProfileName",
                message: "The profile-add-code-group command requires a profile name.",
                path: Usage);

            return 1;
        }

        if (args.Length < 4 || string.IsNullOrWhiteSpace(args[3]))
        {
            CommandInputError.Show(
                code: "MissingProfileAddCodeGroupName",
                message: "The profile-add-code-group command requires a code group name or prefix.",
                path: Usage);

            return 1;
        }

        if (args.Length > 4)
        {
            CommandInputError.Show(
                code: "InvalidProfileAddCodeGroupArguments",
                message: "The profile-add-code-group command accepts only a path, profile name, and code group name or prefix.",
                path: Usage);

            return 1;
        }

        string inputPath = args[1];
        string profileName = args[2];
        string codeGroupName = args[3];

        WhenItFailsProfileWorkspaceEditor editor = new();
        Response<ErrorProfileDefinition> response =
            await editor.ProfileAddCodeGroupAsync(
                inputPath,
                profileName,
                codeGroupName);

        if (!response.IsSuccess || response.Data is null)
        {
            ShowFailure(response, inputPath, profileName);
            return 2;
        }

        string canonicalCodeGroupName = response.Data.IncludeCodeGroups.First(codeGroup =>
            string.Equals(codeGroup, codeGroupName, StringComparison.OrdinalIgnoreCase)
            || response.Message?.Contains($"'{codeGroup}'", StringComparison.Ordinal) == true);

        AnsiConsole.MarkupLine(
            "[green]Updated profile:[/] {0}",
            Markup.Escape(response.Data.Name));
        AnsiConsole.MarkupLine(
            "[bold]Added code group:[/] {0}",
            Markup.Escape(canonicalCodeGroupName));

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
            : "ProfileAddCodeGroupFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The code group could not be added to the profile."
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
