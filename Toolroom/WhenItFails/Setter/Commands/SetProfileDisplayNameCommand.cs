using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'set-profile-display-name' command.
/// </summary>
internal static class SetProfileDisplayNameCommand
{
    private const string Usage =
        "set-profile-display-name <path> <profile-name> <display-name> [--json]";

    /// <summary>
    /// Executes the set-profile-display-name command.
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
                out bool useJsonOutput))
        {
            ShowInvalidArguments();
            return 1;
        }

        WhenItFailsProfileWorkspaceEditor editor = new();
        Response<ErrorProfileDefinition> response =
            await editor.SetProfileDisplayNameAsync(
                inputPath,
                profileName,
                displayName);

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
                "set-profile-display-name",
                new SetProfileDisplayNameResult(
                    Updated: true,
                    Profile: response.Data,
                    DisplayName: response.Data.DisplayName,
                    FailureCode: null,
                    FailureMessage: null));
        }
        else
        {
            AnsiConsole.MarkupLine(
                "[green]Updated profile:[/] {0}",
                Markup.Escape(response.Data.Name));
            AnsiConsole.MarkupLine(
                "[bold]Display name:[/] {0}",
                Markup.Escape(response.Data.DisplayName));

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
        out bool useJsonOutput)
    {
        inputPath = string.Empty;
        profileName = string.Empty;
        displayName = string.Empty;
        useJsonOutput = false;

        if (args.Length is < 4 or > 5
            || string.IsNullOrWhiteSpace(args[1])
            || string.IsNullOrWhiteSpace(args[2]))
        {
            return false;
        }

        inputPath = args[1];
        profileName = args[2];

        for (int index = 3; index < args.Length; index++)
        {
            if (string.Equals(args[index], "--json", StringComparison.OrdinalIgnoreCase))
            {
                if (useJsonOutput)
                {
                    return false;
                }

                useJsonOutput = true;
            }
            else if (string.IsNullOrWhiteSpace(displayName)
                     && !string.IsNullOrWhiteSpace(args[index]))
            {
                displayName = args[index];
            }
            else
            {
                return false;
            }
        }

        return !string.IsNullOrWhiteSpace(displayName);
    }

    private static void ShowInvalidArguments() => CommandInputError.Show(
        code: "InvalidSetProfileDisplayNameArguments",
        message: "The set-profile-display-name command requires a path, profile name, new display name, and an optional --json switch. Quote values containing spaces.",
        path: Usage);

    private static void ShowJsonFailure(Response<ErrorProfileDefinition> response)
    {
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "SetProfileDisplayNameFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The profile display name could not be changed."
            : response.Message;

        CommandJsonOutput.Write(
            "set-profile-display-name",
            new SetProfileDisplayNameResult(
                Updated: false,
                Profile: null,
                DisplayName: null,
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
            : "SetProfileDisplayNameFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The profile display name could not be changed."
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

    private sealed record SetProfileDisplayNameResult(
        bool Updated,
        ErrorProfileDefinition? Profile,
        string? DisplayName,
        string? FailureCode,
        string? FailureMessage);
}
