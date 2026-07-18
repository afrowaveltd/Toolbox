using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'check-doc-links' command.
/// </summary>
internal static class CheckDocLinksCommand
{
    private const string Usage = "check-doc-links <path> [--plain|--json]";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                "MissingCheckDocLinksPath",
                "The check-doc-links command requires a repository root or Setter directory path.",
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
                "UnknownCheckDocLinksOption",
                $"Unknown check-doc-links option '{args[index]}'.",
                Usage);
            return 1;
        }

        string inputPath = args[1];
        Response<DocumentationLinkCheckReport> response =
            await new WhenItFailsDocumentationLinkChecker().CheckAsync(inputPath);

        if (useJsonOutput)
        {
            CommandJsonOutput.Write("check-doc-links", response.Data ?? new DocumentationLinkCheckReport(
                SetterPath: string.Empty,
                MarkdownFilesChecked: 0,
                LocalLinksChecked: 0,
                BrokenLinks: []));
        }
        else if (response.Data is not null)
        {
            ShowReport(response.Data, usePlainOutput);
        }

        if (!response.IsSuccess)
        {
            if (!useJsonOutput && response.Data is null)
            {
                ShowFailure(response, inputPath);
            }

            return 2;
        }

        return 0;
    }

    private static void ShowReport(
        DocumentationLinkCheckReport report,
        bool usePlainOutput)
    {
        if (usePlainOutput)
        {
            foreach (BrokenDocumentationLink brokenLink in report.BrokenLinks)
            {
                AnsiConsole.WriteLine(string.Join(
                    '\t',
                    brokenLink.SourceFile,
                    brokenLink.Target,
                    brokenLink.ResolvedPath));
            }

            return;
        }

        AnsiConsole.MarkupLine(
            "[bold]Markdown files checked:[/] {0}",
            report.MarkdownFilesChecked);
        AnsiConsole.MarkupLine(
            "[bold]Local links checked:[/] {0}",
            report.LocalLinksChecked);

        if (report.BrokenLinks.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]No broken local Markdown links were found.[/]");
            return;
        }

        Table table = new Table()
            .Border(TableBorder.Rounded)
            .Expand();
        table.AddColumn("Source");
        table.AddColumn("Target");
        table.AddColumn("Resolved path");

        foreach (BrokenDocumentationLink brokenLink in report.BrokenLinks)
        {
            table.AddRow(
                Markup.Escape(brokenLink.SourceFile),
                Markup.Escape(brokenLink.Target),
                Markup.Escape(brokenLink.ResolvedPath));
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine(
            "[red]Found {0} broken local Markdown link(s).[/]",
            report.BrokenLinks.Count);
    }

    private static void ShowInvalidOutputArguments()
    {
        CommandInputError.Show(
            "InvalidCheckDocLinksOutputArguments",
            "The --plain and --json switches are mutually exclusive and may be specified only once.",
            Usage);
    }

    private static void ShowFailure(
        Response<DocumentationLinkCheckReport> response,
        string inputPath)
    {
        ErrorCatalogValidationResult result = new();
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "CheckDocLinksFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "Setter documentation links could not be checked."
            : response.Message;

        result.AddError(failureCode, failureMessage, inputPath);
        new ConsoleValidationResultShow().Show(
            result,
            new ConsoleShowOptions { SourcePath = inputPath });
    }
}
