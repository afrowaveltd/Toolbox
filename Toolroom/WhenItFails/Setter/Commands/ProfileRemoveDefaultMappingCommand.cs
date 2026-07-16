using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'profile-remove-default-mapping' command.
/// </summary>
internal static class ProfileRemoveDefaultMappingCommand
{
    private const string Usage =
        "profile-remove-default-mapping <path> <profile-name> <mapping-key> [--json]";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (!TryParseArguments(
                args,
                out string inputPath,
                out string profileName,
                out string mappingKey,
                out bool useJsonOutput))
        {
            ShowInvalidArguments();
            return 1;
        }

        string canonicalMappingKey = TextKeyNormalizer.NormalizeKey(mappingKey);
        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().ProfileRemoveDefaultMappingAsync(
                inputPath,
                profileName,
                mappingKey);

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
                "profile-remove-default-mapping",
                new ProfileRemoveDefaultMappingResult(
                    Updated: true,
                    Profile: response.Data,
                    RemovedMappingKey: canonicalMappingKey,
                    FailureCode: null,
                    FailureMessage: null));
        }
        else
        {
            AnsiConsole.MarkupLine(
                "[green]Updated profile:[/] {0}",
                Markup.Escape(response.Data.Name));
            AnsiConsole.MarkupLine(
                "[bold]Removed default mapping:[/] {0}",
                Markup.Escape(canonicalMappingKey));

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
        out string mappingKey,
        out bool useJsonOutput)
    {
        inputPath = string.Empty;
        profileName = string.Empty;
        mappingKey = string.Empty;
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
            else if (string.IsNullOrWhiteSpace(mappingKey) && !string.IsNullOrWhiteSpace(args[index]))
            {
                mappingKey = args[index];
            }
            else
            {
                return false;
            }
        }

        return !string.IsNullOrWhiteSpace(mappingKey);
    }

    private static void ShowInvalidArguments() => CommandInputError.Show(
        code: "InvalidProfileRemoveDefaultMappingArguments",
        message: "The profile-remove-default-mapping command requires a path, profile name, mapping key, and an optional --json switch.",
        path: Usage);

    private static void ShowJsonFailure(Response<ErrorProfileDefinition> response)
    {
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "ProfileRemoveDefaultMappingFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The default mapping could not be removed from the profile."
            : response.Message;

        CommandJsonOutput.Write(
            "profile-remove-default-mapping",
            new ProfileRemoveDefaultMappingResult(
                Updated: false,
                Profile: null,
                RemovedMappingKey: null,
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
            : "ProfileRemoveDefaultMappingFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The default mapping could not be removed from the profile."
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

    private sealed record ProfileRemoveDefaultMappingResult(
        bool Updated,
        ErrorProfileDefinition? Profile,
        string? RemovedMappingKey,
        string? FailureCode,
        string? FailureMessage);
}
