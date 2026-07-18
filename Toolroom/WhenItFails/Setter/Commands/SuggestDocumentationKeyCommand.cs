using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;
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
        JsonsOptions options = WhenItFailsWorkspacePathResolver.ResolveJsonsOptions(inputPath);

        Response<ErrorCategoryCatalogDocument> categoryResponse =
            await new JsonErrorCategoryCatalogLoader().LoadFromFileAsync(
                options.CategoryCatalogFilePath);
        if (!categoryResponse.IsSuccess || categoryResponse.Data is null)
        {
            ShowFailure(
                categoryResponse,
                inputPath,
                categoryLookup,
                "CategoryCatalogLoadFailed",
                "The category catalog could not be loaded.");
            return 2;
        }

        ErrorCategoryCatalogDocument categories =
            new ErrorCategoryCatalogDocumentNormalizer().Normalize(categoryResponse.Data);
        string normalizedLookup = TextKeyNormalizer.NormalizeKey(categoryLookup);
        ErrorCategoryDefinition? category = categories.Categories.FirstOrDefault(candidate =>
            string.Equals(candidate.Name, normalizedLookup, StringComparison.OrdinalIgnoreCase)
            || candidate.Aliases.Any(alias => string.Equals(
                alias,
                normalizedLookup,
                StringComparison.OrdinalIgnoreCase)));
        if (category is null)
        {
            ErrorCatalogValidationResult result = new();
            result.AddError(
                "CategoryNotFound",
                $"Category '{normalizedLookup}' was not found.",
                categoryLookup);
            new ConsoleValidationResultShow().Show(
                result,
                new ConsoleShowOptions { SourcePath = inputPath });
            return 2;
        }

        Response<ErrorCatalogDocument> errorResponse =
            await new JsonErrorCatalogLoader().LoadFromFileAsync(
                options.ErrorCatalogFilePath);
        if (!errorResponse.IsSuccess || errorResponse.Data is null)
        {
            ShowFailure(
                errorResponse,
                inputPath,
                title,
                "ErrorCatalogLoadFailed",
                "The error catalog could not be loaded.");
            return 2;
        }

        ErrorCatalogDocument errors =
            new ErrorCatalogDocumentNormalizer().Normalize(errorResponse.Data);

        string documentationKey;
        try
        {
            documentationKey = new WhenItFailsDocumentationKeyGenerator().Generate(
                category.Name,
                title,
                errors.Errors.Select(error => error.DocumentationKey));
        }
        catch (ArgumentException exception)
        {
            ErrorCatalogValidationResult result = new();
            result.AddError(
                "DocumentationKeyGenerationFailed",
                exception.Message,
                title);
            new ConsoleValidationResultShow().Show(
                result,
                new ConsoleShowOptions { SourcePath = inputPath });
            return 2;
        }

        SuggestDocumentationKeyResult suggestion = new(
            category.Name,
            title.Trim(),
            documentationKey);

        if (usePlainOutput)
        {
            AnsiConsole.WriteLine(documentationKey);
            return 0;
        }

        if (useJsonOutput)
        {
            CommandJsonOutput.Write("suggest-doc-key", suggestion);
            return 0;
        }

        AnsiConsole.MarkupLine("[green]Suggested documentation key[/]");
        AnsiConsole.MarkupLine("[bold]Category:[/] {0}", Markup.Escape(category.Name));
        AnsiConsole.MarkupLine("[bold]Title:[/] {0}", Markup.Escape(title.Trim()));
        AnsiConsole.MarkupLine("[bold]Documentation key:[/] {0}", Markup.Escape(documentationKey));
        return 0;
    }

    private static void ShowInvalidOutputArguments()
    {
        CommandInputError.Show(
            "InvalidSuggestDocumentationKeyOutputArguments",
            "The --plain and --json switches are mutually exclusive and may be specified only once.",
            Usage);
    }

    private static void ShowFailure<T>(
        Response<T> response,
        string inputPath,
        string lookup,
        string fallbackCode,
        string fallbackMessage)
    {
        ErrorCatalogValidationResult result = new();
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : fallbackCode;
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? fallbackMessage
            : response.Message;

        result.AddError(failureCode, failureMessage, lookup);
        new ConsoleValidationResultShow().Show(
            result,
            new ConsoleShowOptions { SourcePath = inputPath });
    }
}

/// <summary>
/// Describes one read-only documentation key suggestion.
/// </summary>
internal sealed record SuggestDocumentationKeyResult(
    string Category,
    string Title,
    string DocumentationKey);
