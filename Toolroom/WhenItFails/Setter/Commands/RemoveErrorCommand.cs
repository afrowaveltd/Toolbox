using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Editing;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'remove-error' command.
/// </summary>
internal static class RemoveErrorCommand
{
    private const string Usage = "remove-error <path> <id|code|name> [--json]";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length is < 3 or > 4
            || string.IsNullOrWhiteSpace(args[1])
            || string.IsNullOrWhiteSpace(args[2])
            || (args.Length == 4
                && !string.Equals(args[3], "--json", StringComparison.OrdinalIgnoreCase)))
        {
            CommandInputError.Show(
                "InvalidRemoveErrorArguments",
                "The remove-error command requires a project root or Jsons/WhenItFails path, one error id, code, or name, and an optional --json switch.",
                Usage);
            return 1;
        }

        string inputPath = args[1];
        string lookupValue = args[2];
        bool useJsonOutput = args.Length == 4;
        Response<ErrorDefinition> response =
            await new WhenItFailsWorkspaceEditor().RemoveErrorAsync(inputPath, lookupValue);

        if (!response.IsSuccess || response.Data is null)
        {
            if (useJsonOutput)
            {
                await ShowJsonFailureAsync(response, inputPath, lookupValue);
            }
            else
            {
                ShowFailure(response, inputPath, lookupValue);
            }

            return 2;
        }

        ErrorDefinition error = response.Data;
        if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "remove-error",
                new RemoveErrorResult(
                    Removed: true,
                    Error: error,
                    FailureCode: null,
                    FailureMessage: null,
                    References: []));
        }
        else
        {
            AnsiConsole.MarkupLine("[green]Error definition removed[/]");
            AnsiConsole.MarkupLine("[bold]Id:[/] {0}", Markup.Escape(error.Id));
            AnsiConsole.MarkupLine("[bold]Code:[/] {0}", error.Code);
            AnsiConsole.MarkupLine("[bold]Name:[/] {0}", Markup.Escape(error.Name));
        }

        return 0;
    }

    private static async Task ShowJsonFailureAsync(
        Response<ErrorDefinition> response,
        string inputPath,
        string lookupValue)
    {
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "RemoveErrorFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The error definition could not be removed."
            : response.Message;
        IReadOnlyList<ErrorProfileReference> references = [];

        if (string.Equals(
                failureCode,
                "ErrorIsReferencedByProfiles",
                StringComparison.OrdinalIgnoreCase))
        {
            Response<ErrorReferenceReport> referenceResponse =
                await new WhenItFailsErrorReferenceFinder().FindAsync(inputPath, lookupValue);
            if (referenceResponse.IsSuccess && referenceResponse.Data is not null)
            {
                references = referenceResponse.Data.References;
            }
        }

        CommandJsonOutput.Write(
            "remove-error",
            new RemoveErrorResult(
                Removed: false,
                Error: null,
                FailureCode: failureCode,
                FailureMessage: failureMessage,
                References: references));
    }

    private static void ShowFailure(
        Response<ErrorDefinition> response,
        string inputPath,
        string lookupValue)
    {
        ErrorCatalogValidationResult result = new();
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "RemoveErrorFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "The error definition could not be removed."
            : response.Message;

        result.AddError(failureCode, failureMessage, lookupValue);
        new ConsoleValidationResultShow().Show(
            result,
            new ConsoleShowOptions { SourcePath = inputPath });
    }

    private sealed record RemoveErrorResult(
        bool Removed,
        ErrorDefinition? Error,
        string? FailureCode,
        string? FailureMessage,
        IReadOnlyList<ErrorProfileReference> References);
}
