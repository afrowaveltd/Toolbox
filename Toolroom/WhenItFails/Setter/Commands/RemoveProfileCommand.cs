using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'remove-profile' command.
/// </summary>
internal static class RemoveProfileCommand
{
    private const string Usage =
        "remove-profile <path> <name> [--json]";

    /// <summary>
    /// Executes the remove-profile command.
    /// </summary>
    /// <param name="args">The full command-line arguments.</param>
    /// <returns>Exit code: 0 on success, 1 on invalid command input, 2 on edit failure.</returns>
    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (!TryParseArguments(args, out string inputPath, out string profileName, out bool useJsonOutput))
        {
            ShowInvalidArguments();
            return 1;
        }

        WhenItFailsProfileWorkspaceEditor editor = new();
        Response<ErrorProfileDefinition> response =
            await editor.RemoveProfileAsync(
                inputPath,
                profileName);

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
                "remove-profile",
                new RemoveProfileResult(
                    Removed: true,
                    Profile: response.Data,
                    FailureCode: null,
                    FailureMessage: null));
        }
        else
        {
            AnsiConsole.MarkupLine(
                "[green]Removed profile:[/] {0}",
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
        out bool useJsonOutput)
    {
        inputPath = string.Empty;
        profileName = string.Empty;
        useJsonOutput = false;

        if (args.Length is < 3 or > 4
            || string.IsNullOrWhiteSpace(args[1]))
        {
            return false;
        }

        inputPath = args[1];
        bool profileNameAssigned = false;

        for (int index = 2; index < args.Length; index++)
        {
            if (string.Equals(args[index], "--json", StringComparison.OrdinalIgnoreCase))
            {
                if (useJsonOutput)
                {
                    return false;
                }

                useJsonOutput = true;
            }
            else if (!profileNameAssigned && !string.IsNullOrWhiteSpace(args[index]))
            {
                profileName = args[index];
                profileNameAssigned = true;
            }
            else
            {
                return false;
            }
        }

        return profileNameAssigned;
    }

    private static void ShowInvalidArguments() => CommandInputError.Show(
        code: "InvalidRemoveProfileArguments",
        message: "The remove-profile command requires a path, profile name, and an optional --json switch.",
        path: Usage);

    private static void ShowJsonFailure(Response<ErrorProfileDefinition> response)
    {
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "RemoveProfileFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The profile could not be removed."
            : response.Message;

        CommandJsonOutput.Write(
            "remove-profile",
            new RemoveProfileResult(
                Removed: false,
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
            : "RemoveProfileFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The profile could not be removed."
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

    private sealed record RemoveProfileResult(
        bool Removed,
        ErrorProfileDefinition? Profile,
        string? FailureCode,
        string? FailureMessage);
}
