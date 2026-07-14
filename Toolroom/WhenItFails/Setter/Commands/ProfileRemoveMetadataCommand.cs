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
        "profile-remove-metadata <path> <profile-name> <metadata-key>";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show("MissingProfileRemoveMetadataPath", "The profile-remove-metadata command requires a project root or Jsons/WhenItFails directory path.", Usage);
            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            CommandInputError.Show("MissingProfileRemoveMetadataProfileName", "The profile-remove-metadata command requires a profile name.", Usage);
            return 1;
        }

        if (args.Length < 4 || string.IsNullOrWhiteSpace(args[3]))
        {
            CommandInputError.Show("MissingProfileRemoveMetadataKey", "The profile-remove-metadata command requires a metadata key.", Usage);
            return 1;
        }

        if (args.Length > 4)
        {
            CommandInputError.Show("InvalidProfileRemoveMetadataArguments", "The profile-remove-metadata command accepts only a path, profile name, and metadata key.", Usage);
            return 1;
        }

        string inputPath = args[1];
        string profileName = args[2];
        string metadataKey = args[3];

        Response<ErrorProfileDefinition> response =
            await new WhenItFailsProfileWorkspaceEditor().ProfileRemoveMetadataAsync(
                inputPath,
                profileName,
                metadataKey);

        if (!response.IsSuccess || response.Data is null)
        {
            ShowFailure(response, inputPath, profileName);
            return 2;
        }

        AnsiConsole.MarkupLine("[green]Updated profile:[/] {0}", Markup.Escape(response.Data.Name));
        AnsiConsole.MarkupLine("[bold]Removed metadata:[/] {0}", Markup.Escape(TextKeyNormalizer.NormalizeKey(metadataKey)));

        if (!string.IsNullOrWhiteSpace(response.Message))
        {
            AnsiConsole.MarkupLine("[grey]{0}[/]", Markup.Escape(response.Message));
        }

        return 0;
    }

    private static void ShowFailure(Response<ErrorProfileDefinition> response, string inputPath, string profileName)
    {
        ErrorCatalogValidationResult result = new();
        string failureCode = response.Issues.Count > 0 ? response.Issues[0].Code : "ProfileRemoveMetadataFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The metadata could not be removed from the profile."
            : response.Message;

        result.AddError(failureCode, failureMessage, profileName);
        new ConsoleValidationResultShow().Show(result, new ConsoleShowOptions { SourcePath = inputPath });
    }
}
