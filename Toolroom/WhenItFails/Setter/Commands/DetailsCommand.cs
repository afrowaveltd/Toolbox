using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Validation;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Views;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'details' (and 'detail') command: shows one error definition in detail.
/// </summary>
internal static class DetailsCommand
{
    private const string Usage =
        "details <path> <id|code|name> [--plain|--json]";

    /// <summary>
    /// Executes the details command.
    /// </summary>
    /// <param name="args">The full command-line arguments (args[0] is "details" or "detail").</param>
    /// <returns>Exit code: 0 on success, 1 on invalid input or not found, 2 on validation errors.</returns>
    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 3
            || string.IsNullOrWhiteSpace(args[1])
            || string.IsNullOrWhiteSpace(args[2]))
        {
            ErrorCatalogValidationResult missingDetailsArgumentsResult = new();

            missingDetailsArgumentsResult.AddError(
                code: "MissingDetailsArguments",
                message: "The details command requires a project root or Jsons/WhenItFails directory path and an error id, code or name.",
                path: Usage);

            new ConsoleValidationResultShow().Show(
                missingDetailsArgumentsResult,
                new ConsoleShowOptions
                {
                    SourcePath = "command line"
                });

            return 1;
        }

        if (!TryParseOptions(args, out bool usePlainOutput, out bool useJsonOutput))
        {
            CommandInputError.Show(
                code: "InvalidDetailsOutputArguments",
                message: "The --plain and --json switches are mutually exclusive and may be specified only once.",
                path: Usage);
            return 1;
        }

        string inputPath = args[1];
        string lookupValue = args[2];

        WhenItFailsWorkspaceValidationOutcome validationOutcome =
            await new WhenItFailsWorkspaceValidator().ValidateAsync(inputPath);

        if (!validationOutcome.ValidationResult.IsValid)
        {
            if (useJsonOutput)
            {
                CommandJsonOutput.Write(
                    "details",
                    new DetailsResult(
                        Found: false,
                        Error: null,
                        FailureCode: null,
                        FailureMessage: null,
                        Validation: validationOutcome.ValidationResult));
            }
            else
            {
                new ConsoleValidationResultShow().Show(
                    validationOutcome.ValidationResult,
                    new ConsoleShowOptions
                    {
                        SourcePath = validationOutcome.DisplayPath
                    });
            }

            return 2;
        }

        WhenItFailsWorkspaceSummary summary =
            await new WhenItFailsWorkspaceSummarizer().LoadAsync(inputPath);

        ErrorDefinition? errorDefinition = ErrorsCommand.FindErrorDefinition(
            summary,
            lookupValue);

        if (errorDefinition is null)
        {
            const string failureCode = "ErrorDefinitionNotFound";
            string failureMessage =
                $"No error definition was found for '{lookupValue}'. Search by Id, Code or Name.";

            if (useJsonOutput)
            {
                CommandJsonOutput.Write(
                    "details",
                    new DetailsResult(
                        Found: false,
                        Error: null,
                        FailureCode: failureCode,
                        FailureMessage: failureMessage,
                        Validation: null));
            }
            else
            {
                ErrorCatalogValidationResult notFoundResult = new();
                notFoundResult.AddError(
                    code: failureCode,
                    message: failureMessage,
                    path: lookupValue);

                new ConsoleValidationResultShow().Show(
                    notFoundResult,
                    new ConsoleShowOptions
                    {
                        SourcePath = summary.DisplayPath
                    });
            }

            return 1;
        }

        if (usePlainOutput)
        {
            DetailsView.ShowPlain(summary, errorDefinition);
        }
        else if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "details",
                new DetailsResult(
                    Found: true,
                    Error: errorDefinition,
                    FailureCode: null,
                    FailureMessage: null,
                    Validation: null));
        }
        else
        {
            DetailsView.Show(summary, errorDefinition);
        }

        return 0;
    }

    public static bool TryParseOptions(
        string[] args,
        out bool usePlainOutput,
        out bool useJsonOutput)
    {
        usePlainOutput = false;
        useJsonOutput = false;

        for (int index = 3; index < args.Length; index++)
        {
            if (string.Equals(args[index], "--plain", StringComparison.OrdinalIgnoreCase))
            {
                if (usePlainOutput || useJsonOutput)
                {
                    return false;
                }

                usePlainOutput = true;
                continue;
            }

            if (string.Equals(args[index], "--json", StringComparison.OrdinalIgnoreCase))
            {
                if (useJsonOutput || usePlainOutput)
                {
                    return false;
                }

                useJsonOutput = true;
                continue;
            }

            return false;
        }

        return true;
    }

    public static bool TryParseOptions(string[] args, out bool usePlainOutput)
    {
        return TryParseOptions(args, out usePlainOutput, out _);
    }

    private sealed record DetailsResult(
        bool Found,
        ErrorDefinition? Error,
        string? FailureCode,
        string? FailureMessage,
        ErrorCatalogValidationResult? Validation);
}
