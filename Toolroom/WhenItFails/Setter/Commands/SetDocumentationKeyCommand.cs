using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Documentation;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'set-documentation-key' command.
/// </summary>
internal static class SetDocumentationKeyCommand
{
    private const string Usage =
        "set-documentation-key <path> <id|code|name> <documentation-key> [--json]";

    /// <summary>
    /// Executes the set-documentation-key command.
    /// </summary>
    /// <param name="args">The full command-line arguments.</param>
    /// <returns>Exit code: 0 on success, 1 on invalid input, 2 on domain failure.</returns>
    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 4
            || string.IsNullOrWhiteSpace(args[1])
            || string.IsNullOrWhiteSpace(args[2]))
        {
            ShowInvalidArguments();
            return 1;
        }

        List<string> documentationKeyParts = [];
        bool useJsonOutput = false;

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

            documentationKeyParts.Add(args[index]);
        }

        string newDocumentationKey = string.Join(" ", documentationKeyParts);
        if (string.IsNullOrWhiteSpace(newDocumentationKey))
        {
            ShowInvalidArguments();
            return 1;
        }

        if (!DocumentationKeyFormat.IsCanonical(newDocumentationKey))
        {
            ShowInvalidDocumentationKeyFormat();
            return 1;
        }

        string inputPath = args[1];
        string lookupValue = args[2];
        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().SetErrorDocumentationKeyAsync(
                inputPath,
                lookupValue,
                newDocumentationKey);

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

        if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "set-documentation-key",
                new SetDocumentationKeyResult(
                    Updated: true,
                    Error: response.Data,
                    FailureCode: null,
                    FailureMessage: null));
        }
        else
        {
            AnsiConsole.MarkupLine(
                "[green]Updated documentation key:[/] {0}",
                Markup.Escape(response.Data.Id));
            AnsiConsole.MarkupLine(
                "[bold]New documentation key:[/] {0}",
                Markup.Escape(response.Data.DocumentationKey ?? string.Empty));

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
            "InvalidSetDocumentationKeyArguments",
            "The set-documentation-key command requires a path, an error id/code/name, a new documentation key, and an optional --json switch.",
            Usage);
    }

    private static void ShowInvalidDocumentationKeyFormat()
    {
        CommandInputError.Show(
            "InvalidDocumentationKeyFormat",
            "The documentation key must contain at least two slash-separated lowercase kebab-case segments, for example 'when-it-fails/errors/network-unavailable'.",
            Usage);
    }

    private static void ShowJsonFailure(Response<ErrorDefinition> response)
    {
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "SetDocumentationKeyFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "Error documentation key could not be changed."
            : response.Message;

        CommandJsonOutput.Write(
            "set-documentation-key",
            new SetDocumentationKeyResult(
                Updated: false,
                Error: null,
                FailureCode: failureCode,
                FailureMessage: failureMessage));
    }

    private static void ShowFailure(
        Response<ErrorDefinition> response,
        string inputPath,
        string lookupValue)
    {
        ErrorCatalogValidationResult result = new();
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "SetDocumentationKeyFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "Error documentation key could not be changed."
            : response.Message;

        result.AddError(failureCode, failureMessage, lookupValue);
        new ConsoleValidationResultShow().Show(
            result,
            new ConsoleShowOptions { SourcePath = inputPath });
    }

    private sealed record SetDocumentationKeyResult(
        bool Updated,
        ErrorDefinition? Error,
        string? FailureCode,
        string? FailureMessage);
}
