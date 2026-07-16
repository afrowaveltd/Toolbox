using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'profile-remove-owner' command.
/// </summary>
internal static class ProfileRemoveOwnerCommand
{
    private const string Usage =
        "profile-remove-owner <path> <profile-name> <owner-name|alias> [--json]";

    /// <summary>
    /// Executes the profile-remove-owner command.
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
            await editor.ProfileRemoveOwnerAsync(
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

        string canonicalOwnerName = ResolveCanonicalOwnerName(response.Message, ownerName);

        if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "profile-remove-owner",
                new ProfileRemoveOwnerResult(
                    Updated: true,
                    Profile: response.Data,
                    RemovedOwner: canonicalOwnerName,
                    FailureCode: null,
                    FailureMessage: null));
        }
        else
        {
            AnsiConsole.MarkupLine(
                "[green]Updated profile:[/] {0}",
                Markup.Escape(response.Data.Name));
            AnsiConsole.MarkupLine(
                "[bold]Removed owner:[/] {0}",
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

    private static string ResolveCanonicalOwnerName(string? message, string ownerName)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            int firstQuote = message.IndexOf('\'', StringComparison.Ordinal);
            if (firstQuote >= 0)
            {
                int secondQuote = message.IndexOf('\'', firstQuote + 1);
                if (secondQuote > firstQuote + 1)
                {
                    return message[(firstQuote + 1)..secondQuote];
                }
            }
        }

        return ownerName.Trim();
    }

    private static void ShowInvalidArguments()
    {
        CommandInputError.Show(
            code: "InvalidProfileRemoveOwnerArguments",
            message: "The profile-remove-owner command requires a path, profile name, owner name or alias, and an optional --json switch.",
            path: Usage);
    }

    private static void ShowJsonFailure(Response<ErrorProfileDefinition> response)
    {
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "ProfileRemoveOwnerFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The owner could not be removed from the profile."
            : response.Message;

        CommandJsonOutput.Write(
            "profile-remove-owner",
            new ProfileRemoveOwnerResult(
                Updated: false,
                Profile: null,
                RemovedOwner: null,
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
            : "ProfileRemoveOwnerFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The owner could not be removed from the profile."
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

    private sealed record ProfileRemoveOwnerResult(
        bool Updated,
        ErrorProfileDefinition? Profile,
        string? RemovedOwner,
        string? FailureCode,
        string? FailureMessage);
}
