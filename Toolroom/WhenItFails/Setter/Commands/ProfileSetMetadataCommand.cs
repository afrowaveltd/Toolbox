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
        "profile-set-metadata <path> <profile-name> <metadata-key> <metadata-value>";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show("MissingProfileSetMetadataPath", "The profile-set-metadata command requires a project root or Jsons/WhenItFails directory path.", Usage);
            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            CommandInputError.Show("MissingProfileSetMetadataProfileName", "The profile-set-metadata command requires a profile name.", Usage);
            return 1;
        }

        if (args.Length < 4 || string.IsNullOrWhiteSpace(args[3]))
        {
            CommandInputError.Show("MissingProfileSetMetadataKey", "The profile-set-metadata command requires a metadata key.", Usage);
            return 1;
        }

        if (args.Length < 5 || string.IsNullOrWhiteSpace(args[4]))
        {
            CommandInputError.Show("MissingProfileSetMetadataValue", "The profile-set-metadata command requires a metadata value.", Usage);
            return 1;
        }

        if (args.Length > 5)
        {
            CommandInputError.Show("InvalidProfileSetMetadataArguments", "The profile-set-metadata command accepts only a path, profile name, metadata key, and metadata value. Quote values containing spaces.", Usage);
            return 1;
        }

        string inputPath = args[1];
        string profileName = args[2];
        string metadataKey = args[3];
        string metadataValue = args[4];

        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().ProfileSetMetadataAsync(
                inputPath,
                profileName,
                metadataKey,
                metadataValue);

        if (!response.IsSuccess || response.Data is null)
        {
            ShowFailure(response, inputPath, profileName);
            return 2;
        }

        string canonicalKey = TextKeyNormalizer.NormalizeKey(metadataKey);
        response.Data.Metadata.TryGet(canonicalKey, out string? savedValue);

        AnsiConsole.MarkupLine("[green]Updated profile:[/] {0}", Markup.Escape(response.Data.Name));
        AnsiConsole.MarkupLine("[bold]Metadata:[/] {0} = {1}", Markup.Escape(canonicalKey), Markup.Escape(savedValue ?? string.Empty));

        if (!string.IsNullOrWhiteSpace(response.Message))
        {
            AnsiConsole.MarkupLine("[grey]{0}[/]", Markup.Escape(response.Message));
        }

        return 0;
    }

    private static void ShowFailure(Response<ErrorProfileDefinition> response, string inputPath, string profileName)
    {
        ErrorCatalogValidationResult result = new();
        string failureCode = response.Issues.Count > 0 ? response.Issues[0].Code : "ProfileSetMetadataFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The metadata could not be set on the profile."
            : response.Message;

        result.AddError(failureCode, failureMessage, profileName);
        new ConsoleValidationResultShow().Show(result, new ConsoleShowOptions { SourcePath = inputPath });
    }
}
