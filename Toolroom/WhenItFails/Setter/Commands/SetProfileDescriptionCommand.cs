using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'set-profile-description' command.
/// </summary>
internal static class SetProfileDescriptionCommand
{
    private const string Usage =
        "set-profile-description <path> <profile-name> <description>";

    /// <summary>
    /// Executes the set-profile-description command.
    /// </summary>
    /// <param name="args">The full command-line arguments.</param>
    /// <returns>Exit code: 0 on success, 1 on invalid command input, 2 on edit failure.</returns>
    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                code: "MissingSetProfileDescriptionPath",
                message: "The set-profile-description command requires a project root or Jsons/WhenItFails directory path.",
                path: Usage);

            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            CommandInputError.Show(
                code: "MissingSetProfileDescriptionProfileName",
                message: "The set-profile-description command requires a profile name.",
                path: Usage);

            return 1;
        }

        if (args.Length < 4)
        {
            CommandInputError.Show(
                code: "MissingSetProfileDescriptionValue",
                message: "The set-profile-description command requires a description argument. Pass an empty quoted string to clear it.",
                path: Usage);

            return 1;
        }

        if (args.Length > 4)
        {
            CommandInputError.Show(
                code: "InvalidSetProfileDescriptionArguments",
                message: "The set-profile-description command accepts only a path, profile name, and description. Quote values containing spaces.",
                path: Usage);

            return 1;
        }

        string inputPath = args[1];
        string profileName = args[2];
        string description = args[3];

        WhenItFailsProfileWorkspaceEditor editor = new();
        Response<ErrorProfileDefinition> response =
            await editor.SetProfileDescriptionAsync(
                inputPath,
                profileName,
                description);

        if (!response.IsSuccess || response.Data is null)
        {
            ShowFailure(response, inputPath, profileName);
            return 2;
        }

        AnsiConsole.MarkupLine(
            "[green]Updated profile:[/] {0}",
            Markup.Escape(response.Data.Name));

        if (string.IsNullOrWhiteSpace(response.Data.Description))
        {
            AnsiConsole.MarkupLine("[bold]Description:[/] [grey]<empty>[/]");
        }
        else
        {
            AnsiConsole.MarkupLine(
                "[bold]Description:[/] {0}",
                Markup.Escape(response.Data.Description));
        }

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
            : "SetProfileDescriptionFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The profile description could not be changed."
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
