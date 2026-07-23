using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'check-doc-keys' command.
/// </summary>
internal static class CheckDocKeysCommand
{
    private const string Usage = "check-doc-keys <path> [--plain|--json]";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                "MissingCheckDocKeysPath",
                "The check-doc-keys command requires a project root or Jsons/WhenItFails directory path.",
                Usage);
            return 1;
        }

        bool usePlainOutput = false;
        bool useJsonOutput = false;

        for (int index = 2; index < args.Length; index++)
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
                "UnknownCheckDocKeysOption",
                $"Unknown check-doc-keys option '{args[index]}'.",
                Usage);
            return 1;
        }

        string inputPath = args[1];
        DocumentationKeyCommandReport report;

        try
        {
            WhenItFailsWorkspaceSummary workspace =
                await new WhenItFailsWorkspaceSummarizer().LoadAsync(inputPath);
            DocumentationKeyCheckReport keyReport =
                new WhenItFailsDocumentationKeyChecker().Check(workspace.ErrorCatalog);
            DocumentationKeyFormatCheckReport formatReport =
                new WhenItFailsDocumentationKeyFormatChecker().Check(workspace.ErrorCatalog);
            report = new DocumentationKeyCommandReport(keyReport, formatReport);
        }
        catch (Exception exception)
        {
            ErrorCatalogValidationResult validation = CreateFailure(inputPath, exception.Message);

            if (useJsonOutput)
            {
                CommandJsonOutput.Write(
                    "check-doc-keys",
                    new DocumentationKeyCommandFailure(
                        Checked: false,
                        Validation: validation));
            }
            else
            {
                ShowFailure(inputPath, validation);
            }

            return 2;
        }

        if (useJsonOutput)
        {
            CommandJsonOutput.Write("check-doc-keys", report);
        }
        else
        {
            ShowReport(report, usePlainOutput);
        }

        return report.IsValid ? 0 : 2;
    }

    private static void ShowReport(
        DocumentationKeyCommandReport report,
        bool usePlainOutput)
    {
        if (usePlainOutput)
        {
            foreach (DocumentationKeyIssue issue in report.Keys.MissingKeys)
            {
                AnsiConsole.WriteLine(string.Join(
                    '\t',
                    "missing",
                    issue.ErrorCode,
                    issue.ErrorId,
                    issue.ErrorName));
            }

            foreach (DuplicateDocumentationKey duplicate in report.Keys.DuplicateKeys)
            {
                foreach (DocumentationKeyIssue issue in duplicate.Errors)
                {
                    AnsiConsole.WriteLine(string.Join(
                        '\t',
                        "duplicate",
                        duplicate.DocumentationKey,
                        issue.ErrorCode,
                        issue.ErrorId,
                        issue.ErrorName));
                }
            }

            foreach (InvalidDocumentationKeyFormat issue in report.Format.InvalidKeys)
            {
                AnsiConsole.WriteLine(string.Join(
                    '\t',
                    "invalid-format",
                    issue.DocumentationKey,
                    issue.ErrorCode,
                    issue.ErrorId,
                    issue.ErrorName));
            }

            return;
        }

        AnsiConsole.MarkupLine("[bold]Errors checked:[/] {0}", report.TotalErrors);

        if (report.IsValid)
        {
            AnsiConsole.MarkupLine(
                "[green]All errors have unique, non-empty, canonical documentation keys.[/]");
            return;
        }

        if (report.Keys.MissingKeys.Count > 0)
        {
            Table missingTable = new Table()
                .Border(TableBorder.Rounded)
                .Expand();
            missingTable.AddColumn("Code");
            missingTable.AddColumn("Id");
            missingTable.AddColumn("Name");

            foreach (DocumentationKeyIssue issue in report.Keys.MissingKeys)
            {
                missingTable.AddRow(
                    issue.ErrorCode.ToString(),
                    Markup.Escape(issue.ErrorId),
                    Markup.Escape(issue.ErrorName));
            }

            AnsiConsole.MarkupLine(
                "[red]Missing documentation keys:[/] {0}",
                report.Keys.MissingKeys.Count);
            AnsiConsole.Write(missingTable);
        }

        if (report.Keys.DuplicateKeys.Count > 0)
        {
            Table duplicateTable = new Table()
                .Border(TableBorder.Rounded)
                .Expand();
            duplicateTable.AddColumn("Documentation key");
            duplicateTable.AddColumn("Code");
            duplicateTable.AddColumn("Id");
            duplicateTable.AddColumn("Name");

            foreach (DuplicateDocumentationKey duplicate in report.Keys.DuplicateKeys)
            {
                foreach (DocumentationKeyIssue issue in duplicate.Errors)
                {
                    duplicateTable.AddRow(
                        Markup.Escape(duplicate.DocumentationKey),
                        issue.ErrorCode.ToString(),
                        Markup.Escape(issue.ErrorId),
                        Markup.Escape(issue.ErrorName));
                }
            }

            AnsiConsole.MarkupLine(
                "[red]Duplicate documentation keys:[/] {0}",
                report.Keys.DuplicateKeys.Count);
            AnsiConsole.Write(duplicateTable);
        }

        if (report.Format.InvalidKeys.Count > 0)
        {
            Table formatTable = new Table()
                .Border(TableBorder.Rounded)
                .Expand();
            formatTable.AddColumn("Documentation key");
            formatTable.AddColumn("Code");
            formatTable.AddColumn("Id");
            formatTable.AddColumn("Name");

            foreach (InvalidDocumentationKeyFormat issue in report.Format.InvalidKeys)
            {
                formatTable.AddRow(
                    Markup.Escape(issue.DocumentationKey),
                    issue.ErrorCode.ToString(),
                    Markup.Escape(issue.ErrorId),
                    Markup.Escape(issue.ErrorName));
            }

            AnsiConsole.MarkupLine(
                "[red]Non-canonical documentation keys:[/] {0}",
                report.Format.InvalidKeys.Count);
            AnsiConsole.Write(formatTable);
        }
    }

    private static void ShowInvalidOutputArguments()
    {
        CommandInputError.Show(
            "InvalidCheckDocKeysOutputArguments",
            "The --plain and --json switches are mutually exclusive and may be specified only once.",
            Usage);
    }

    private static ErrorCatalogValidationResult CreateFailure(string inputPath, string message)
    {
        ErrorCatalogValidationResult result = new();
        result.AddError(
            "CheckDocKeysFailed",
            string.IsNullOrWhiteSpace(message)
                ? "Documentation keys could not be checked."
                : message,
            inputPath);
        return result;
    }

    private static void ShowFailure(
        string inputPath,
        ErrorCatalogValidationResult result)
    {
        new ConsoleValidationResultShow().Show(
            result,
            new ConsoleShowOptions { SourcePath = inputPath });
    }
}

internal sealed record DocumentationKeyCommandReport(
    DocumentationKeyCheckReport Keys,
    DocumentationKeyFormatCheckReport Format)
{
    public int TotalErrors => Keys.TotalErrors;

    public bool IsValid => Keys.IsValid && Format.IsValid;
}

internal sealed record DocumentationKeyCommandFailure(
    bool Checked,
    ErrorCatalogValidationResult Validation);
