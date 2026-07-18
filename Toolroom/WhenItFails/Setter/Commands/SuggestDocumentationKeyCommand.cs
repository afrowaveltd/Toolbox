using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'suggest-doc-key' command.
/// </summary>
internal static class SuggestDocumentationKeyCommand
{
    private const string Usage =
        "suggest-doc-key <path> <category-name|alias> <title> [--plain|--json]";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                "MissingSuggestDocumentationKeyPath",
                "The suggest-doc-key command requires a project root or Jsons/WhenItFails directory path.",
                Usage);
            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            CommandInputError.Show(
                "MissingSuggestDocumentationKeyCategory",
                "The suggest-doc-key command requires a category name or alias.",
                Usage);
            return 1;
        }

        if (args.Length < 4 || string.IsNullOrWhiteSpace(args[3]))
        {
            CommandInputError.Show(
                "MissingSuggestDocumentationKeyTitle",
                "The suggest-doc-key command requires an error title.",
                Usage);
            return 1;
        }

        bool usePlainOutput = false;
        bool useJsonOutput = false;
        for (int index = 4; index < args.Length; index++)
        {
            if (string.Equals(args[index], "--plain", StringComparison.OrdinalIgnoreCase))
            {
                if (usePlainOutput || useJsonOutput)
                {
                    ShowInvalidOutputArguments();
                    return 1;
                }

                usePlainOutput = true;
                continue;
            }

            if (string.Equals(args[index], "--json", StringComparison.OrdinalIgnoreCase))
            {
                if (useJsonOutput || usePlainOutput)
                {
                    ShowInvalidOutputArguments();
                    return 1;
                }

                useJsonOutput = true;
                continue;
            }

            CommandInputError.Show(
                "InvalidSuggestDocumentationKeyArguments",
                $"Unknown suggest-doc-key argument '{args[index]}'.",
                Usage);
            return 1;
        }

        string inputPath = args[1];
        string categoryLookup = args[2];
        string title = args[3];

        Response<DocumentationKeySuggestion> response =
            await new WhenItFailsDocumentationKeySuggester().SuggestAsync(
                inputPath,
                categoryLookup,
                title);
        if (!response.IsSuccess || response.Data is null)
        {
            if (useJsonOutput)
            {
                ShowJsonFailure(response, categoryLookup, title);
            }
            else
            {
                ShowFailure(response, inputPath, categoryLookup);
            }

            return 2;
        }

        DocumentationKeySuggestion suggestion = response.Data;
        if (usePlainOutput)
        {
            AnsiConsole.WriteLine(suggestion.DocumentationKey);
            return 0;
        }

        if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "suggest-doc-key",
                new SuggestDocumentationKeyResult(
                    suggestion.Category,
                    suggestion.Title,
                    suggestion.DocumentationKey,
                    FailureCode: null,
                    FailureMessage: null));
            return 0;
        }

        AnsiConsole.MarkupLine("[green]Suggested documentation key[/]");
        AnsiConsole.MarkupLine("[bold]Category:[/] {0}", Markup.Escape(suggestion.Category));
        AnsiConsole.MarkupLine("[bold]Title:[/] {0}", Markup.Escape(suggestion.Title));
        AnsiConsole.MarkupLine(
            "[bold]Documentation key:[/] {0}",
            Markup.Escape(suggestion.DocumentationKey));
        return 0;
    }

    private static void ShowInvalidOutputArguments()
    {
        CommandInputError.Show(
            "InvalidSuggestDocumentationKeyOutputArguments",
            "The --plain and --json switches are mutually exclusive and may be specified only once.",
            Usage);
    }

    private static void ShowJsonFailure(
        Response<DocumentationKeySuggestion> response,
        string categoryLookup,
        string title)
    {
        (string failureCode, string failureMessage) = GetFailure(response);
        CommandJsonOutput.Write(
            "suggest-doc-key",
            new SuggestDocumentationKeyResult(
                categoryLookup.Trim(),
                title.Trim(),
                DocumentationKey: null,
                failureCode,
                failureMessage));
    }

    private static void ShowFailure(
        Response<DocumentationKeySuggestion> response,
        string inputPath,
        string lookup)
    {
        (string failureCode, string failureMessage) = GetFailure(response);
        ErrorCatalogValidationResult result = new();
        result.AddError(failureCode, failureMessage, lookup);
        new ConsoleValidationResultShow().Show(
            result,
            new ConsoleShowOptions { SourcePath = inputPath });
    }

    private static (string Code, string Message) GetFailure(
        Response<DocumentationKeySuggestion> response)
    {
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "SuggestDocumentationKeyFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The documentation key could not be suggested."
            : response.Message;
        return (failureCode, failureMessage);
    }

    private sealed record SuggestDocumentationKeyResult(
        string Category,
        string Title,
        string? DocumentationKey,
        string? FailureCode,
        string? FailureMessage);
}
