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
        "set-profile-description <path> <profile-name> <description> [--json]";

    /// <summary>
    /// Executes the set-profile-description command.
    /// </summary>
    /// <param name="args">The full command-line arguments.</param>
    /// <returns>Exit code: 0 on success, 1 on invalid command input, 2 on edit failure.</returns>
    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (!TryParseArguments(
                args,
                out string inputPath,
                out string profileName,
                out string description,
                out bool useJsonOutput))
        {
            ShowInvalidArguments();
            return 1;
        }

        WhenItFailsProfileWorkspaceEditor editor = new();
        Response<ErrorProfileDefinition> response =
            await editor.SetProfileDescriptionAsync(
                inputPath,
                profileName,
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
                "set-profile-description",
                new SetProfileDescriptionResult(
                    Updated: true,
                    Profile: response.Data,
                    Description: response.Data.Description,
                    FailureCode: null,
                    FailureMessage: null));
        }
        else
        {
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
        }

        return 0;
    }

    private static bool TryParseArguments(
        string[] args,
        out string inputPath,
        out string profileName,
        out string description,
        out bool useJsonOutput)
    {
        inputPath = string.Empty;
        profileName = string.Empty;
        description = string.Empty;
        useJsonOutput = false;

        if (args.Length is < 4 or > 5
            || string.IsNullOrWhiteSpace(args[1])
            || string.IsNullOrWhiteSpace(args[2]))
        {
            return false;
        }

        inputPath = args[1];
        profileName = args[2];
        bool descriptionAssigned = false;

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

        return descriptionAssigned;
    }

    private static void ShowInvalidArguments() => CommandInputError.Show(
        code: "InvalidSetProfileDescriptionArguments",
        message: "The set-profile-description command requires a path, profile name, description, and an optional --json switch. Pass an empty quoted string to clear the description.",
        path: Usage);

    private static void ShowJsonFailure(Response<ErrorProfileDefinition> response)
    {
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "SetProfileDescriptionFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The profile description could not be changed."
            : response.Message;

        CommandJsonOutput.Write(
            "set-profile-description",
            new SetProfileDescriptionResult(
                Updated: false,
                Profile: null,
                Description: null,
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

    private sealed record SetProfileDescriptionResult(
        bool Updated,
        ErrorProfileDefinition? Profile,
        string? Description,
        string? FailureCode,
        string? FailureMessage);
}
