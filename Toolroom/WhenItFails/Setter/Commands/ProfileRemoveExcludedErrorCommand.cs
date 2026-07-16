using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'profile-remove-excluded-error' command.
/// </summary>
internal static class ProfileRemoveExcludedErrorCommand
{
    private const string Usage =
        "profile-remove-excluded-error <path> <profile-name> <id|code|name> [--json]";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (!TryParseArguments(args, out string inputPath, out string profileName, out string errorLookup, out bool useJsonOutput))
        {
            ShowInvalidArguments();
            return 1;
        }

        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().ProfileRemoveExcludedErrorAsync(
                inputPath,
                profileName,
                errorLookup);

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

        string canonicalErrorId = ExtractCanonicalErrorId(response.Message, errorLookup);

        if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "profile-remove-excluded-error",
                new ProfileRemoveExcludedErrorResult(
                    Updated: true,
                    Profile: response.Data,
                    RemovedExcludedError: canonicalErrorId,
                    FailureCode: null,
                    FailureMessage: null));
        }
        else
        {
            AnsiConsole.MarkupLine(
                "[green]Updated profile:[/] {0}",
                Markup.Escape(response.Data.Name));
            AnsiConsole.MarkupLine(
                "[bold]Stopped excluding error:[/] {0}",
                Markup.Escape(canonicalErrorId));

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
        out string errorLookup,
        out bool useJsonOutput)
    {
        inputPath = string.Empty;
        profileName = string.Empty;
        errorLookup = string.Empty;
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
            else if (string.IsNullOrWhiteSpace(errorLookup) && !string.IsNullOrWhiteSpace(args[index]))
            {
                errorLookup = args[index];
            }
            else
            {
                return false;
            }
        }

        return !string.IsNullOrWhiteSpace(errorLookup);
    }

    private static string ExtractCanonicalErrorId(string? message, string fallback)
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

    private static void ShowInvalidArguments() => CommandInputError.Show(
        code: "InvalidProfileRemoveExcludedErrorArguments",
        message: "The profile-remove-excluded-error command requires a path, profile name, error id, code, or name, and an optional --json switch.",
        path: Usage);

    private static void ShowJsonFailure(Response<ErrorProfileDefinition> response)
    {
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "ProfileRemoveExcludedErrorFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The error could not be removed from the profile's excluded errors."
            : response.Message;

        CommandJsonOutput.Write(
            "profile-remove-excluded-error",
            new ProfileRemoveExcludedErrorResult(
                Updated: false,
                Profile: null,
                RemovedExcludedError: null,
                FailureCode: failureCode,
                FailureMessage: failureMessage));
    }

    private static void ShowFailure(
        Response<ErrorProfileDefinition> response,
        string inputPath,
        string profileName)
    {
        ErrorCatalogValidationResult result = new();
        result.AddError(
            code: response.Issues.Count > 0 ? response.Issues[0].Code : "ProfileRemoveExcludedErrorFailed",
            message: string.IsNullOrWhiteSpace(response.Message)
                ? "The error could not be removed from the profile's excluded errors."
                : response.Message,
            path: profileName);

        new ConsoleValidationResultShow().Show(
            result,
            new ConsoleShowOptions { SourcePath = inputPath });
    }

    private sealed record ProfileRemoveExcludedErrorResult(
        bool Updated,
        ErrorProfileDefinition? Profile,
        string? RemovedExcludedError,
        string? FailureCode,
        string? FailureMessage);
}
