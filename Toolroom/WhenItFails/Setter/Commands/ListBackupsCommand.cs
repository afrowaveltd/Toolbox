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
    private const string Usage = "list-backups <path> [--plain]";

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

        if (args.Length > 3)
        {
            CommandInputError.Show(
                "InvalidListBackupsArguments",
                "The list-backups command accepts only a path and optional --plain switch.",
                Usage);
            return Task.FromResult(1);
        }

        bool plain = false;
        if (args.Length == 3)
        {
            if (!string.Equals(args[2], "--plain", StringComparison.OrdinalIgnoreCase))
            {
                CommandInputError.Show(
                    "UnknownListBackupsOption",
                    $"Unknown list-backups option '{args[2]}'.",
                    Usage);
                return Task.FromResult(1);
            }

            plain = true;
        }

        string inputPath = args[1];
        Response<IReadOnlyList<WhenItFailsBackupInfo>> response =
            new WhenItFailsBackupLister().List(inputPath);

        if (!response.IsSuccess || response.Data is null)
        {
            ShowFailure(response, inputPath);
            return Task.FromResult(2);
        }

        if (plain)
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
