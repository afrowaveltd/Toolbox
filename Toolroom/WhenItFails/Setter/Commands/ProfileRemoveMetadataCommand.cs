using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'profile-remove-metadata' command.
/// </summary>
internal static class ProfileRemoveMetadataCommand
{
    private const string Usage =
        "profile-remove-metadata <path> <profile-name> <metadata-key> [--json]";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (!TryParseArguments(
                args,
                out string inputPath,
                out string profileName,
                out string metadataKey,
                out bool useJsonOutput))
        {
            ShowInvalidArguments();
            return 1;
        }

        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().ProfileRemoveMetadataAsync(
                inputPath,
                profileName,
                metadataKey);

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

        string canonicalKey = TextKeyNormalizer.NormalizeKey(metadataKey);

        if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "profile-remove-metadata",
                new ProfileRemoveMetadataResult(
                    Updated: true,
                    Profile: response.Data,
                    RemovedMetadataKey: canonicalKey,
                    FailureCode: null,
                    FailureMessage: null));
        }
        else
        {
            AnsiConsole.MarkupLine(
                "[green]Updated profile:[/] {0}",
                Markup.Escape(response.Data.Name));
            AnsiConsole.MarkupLine(
                "[bold]Removed metadata:[/] {0}",
                Markup.Escape(canonicalKey));

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
        out string metadataKey,
        out bool useJsonOutput)
    {
        inputPath = string.Empty;
        profileName = string.Empty;
        metadataKey = string.Empty;
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
            else if (string.IsNullOrWhiteSpace(metadataKey)
                     && !string.IsNullOrWhiteSpace(args[index]))
            {
                metadataKey = args[index];
            }
            else
            {
                return false;
            }
        }

        return !string.IsNullOrWhiteSpace(metadataKey);
    }

    private static void ShowInvalidArguments() => CommandInputError.Show(
        code: "InvalidProfileRemoveMetadataArguments",
        message: "The profile-remove-metadata command requires a path, profile name, metadata key, and an optional --json switch.",
        path: Usage);

    private static void ShowJsonFailure(Response<ErrorProfileDefinition> response)
    {
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "ProfileRemoveMetadataFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The metadata could not be removed from the profile."
            : response.Message;

        CommandJsonOutput.Write(
            "profile-remove-metadata",
            new ProfileRemoveMetadataResult(
                Updated: false,
                Profile: null,
                RemovedMetadataKey: null,
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
            : "ProfileRemoveMetadataFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The metadata could not be removed from the profile."
            : response.Message;

        result.AddError(failureCode, failureMessage, profileName);
        new ConsoleValidationResultShow().Show(
            result,
            new ConsoleShowOptions { SourcePath = inputPath });
    }

    private sealed record ProfileRemoveMetadataResult(
        bool Updated,
        ErrorProfileDefinition? Profile,
        string? RemovedMetadataKey,
        string? FailureCode,
        string? FailureMessage);
}
