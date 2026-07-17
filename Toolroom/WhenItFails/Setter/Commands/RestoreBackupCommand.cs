using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.SeeMe.WhenItFails.Console;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.WhenItFails.Validation;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Handles the 'restore-backup' command.
/// </summary>
internal static class RestoreBackupCommand
{
    private const string Usage =
        "restore-backup <path> <backup-file-name> [--json]";

    public static async Task<int> ExecuteAsync(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            CommandInputError.Show(
                "MissingRestoreBackupPath",
                "The restore-backup command requires a project root or Jsons/WhenItFails directory path.",
                Usage);
            return 1;
        }

        if (args.Length < 3 || string.IsNullOrWhiteSpace(args[2]))
        {
            CommandInputError.Show(
                "MissingRestoreBackupFileName",
                "The restore-backup command requires an exact backup file name from list-backups.",
                Usage);
            return 1;
        }

        bool useJsonOutput = false;
        for (int index = 3; index < args.Length; index++)
        {
            if (string.Equals(args[index], "--json", StringComparison.OrdinalIgnoreCase)
                && !useJsonOutput)
            {
                useJsonOutput = true;
                continue;
            }

            CommandInputError.Show(
                "InvalidRestoreBackupArguments",
                $"Unknown or duplicate restore-backup argument '{args[index]}'.",
                Usage);
            return 1;
        }

        string inputPath = args[1];
        string backupFileName = args[2];
        Response<WhenItFailsBackupRestoreResult> response =
            await new WhenItFailsBackupRestorer().RestoreAsync(
                inputPath,
                backupFileName);

        if (!response.IsSuccess || response.Data is null)
        {
            if (useJsonOutput)
            {
                CommandJsonOutput.Write(
                    "restore-backup",
                    new RestoreBackupCommandResult(
                        Restored: false,
                        Result: null,
                        FailureCode: response.Issues.Count > 0
                            ? response.Issues[0].Code
                            : "RestoreBackupFailed",
                        FailureMessage: string.IsNullOrWhiteSpace(response.Message)
                            ? "The backup could not be restored."
                            : response.Message));
            }
            else
            {
                ShowFailure(response, inputPath, backupFileName);
            }

            return 2;
        }

        if (useJsonOutput)
        {
            CommandJsonOutput.Write(
                "restore-backup",
                new RestoreBackupCommandResult(
                    Restored: true,
                    Result: response.Data,
                    FailureCode: null,
                    FailureMessage: null));
            return 0;
        }

        AnsiConsole.MarkupLine("[green]Backup restored successfully.[/]");
        AnsiConsole.MarkupLine(
            "[bold]Catalog:[/] {0}",
            Markup.Escape(response.Data.CatalogFileName));
        AnsiConsole.MarkupLine(
            "[bold]Restored backup:[/] {0}",
            Markup.Escape(response.Data.RestoredBackupFileName));
        AnsiConsole.MarkupLine(
            "[bold]Safety backup:[/] {0}",
            Markup.Escape(response.Data.SafetyBackupFileName));
        return 0;
    }

    private static void ShowFailure(
        Response<WhenItFailsBackupRestoreResult> response,
        string inputPath,
        string backupFileName)
    {
        ErrorCatalogValidationResult result = new();
        result.AddError(
            response.Issues.Count > 0
                ? response.Issues[0].Code
                : "RestoreBackupFailed",
            string.IsNullOrWhiteSpace(response.Message)
                ? "The backup could not be restored."
                : response.Message,
            backupFileName);

        new ConsoleValidationResultShow().Show(
            result,
            new ConsoleShowOptions { SourcePath = inputPath });
    }

    private sealed record RestoreBackupCommandResult(
        bool Restored,
        WhenItFailsBackupRestoreResult? Result,
        string? FailureCode,
        string? FailureMessage);
}
