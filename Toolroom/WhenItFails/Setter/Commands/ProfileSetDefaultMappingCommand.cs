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
        "profile-set-default-mapping <path> <profile-name> <mapping-key> <mapping-value>";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                code: "MissingProfileSetDefaultMappingPath",
                message: "The profile-set-default-mapping command requires a project root or Jsons/WhenItFails directory path.",
                path: Usage);
            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            CommandInputError.Show(
                code: "MissingProfileSetDefaultMappingProfileName",
                message: "The profile-set-default-mapping command requires a profile name.",
                path: Usage);
            return 1;
        }

        if (args.Length < 4 || string.IsNullOrWhiteSpace(args[3]))
        {
            CommandInputError.Show(
                code: "MissingProfileSetDefaultMappingKey",
                message: "The profile-set-default-mapping command requires a mapping key.",
                path: Usage);
            return 1;
        }

        if (args.Length < 5 || string.IsNullOrWhiteSpace(args[4]))
        {
            CommandInputError.Show(
                code: "MissingProfileSetDefaultMappingValue",
                message: "The profile-set-default-mapping command requires a mapping value.",
                path: Usage);
            return 1;
        }

        if (args.Length > 5)
        {
            CommandInputError.Show(
                code: "InvalidProfileSetDefaultMappingArguments",
                message: "The profile-set-default-mapping command accepts only a path, profile name, mapping key, and mapping value. Quote values containing spaces.",
                path: Usage);
            return 1;
        }

        string inputPath = args[1];
        string profileName = args[2];
        string mappingKey = args[3];
        string mappingValue = args[4];

        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().ProfileSetDefaultMappingAsync(
                inputPath,
                profileName,
                mappingKey,
                mappingValue);

        if (!response.IsSuccess || response.Data is null)
        {
            ShowFailure(response, inputPath, profileName);
            return 2;
        }

        string canonicalMappingKey = TextKeyNormalizer.NormalizeKey(mappingKey);

        AnsiConsole.MarkupLine(
            "[green]Updated profile:[/] {0}",
            Markup.Escape(response.Data.Name));
        AnsiConsole.MarkupLine(
            "[bold]Default mapping:[/] {0} = {1}",
            Markup.Escape(canonicalMappingKey),
            Markup.Escape(response.Data.DefaultMappings[canonicalMappingKey]));

        if (!string.IsNullOrWhiteSpace(response.Message))
        {
            AnsiConsole.MarkupLine(
                "[grey]{0}[/]",
                Markup.Escape(response.Message));
        }

        return 0;
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
}
