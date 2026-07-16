using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'profile-set-default-mapping' command.
/// </summary>
internal static class ProfileSetDefaultMappingCommand
{
    private const string Usage =
        "profile-set-default-mapping <path> <profile-name> <mapping-key> <mapping-value> [--json]";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (!TryParseArguments(
                args,
                out string inputPath,
                out string profileName,
                out string mappingKey,
                out string mappingValue,
                out bool useJsonOutput))
        {
            ShowInvalidArguments();
            return 1;
        }

        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().ProfileSetDefaultMappingAsync(
                inputPath,
                profileName,
                mappingKey,
                mappingValue);

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

        string canonicalMappingKey = TextKeyNormalizer.NormalizeKey(mappingKey);
        string savedValue = response.Data.DefaultMappings[canonicalMappingKey];

        if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "profile-set-default-mapping",
                new ProfileSetDefaultMappingResult(
                    Updated: true,
                    Profile: response.Data,
                    MappingKey: canonicalMappingKey,
                    MappingValue: savedValue,
                    FailureCode: null,
                    FailureMessage: null));
        }
        else
        {
            AnsiConsole.MarkupLine(
                "[green]Updated profile:[/] {0}",
                Markup.Escape(response.Data.Name));
            AnsiConsole.MarkupLine(
                "[bold]Default mapping:[/] {0} = {1}",
                Markup.Escape(canonicalMappingKey),
                Markup.Escape(savedValue));

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
        out string mappingValue,
        out bool useJsonOutput)
    {
        inputPath = string.Empty;
        profileName = string.Empty;
        mappingKey = string.Empty;
        mappingValue = string.Empty;
        useJsonOutput = false;

        if (args.Length is < 5 or > 6
            || string.IsNullOrWhiteSpace(args[1])
            || string.IsNullOrWhiteSpace(args[2]))
        {
            return false;
        }

        inputPath = args[1];
        profileName = args[2];
        List<string> values = [];

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
            else if (!string.IsNullOrWhiteSpace(args[index]))
            {
                values.Add(args[index]);
            }
            else
            {
                return false;
            }
        }

        if (values.Count != 2)
        {
            return false;
        }

        mappingKey = values[0];
        mappingValue = values[1];
        return true;
    }

    private static void ShowInvalidArguments() => CommandInputError.Show(
        code: "InvalidProfileSetDefaultMappingArguments",
        message: "The profile-set-default-mapping command requires a path, profile name, mapping key, mapping value, and an optional --json switch. Quote values containing spaces.",
        path: Usage);

    private static void ShowJsonFailure(Response<ErrorProfileDefinition> response)
    {
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "ProfileSetDefaultMappingFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The default mapping could not be set on the profile."
            : response.Message;

        CommandJsonOutput.Write(
            "profile-set-default-mapping",
            new ProfileSetDefaultMappingResult(
                Updated: false,
                Profile: null,
                MappingKey: null,
                MappingValue: null,
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
            : "ProfileSetDefaultMappingFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The default mapping could not be set on the profile."
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

    private sealed record ProfileSetDefaultMappingResult(
        bool Updated,
        ErrorProfileDefinition? Profile,
        string? MappingKey,
        string? MappingValue,
        string? FailureCode,
        string? FailureMessage);
}
