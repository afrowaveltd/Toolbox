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
        "error-set-metadata <path> <id|code|name> <metadata-key> <metadata-value> [--json]";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 5
            || args.Length > 6
            || string.IsNullOrWhiteSpace(args[1])
            || string.IsNullOrWhiteSpace(args[2]))
        {
            ShowInvalidArguments();
            return 1;
        }

        bool useJsonOutput = false;
        List<string> values = [];

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

            if (string.IsNullOrWhiteSpace(args[index]))
            {
                ShowInvalidArguments();
                return 1;
            }

            values.Add(args[index]);
        }

        if (values.Count != 2)
        {
            ShowInvalidArguments();
            return 1;
        }

        string inputPath = args[1];
        string lookupValue = args[2];
        string metadataKey = values[0];
        string metadataValue = values[1];

        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().ErrorSetMetadataAsync(
                inputPath,
                lookupValue,
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
                ShowFailure(response, inputPath, lookupValue);
            }

            return 2;
        }

        string canonicalKey = TextKeyNormalizer.NormalizeKey(metadataKey);
        response.Data.Metadata.TryGet(canonicalKey, out string? savedValue);

        if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "error-set-metadata",
                new ErrorSetMetadataResult(
                    Updated: true,
                    Error: response.Data,
                    MetadataKey: canonicalKey,
                    MetadataValue: savedValue,
                    FailureCode: null,
                    FailureMessage: null));
        }
        else
        {
            AnsiConsole.MarkupLine("[green]Updated error:[/] {0}", Markup.Escape(response.Data.Id));
            AnsiConsole.MarkupLine("[bold]Metadata:[/] {0} = {1}", Markup.Escape(canonicalKey), Markup.Escape(savedValue ?? string.Empty));

            if (!string.IsNullOrWhiteSpace(response.Message))
            {
                AnsiConsole.MarkupLine("[grey]{0}[/]", Markup.Escape(response.Message));
            }
        }

        return 0;
    }

    private static void ShowInvalidArguments()
    {
        CommandInputError.Show(
            "InvalidErrorSetMetadataArguments",
            "The error-set-metadata command requires a path, error lookup, metadata key, metadata value, and an optional --json switch. Quote values containing spaces.",
            Usage);
    }

    private static void ShowJsonFailure(Response<ErrorDefinition> response)
    {
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "ErrorSetMetadataFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The metadata could not be set on the error definition."
            : response.Message;

        CommandJsonOutput.Write(
            "error-set-metadata",
            new ErrorSetMetadataResult(
                Updated: false,
                Error: null,
                MetadataKey: null,
                MetadataValue: null,
                FailureCode: failureCode,
                FailureMessage: failureMessage));
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

    private sealed record ErrorSetMetadataResult(
        bool Updated,
        ErrorDefinition? Error,
        string? MetadataKey,
        string? MetadataValue,
        string? FailureCode,
        string? FailureMessage);
}
