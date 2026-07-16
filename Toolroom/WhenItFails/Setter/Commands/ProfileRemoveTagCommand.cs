using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

internal static class ProfileRemoveTagCommand
{
    private const string Usage =
        "profile-remove-tag <path> <profile-name> <tag> [--json]";

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
        string? tagName = null;

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

            if (tagName is not null || string.IsNullOrWhiteSpace(args[index]))
            {
                ShowInvalidArguments();
                return 1;
            }

            tagName = args[index];
        }

        if (string.IsNullOrWhiteSpace(tagName))
        {
            ShowInvalidArguments();
            return 1;
        }

        string inputPath = args[1];
        string profileName = args[2];

        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().ProfileRemoveTagAsync(
                inputPath,
                profileName,
                tagName);

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

        string canonicalTagName = TextKeyNormalizer.NormalizeKey(tagName);

        if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "profile-remove-tag",
                new ProfileRemoveTagResult(
                    Updated: true,
                    Profile: response.Data,
                    RemovedTag: canonicalTagName,
                    FailureCode: null,
                    FailureMessage: null));
        }
        else
        {
            AnsiConsole.MarkupLine(
                "[green]Updated profile:[/] {0}",
                Markup.Escape(response.Data.Name));
            AnsiConsole.MarkupLine(
                "[bold]Removed tag:[/] {0}",
                Markup.Escape(canonicalTagName));

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
            code: "InvalidProfileRemoveTagArguments",
            message: "The profile-remove-tag command requires a path, profile name, tag, and an optional --json switch.",
            path: Usage);
    }

    private static void ShowJsonFailure(Response<ErrorProfileDefinition> response)
    {
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "ProfileRemoveTagFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The tag could not be removed from the profile."
            : response.Message;

        CommandJsonOutput.Write(
            "profile-remove-tag",
            new ProfileRemoveTagResult(
                Updated: false,
                Profile: null,
                RemovedTag: null,
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
            response.Issues.Count > 0 ? response.Issues[0].Code : "ProfileRemoveTagFailed",
            string.IsNullOrWhiteSpace(response.Message)
                ? "The tag could not be removed from the profile."
                : response.Message,
            path: profileName);

        new ConsoleValidationResultShow().Show(
            result,
            new ConsoleShowOptions
            {
                SourcePath = inputPath
            });
    }

    private sealed record ProfileRemoveTagResult(
        bool Updated,
        ErrorProfileDefinition? Profile,
        string? RemovedTag,
        string? FailureCode,
        string? FailureMessage);
}
