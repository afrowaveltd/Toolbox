using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'profile-remove-error' command.
/// </summary>
internal static class ProfileRemoveErrorCommand
{
    private const string Usage =
        "profile-remove-error <path> <profile-name> <id|code|name> [--json]";

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
        string? errorLookup = null;

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

            if (errorLookup is not null || string.IsNullOrWhiteSpace(args[index]))
            {
                ShowInvalidArguments();
                return 1;
            }

            errorLookup = args[index];
        }

        if (string.IsNullOrWhiteSpace(errorLookup))
        {
            ShowInvalidArguments();
            return 1;
        }

        string inputPath = args[1];
        string profileName = args[2];

        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().ProfileRemoveErrorAsync(
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
                "profile-remove-error",
                new ProfileRemoveErrorResult(
                    Updated: true,
                    Profile: response.Data,
                    RemovedError: canonicalErrorId,
                    FailureCode: null,
                    FailureMessage: null));
        }
        else
        {
            AnsiConsole.MarkupLine(
                "[green]Updated profile:[/] {0}",
                Markup.Escape(response.Data.Name));
            AnsiConsole.MarkupLine(
                "[bold]Removed error:[/] {0}",
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

        return TextKeyNormalizer.NormalizeKey(fallback);
    }

    private static void ShowInvalidArguments()
    {
        CommandInputError.Show(
            code: "InvalidProfileRemoveErrorArguments",
            message: "The profile-remove-error command requires a path, profile name, error id, code, or name, and an optional --json switch.",
            path: Usage);
    }

    private static void ShowJsonFailure(Response<ErrorProfileDefinition> response)
    {
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "ProfileRemoveErrorFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The error could not be removed from the profile."
            : response.Message;

        CommandJsonOutput.Write(
            "profile-remove-error",
            new ProfileRemoveErrorResult(
                Updated: false,
                Profile: null,
                RemovedError: null,
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
            : "ProfileRemoveErrorFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The error could not be removed from the profile."
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

    private sealed record ProfileRemoveErrorResult(
        bool Updated,
        ErrorProfileDefinition? Profile,
        string? RemovedError,
        string? FailureCode,
        string? FailureMessage);
}
