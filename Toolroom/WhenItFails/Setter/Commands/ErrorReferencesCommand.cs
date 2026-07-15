using System.Globalization;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'error-references' command.
/// </summary>
internal static class ErrorReferencesCommand
{
    private const string Usage =
        "error-references <path> <id|code|name> [--plain|--json]";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                "MissingErrorReferencesPath",
                "The error-references command requires a project root or Jsons/WhenItFails directory path.",
                Usage);
            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            CommandInputError.Show(
                "MissingErrorReferencesLookup",
                "The error-references command requires an error id, code, or name.",
                Usage);
            return 1;
        }

        bool usePlainOutput = false;
        bool useJsonOutput = false;
        for (int index = 3; index < args.Length; index++)
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
                "InvalidErrorReferencesArguments",
                $"Unknown error-references argument '{args[index]}'.",
                Usage);
            return 1;
        }

        string inputPath = args[1];
        string lookupValue = args[2];
        Response<ErrorReferenceReport> response =
            await new WhenItFailsErrorReferenceFinder().FindAsync(inputPath, lookupValue);

        if (!response.IsSuccess || response.Data is null)
        {
            ShowFailure(response, inputPath, lookupValue);
            return 2;
        }

        if (usePlainOutput)
        {
            ShowPlain(response.Data);
        }
        else if (useJsonOutput)
        {
            CommandJsonOutput.Write("error-references", response.Data);
        }
        else
        {
            Show(response.Data);
        }

        return 0;
    }

    private static void Show(ErrorReferenceReport report)
    {
        AnsiConsole.MarkupLine(
            "[bold]Error:[/] {0} [grey]({1}, {2})[/]",
            Markup.Escape(report.ErrorId),
            report.ErrorCode,
            Markup.Escape(report.ErrorName));
        AnsiConsole.MarkupLine(
            "[bold]Included by:[/] {0}  [bold]Excluded by:[/] {1}",
            report.IncludedByProfiles,
            report.ExcludedByProfiles);

        if (report.References.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No explicit profile references.[/]");
            return;
        }

        Table table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Kind")
            .AddColumn("Profile")
            .AddColumn("Display name");

        foreach (ErrorProfileReference reference in report.References)
        {
            table.AddRow(
                reference.ReferenceKind == "Include"
                    ? "[green]Include[/]"
                    : "[red]Exclude[/]",
                Markup.Escape(reference.ProfileName),
                Markup.Escape(reference.DisplayName));
        }

        AnsiConsole.Write(table);
    }

    private static void ShowPlain(ErrorReferenceReport report)
    {
        if (report.References.Count == 0)
        {
            Console.WriteLine(string.Join(
                '\t',
                report.ErrorCode.ToString(CultureInfo.InvariantCulture),
                report.ErrorId,
                report.ErrorName,
                "NONE",
                string.Empty,
                string.Empty));
            return;
        }

        foreach (ErrorProfileReference reference in report.References)
        {
            Console.WriteLine(string.Join(
                '\t',
                report.ErrorCode.ToString(CultureInfo.InvariantCulture),
                report.ErrorId,
                report.ErrorName,
                reference.ReferenceKind.ToUpperInvariant(),
                reference.ProfileName,
                reference.DisplayName));
        }
    }

    private static void ShowInvalidOutputArguments()
    {
        CommandInputError.Show(
            "InvalidErrorReferencesOutputArguments",
            "The --plain and --json switches are mutually exclusive and may be specified only once.",
            Usage);
    }

    private static void ShowFailure(
        Response<ErrorReferenceReport> response,
        string inputPath,
        string lookupValue)
    {
        ErrorCatalogValidationResult result = new();
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "ErrorReferencesFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "Error profile references could not be inspected."
            : response.Message;

        result.AddError(failureCode, failureMessage, lookupValue);
        new ConsoleValidationResultShow().Show(
            result,
            new ConsoleShowOptions { SourcePath = inputPath });
    }
}
