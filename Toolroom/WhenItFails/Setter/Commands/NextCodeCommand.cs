using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'next-code' command.
/// </summary>
internal static class NextCodeCommand
{
    private const string Usage =
        "next-code <path> <owner-name|alias> <group-name|prefix> [--plain]";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                "MissingNextCodePath",
                "The next-code command requires a project root or Jsons/WhenItFails directory path.",
                Usage);
            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            CommandInputError.Show(
                "MissingNextCodeOwner",
                "The next-code command requires an owner name or alias.",
                Usage);
            return 1;
        }

        if (args.Length < 4 || string.IsNullOrWhiteSpace(args[3]))
        {
            CommandInputError.Show(
                "MissingNextCodeGroup",
                "The next-code command requires a code group name or prefix.",
                Usage);
            return 1;
        }

        bool plain = args.Length == 5
            && string.Equals(args[4], "--plain", StringComparison.OrdinalIgnoreCase);

        if (args.Length > 5 || (args.Length == 5 && !plain))
        {
            CommandInputError.Show(
                "InvalidNextCodeArguments",
                "The next-code command accepts only a path, owner, code group, and optional --plain flag.",
                Usage);
            return 1;
        }

        string inputPath = args[1];
        string ownerName = args[2];
        string codeGroupName = args[3];

        Response<NextCodeSuggestion> response =
            await new WhenItFailsNextCodeFinder().FindAsync(
                inputPath,
                ownerName,
                codeGroupName);

        if (!response.IsSuccess || response.Data is null)
        {
            ShowFailure(response, inputPath, $"{ownerName}/{codeGroupName}");
            return 2;
        }

        NextCodeSuggestion suggestion = response.Data;
        if (plain)
        {
            AnsiConsole.WriteLine($"{suggestion.Code}\t{suggestion.Id}");
            return 0;
        }

        AnsiConsole.MarkupLine("[green]Next available error identity[/]");
        AnsiConsole.MarkupLine("[bold]Owner:[/] {0}", Markup.Escape(suggestion.Owner));
        AnsiConsole.MarkupLine("[bold]Code group:[/] {0}", Markup.Escape(suggestion.CodeGroup));
        AnsiConsole.MarkupLine("[bold]Code prefix:[/] {0}", Markup.Escape(suggestion.CodePrefix));
        AnsiConsole.MarkupLine("[bold]Numeric code:[/] {0}", suggestion.Code);
        AnsiConsole.MarkupLine("[bold]Structured id:[/] {0}", Markup.Escape(suggestion.Id));
        AnsiConsole.MarkupLine(
            "[grey]Shared range: {0}-{1}; sequence: {2}[/]",
            suggestion.RangeFrom,
            suggestion.RangeTo,
            suggestion.Sequence);

        return 0;
    }

    private static void ShowFailure(
        Response<NextCodeSuggestion> response,
        string inputPath,
        string lookup)
    {
        ErrorCatalogValidationResult result = new();
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "NextCodeFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The next available error code could not be determined."
            : response.Message;

        result.AddError(failureCode, failureMessage, lookup);
        new ConsoleValidationResultShow().Show(
            result,
            new ConsoleShowOptions { SourcePath = inputPath });
    }
}
