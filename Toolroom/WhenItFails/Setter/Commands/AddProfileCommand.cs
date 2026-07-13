using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'add-profile' command.
/// </summary>
internal static class AddProfileCommand
{
    private const string Usage =
        "add-profile <path> <name> <display-name> [description]";

    /// <summary>
    /// Executes the add-profile command.
    /// </summary>
    /// <param name="args">The full command-line arguments.</param>
    /// <returns>Exit code: 0 on success, 1 on invalid command input, 2 on edit failure.</returns>
    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                code: "MissingAddProfilePath",
                message: "The add-profile command requires a project root or Jsons/WhenItFails directory path.",
                path: Usage);

            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            CommandInputError.Show(
                code: "MissingAddProfileName",
                message: "The add-profile command requires a stable profile name.",
                path: Usage);

            return 1;
        }

        if (args.Length < 4 || string.IsNullOrWhiteSpace(args[3]))
        {
            CommandInputError.Show(
                code: "MissingAddProfileDisplayName",
                message: "The add-profile command requires a human-readable display name.",
                path: Usage);

            return 1;
        }

        if (args.Length > 5)
        {
            CommandInputError.Show(
                code: "InvalidAddProfileArguments",
                message: "The add-profile command accepts a path, profile name, display name, and one optional description argument. Quote values containing spaces.",
                path: Usage);

            return 1;
        }

        string inputPath = args[1];
        string profileName = args[2];
        string displayName = args[3];
        string? description = args.Length == 5
            ? args[4]
            : null;

        WhenItFailsProfileWorkspaceEditor editor = new();
        Response<ErrorProfileDefinition> response =
            await editor.AddProfileAsync(
                inputPath,
                profileName,
                displayName,
                description);

        if (!response.IsSuccess || response.Data is null)
        {
            ShowFailure(response, inputPath, profileName);
            return 2;
        }

        AnsiConsole.MarkupLine(
            "[green]Added profile:[/] {0}",
            Markup.Escape(response.Data.Name));
        AnsiConsole.MarkupLine(
            "[bold]Display name:[/] {0}",
            Markup.Escape(response.Data.DisplayName));

        if (!string.IsNullOrWhiteSpace(response.Data.Description))
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
            : "AddProfileFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The profile could not be added."
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
