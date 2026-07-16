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
        "add-profile <path> <name> <display-name> [description] [--json]";

    /// <summary>
    /// Executes the add-profile command.
    /// </summary>
    /// <param name="args">The full command-line arguments.</param>
    /// <returns>Exit code: 0 on success, 1 on invalid command input, 2 on edit failure.</returns>
    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (!TryParseArguments(
                args,
                out string inputPath,
                out string profileName,
                out string displayName,
                out string? description,
                out bool useJsonOutput))
        {
            ShowInvalidArguments();
            return 1;
        }

        WhenItFailsProfileWorkspaceEditor editor = new();
        Response<ErrorProfileDefinition> response =
            await editor.AddProfileAsync(
                inputPath,
                profileName,
                displayName,
                description);

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

        if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "add-profile",
                new AddProfileResult(
                    Added: true,
                    Profile: response.Data,
                    FailureCode: null,
                    FailureMessage: null));
        }
        else
        {
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
        }

        return 0;
    }

    private static bool TryParseArguments(
        string[] args,
        out string inputPath,
        out string profileName,
        out string displayName,
        out string? description,
        out bool useJsonOutput)
    {
        inputPath = string.Empty;
        profileName = string.Empty;
        displayName = string.Empty;
        description = null;
        useJsonOutput = false;

        if (args.Length is < 4 or > 6
            || string.IsNullOrWhiteSpace(args[1])
            || string.IsNullOrWhiteSpace(args[2])
            || string.IsNullOrWhiteSpace(args[3]))
        {
            return false;
        }

        inputPath = args[1];
        profileName = args[2];
        displayName = args[3];
        bool descriptionAssigned = false;

        for (int index = 4; index < args.Length; index++)
        {
            if (string.Equals(args[index], "--json", StringComparison.OrdinalIgnoreCase))
            {
                if (useJsonOutput)
                {
                    return false;
                }

                useJsonOutput = true;
            }
            else if (!descriptionAssigned)
            {
                description = args[index];
                descriptionAssigned = true;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    private static void ShowInvalidArguments() => CommandInputError.Show(
        code: "InvalidAddProfileArguments",
        message: "The add-profile command requires a path, profile name, display name, one optional description, and an optional --json switch. Quote values containing spaces.",
        path: Usage);

    private static void ShowJsonFailure(Response<ErrorProfileDefinition> response)
    {
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "AddProfileFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The profile could not be added."
            : response.Message;

        CommandJsonOutput.Write(
            "add-profile",
            new AddProfileResult(
                Added: false,
                Profile: null,
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

    private sealed record AddProfileResult(
        bool Added,
        ErrorProfileDefinition? Profile,
        string? FailureCode,
        string? FailureMessage);
}
