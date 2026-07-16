using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'profile-remove-code-group' command.
/// </summary>
internal static class ProfileRemoveCodeGroupCommand
{
    private const string Usage =
        "profile-remove-code-group <path> <profile-name> <code-group-name|prefix> [--json]";

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
        string? codeGroupName = null;

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

            if (codeGroupName is not null || string.IsNullOrWhiteSpace(args[index]))
            {
                ShowInvalidArguments();
                return 1;
            }

            codeGroupName = args[index];
        }

        if (string.IsNullOrWhiteSpace(codeGroupName))
        {
            ShowInvalidArguments();
            return 1;
        }

        string inputPath = args[1];
        string profileName = args[2];

        WhenItFailsProfileWorkspaceEditor editor = new();
        Response<ErrorProfileDefinition> response =
            await editor.ProfileRemoveCodeGroupAsync(
                inputPath,
                profileName,
                codeGroupName);

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

        string canonicalCodeGroupName = ExtractCanonicalCodeGroupName(response.Message, codeGroupName);

        if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "profile-remove-code-group",
                new ProfileRemoveCodeGroupResult(
                    Updated: true,
                    Profile: response.Data,
                    RemovedCodeGroup: canonicalCodeGroupName,
                    FailureCode: null,
                    FailureMessage: null));
        }
        else
        {
            AnsiConsole.MarkupLine(
                "[green]Updated profile:[/] {0}",
                Markup.Escape(response.Data.Name));
            AnsiConsole.MarkupLine(
                "[bold]Removed code group:[/] {0}",
                Markup.Escape(canonicalCodeGroupName));

            if (!string.IsNullOrWhiteSpace(response.Message))
            {
                AnsiConsole.MarkupLine(
                    "[grey]{0}[/]",
                    Markup.Escape(response.Message));
            }
        }

        return 0;
    }

    private static string ExtractCanonicalCodeGroupName(string? message, string fallback)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            int firstQuote = message.IndexOf('\'');
            if (firstQuote >= 0)
            {
                int secondQuote = message.IndexOf('\'', firstQuote + 1);
                if (secondQuote > firstQuote + 1)
                {
                    return message[(firstQuote + 1)..secondQuote];
                }
            }
        }

        return fallback.Trim();
    }

    private static void ShowInvalidArguments()
    {
        CommandInputError.Show(
            code: "InvalidProfileRemoveCodeGroupArguments",
            message: "The profile-remove-code-group command requires a path, profile name, code group name or prefix, and an optional --json switch.",
            path: Usage);
    }

    private static void ShowJsonFailure(Response<ErrorProfileDefinition> response)
    {
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "ProfileRemoveCodeGroupFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The code group could not be removed from the profile."
            : response.Message;

        CommandJsonOutput.Write(
            "profile-remove-code-group",
            new ProfileRemoveCodeGroupResult(
                Updated: false,
                Profile: null,
                RemovedCodeGroup: null,
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
            : "ProfileRemoveCodeGroupFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The code group could not be removed from the profile."
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

    private sealed record ProfileRemoveCodeGroupResult(
        bool Updated,
        ErrorProfileDefinition? Profile,
        string? RemovedCodeGroup,
        string? FailureCode,
        string? FailureMessage);
}
