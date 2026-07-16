using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'profile-set-metadata' command.
/// </summary>
internal static class ProfileSetMetadataCommand
{
    private const string Usage =
        "profile-set-metadata <path> <profile-name> <metadata-key> <metadata-value> [--json]";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (!TryParseArguments(
                args,
                out string inputPath,
                out string profileName,
                out string metadataKey,
                out string metadataValue,
                out bool useJsonOutput))
        {
            ShowInvalidArguments();
            return 1;
        }

        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().ProfileSetMetadataAsync(
                inputPath,
                profileName,
                metadataKey,
                metadataValue);

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
        response.Data.Metadata.TryGet(canonicalKey, out string? savedValue);

        if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "profile-set-metadata",
                new ProfileSetMetadataResult(
                    Updated: true,
                    Profile: response.Data,
                    MetadataKey: canonicalKey,
                    MetadataValue: savedValue,
                    FailureCode: null,
                    FailureMessage: null));
        }
        else
        {
            AnsiConsole.MarkupLine(
                "[green]Updated profile:[/] {0}",
                Markup.Escape(response.Data.Name));
            AnsiConsole.MarkupLine(
                "[bold]Metadata:[/] {0} = {1}",
                Markup.Escape(canonicalKey),
                Markup.Escape(savedValue ?? string.Empty));

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
        out string metadataValue,
        out bool useJsonOutput)
    {
        inputPath = string.Empty;
        profileName = string.Empty;
        metadataKey = string.Empty;
        metadataValue = string.Empty;
        useJsonOutput = false;

        if (args.Length is < 5 or > 6
            || string.IsNullOrWhiteSpace(args[1])
            || string.IsNullOrWhiteSpace(args[2]))
        {
            return false;
        }

        inputPath = args[1];
        profileName = args[2];
        List<string> positionalArguments = [];

        for (int index = 3; index < args.Length; index++)
        {
            if (string.Equals(args[index], "--json", StringComparison.OrdinalIgnoreCase))
            {
                if (useJsonOutput)
                {
                    return false;
                }

                useJsonOutput = true;
                continue;
            }

            if (string.IsNullOrWhiteSpace(args[index]))
            {
                return false;
            }

            positionalArguments.Add(args[index]);
        }

        if (positionalArguments.Count != 2)
        {
            return false;
        }

        metadataKey = positionalArguments[0];
        metadataValue = positionalArguments[1];
        return true;
    }

    private static void ShowInvalidArguments() => CommandInputError.Show(
        code: "InvalidProfileSetMetadataArguments",
        message: "The profile-set-metadata command requires a path, profile name, metadata key, metadata value, and an optional --json switch. Quote values containing spaces.",
        path: Usage);

    private static void ShowJsonFailure(Response<ErrorProfileDefinition> response)
    {
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "ProfileSetMetadataFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The metadata could not be set on the profile."
            : response.Message;

        CommandJsonOutput.Write(
            "profile-set-metadata",
            new ProfileSetMetadataResult(
                Updated: false,
                Profile: null,
                MetadataKey: null,
                MetadataValue: null,
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
            : "ProfileSetMetadataFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The metadata could not be set on the profile."
            : response.Message;

        result.AddError(failureCode, failureMessage, profileName);
        new ConsoleValidationResultShow().Show(
            result,
            new ConsoleShowOptions { SourcePath = inputPath });
    }

    private sealed record ProfileSetMetadataResult(
        bool Updated,
        ErrorProfileDefinition? Profile,
        string? MetadataKey,
        string? MetadataValue,
        string? FailureCode,
        string? FailureMessage);
}
