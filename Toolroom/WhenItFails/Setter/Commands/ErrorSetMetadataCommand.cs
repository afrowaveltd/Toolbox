using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Normalization;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

internal static class ErrorSetMetadataCommand
{
    private const string Usage =
        "error-set-metadata <path> <id|code|name> <metadata-key> <metadata-value>";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show("MissingErrorSetMetadataPath", "The error-set-metadata command requires a project root or Jsons/WhenItFails directory path.", Usage);
            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            CommandInputError.Show("MissingErrorSetMetadataLookup", "The error-set-metadata command requires an error id, code, or name.", Usage);
            return 1;
        }

        if (args.Length < 4 || string.IsNullOrWhiteSpace(args[3]))
        {
            CommandInputError.Show("MissingErrorSetMetadataKey", "The error-set-metadata command requires a metadata key.", Usage);
            return 1;
        }

        if (args.Length < 5 || string.IsNullOrWhiteSpace(args[4]))
        {
            CommandInputError.Show("MissingErrorSetMetadataValue", "The error-set-metadata command requires a metadata value.", Usage);
            return 1;
        }

        if (args.Length > 5)
        {
            CommandInputError.Show("InvalidErrorSetMetadataArguments", "The error-set-metadata command accepts only a path, error lookup, metadata key, and metadata value. Quote values containing spaces.", Usage);
            return 1;
        }

        string inputPath = args[1];
        string lookupValue = args[2];
        string metadataKey = args[3];
        string metadataValue = args[4];

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorSetMetadataAsync(
                inputPath,
                lookupValue,
                metadataKey,
                metadataValue);

        if (!response.IsSuccess || response.Data is null)
        {
            ShowFailure(response, inputPath, lookupValue);
            return 2;
        }

        string canonicalKey = TextKeyNormalizer.NormalizeKey(metadataKey);
        response.Data.Metadata.TryGet(canonicalKey, out string? savedValue);

        AnsiConsole.MarkupLine("[green]Updated error:[/] {0}", Markup.Escape(response.Data.Id));
        AnsiConsole.MarkupLine("[bold]Metadata:[/] {0} = {1}", Markup.Escape(canonicalKey), Markup.Escape(savedValue ?? string.Empty));

        if (!string.IsNullOrWhiteSpace(response.Message))
        {
            AnsiConsole.MarkupLine("[grey]{0}[/]", Markup.Escape(response.Message));
        }

        return 0;
    }

    private static void ShowFailure(Response<ErrorDefinition> response, string inputPath, string lookupValue)
    {
        ErrorCatalogValidationResult result = new();
        string failureCode = response.Issues.Count > 0 ? response.Issues[0].Code : "ErrorSetMetadataFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The metadata could not be set on the error definition."
            : response.Message;

        result.AddError(failureCode, failureMessage, path: lookupValue);
        new ConsoleValidationResultShow().Show(result, new ConsoleShowOptions { SourcePath = inputPath });
    }
}
