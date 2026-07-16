using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'profile-add-owner' command.
/// </summary>
internal static class ProfileAddOwnerCommand
{
    private const string Usage =
        "profile-add-owner <path> <profile-name> <owner-name|alias> [--json]";

    /// <summary>
    /// Executes the profile-add-owner command.
    /// </summary>
    /// <param name="args">The full command-line arguments.</param>
    /// <returns>Exit code: 0 on success, 1 on invalid command input, 2 on edit failure.</returns>
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
        string? ownerName = null;

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

            if (ownerName is not null || string.IsNullOrWhiteSpace(args[index]))
            {
                ShowInvalidArguments();
                return 1;
            }

            ownerName = args[index];
        }

        if (string.IsNullOrWhiteSpace(ownerName))
        {
            ShowInvalidArguments();
            return 1;
        }

        string inputPath = args[1];
        string profileName = args[2];

        WhenItFailsProfileWorkspaceEditor editor = new();
        Response<ErrorProfileDefinition> response =
            await editor.ProfileAddOwnerAsync(
                inputPath,
                profileName,
                ownerName);

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

        string canonicalOwnerName = response.Data.IncludeOwners.First(owner =>
            string.Equals(owner, ownerName, StringComparison.OrdinalIgnoreCase)
            || response.Message?.Contains($"'{owner}'", StringComparison.Ordinal) == true);

        if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "profile-add-owner",
                new ProfileAddOwnerResult(
                    Updated: true,
                    Profile: response.Data,
                    AddedOwner: canonicalOwnerName,
                    FailureCode: null,
                    FailureMessage: null));
        }
        else
        {
            AnsiConsole.MarkupLine(
                "[green]Updated profile:[/] {0}",
                Markup.Escape(response.Data.Name));
            AnsiConsole.MarkupLine(
                "[bold]Added owner:[/] {0}",
                Markup.Escape(canonicalOwnerName));

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
            code: "InvalidProfileAddOwnerArguments",
            message: "The profile-add-owner command requires a path, profile name, owner name or alias, and an optional --json switch.",
            path: Usage);
    }

    private static void ShowJsonFailure(Response<ErrorProfileDefinition> response)
    {
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "ProfileAddOwnerFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The owner could not be added to the profile."
            : response.Message;

        CommandJsonOutput.Write(
            "profile-add-owner",
            new ProfileAddOwnerResult(
                Updated: false,
                Profile: null,
                AddedOwner: null,
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
            : "ProfileAddOwnerFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The owner could not be added to the profile."
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

    private sealed record ProfileAddOwnerResult(
        bool Updated,
        ErrorProfileDefinition? Profile,
        string? AddedOwner,
        string? FailureCode,
        string? FailureMessage);
}
