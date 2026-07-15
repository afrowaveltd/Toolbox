using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'list-backups' command.
/// </summary>
internal static class ListBackupsCommand
{
    private const string Usage = "list-backups <path> [--plain|--json]";

    public static Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                "MissingListBackupsPath",
                "The list-backups command requires a project root or Jsons/WhenItFails directory path.",
                Usage);
            return Task.FromResult(1);
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
                    return Task.FromResult(1);
                }

                usePlainOutput = true;
                continue;
            }

            if (string.Equals(args[index], "--json", StringComparison.OrdinalIgnoreCase))
            {
                if (useJsonOutput || usePlainOutput)
                {
                    ShowInvalidOutputArguments();
                    return Task.FromResult(1);
                }

                useJsonOutput = true;
                continue;
            }

            CommandInputError.Show(
                "UnknownListBackupsOption",
                $"Unknown list-backups option '{args[index]}'.",
                Usage);
            return Task.FromResult(1);
        }

        string inputPath = args[1];
        Response<IReadOnlyList<WhenItFailsBackupInfo>> response =
            new WhenItFailsBackupLister().List(inputPath);

        if (!response.IsSuccess || response.Data is null)
        {
            ShowFailure(response, inputPath);
            return Task.FromResult(2);
        }

        if (usePlainOutput)
        {
            foreach (WhenItFailsBackupInfo backup in response.Data)
            {
                AnsiConsole.WriteLine(string.Join(
                    '\t',
                    backup.LastWriteTimeUtc.ToString("O"),
                    backup.CatalogFileName,
                    backup.BackupFileName,
                    backup.SizeBytes.ToString(),
                    backup.FullPath));
            }

            return Task.FromResult(0);
        }

        if (useJsonOutput)
        {
            CommandJsonOutput.Write("list-backups", response.Data);
            return Task.FromResult(0);
        }

        if (response.Data.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]No WhenItFails catalog backups were found.[/]");
            return Task.FromResult(0);
        }

        Table table = new Table()
            .Border(TableBorder.Rounded)
            .Expand();
        table.AddColumn("Catalog");
        table.AddColumn("Backup");
        table.AddColumn("UTC");
        table.AddColumn(new TableColumn("Bytes").RightAligned());

        foreach (WhenItFailsBackupInfo backup in response.Data)
        {
            table.AddRow(
                Markup.Escape(backup.CatalogFileName),
                Markup.Escape(backup.BackupFileName),
                Markup.Escape(backup.LastWriteTimeUtc.ToString("yyyy-MM-dd HH:mm:ss")),
                backup.SizeBytes.ToString("N0"));
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("[grey]Found {0} backup(s).[/]", response.Data.Count);
        return Task.FromResult(0);
    }

    private static void ShowInvalidOutputArguments()
    {
        CommandInputError.Show(
            "InvalidListBackupsOutputArguments",
            "The --plain and --json switches are mutually exclusive and may be specified only once.",
            Usage);
    }

    private static void ShowFailure(
        Response<IReadOnlyList<WhenItFailsBackupInfo>> response,
        string inputPath)
    {
        ErrorCatalogValidationResult result = new();
        string failureCode = response.Issues.Count > 0
            ? response.Issues[0].Code
            : "ListBackupsFailed";
        string failureMessage = string.IsNullOrWhiteSpace(response.Message)
            ? "WhenItFails catalog backups could not be listed."
            : response.Message;

        result.AddError(failureCode, failureMessage, inputPath);
        new ConsoleValidationResultShow().Show(
            result,
            new ConsoleShowOptions { SourcePath = inputPath });
    }
}
