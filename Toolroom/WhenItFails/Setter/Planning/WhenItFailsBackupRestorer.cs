using Afrowave.Toolbox.Essentials.Results;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;

/// <summary>
/// Safely restores one catalog backup and validates the complete workspace.
/// </summary>
internal sealed class WhenItFailsBackupRestorer
{
    private static readonly HashSet<string> SupportedCatalogFiles =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "errors.en.json",
            "categories.en.json",
            "code-groups.en.json",
            "owners.en.json",
            "profiles.json"
        };

    /// <summary>
    /// Restores an exact backup file name from the workspace backup directory.
    /// The current active catalog is backed up first. Invalid restored content is rolled back.
    /// </summary>
    public async Task<Response<WhenItFailsBackupRestoreResult>> RestoreAsync(
        string inputPath,
        string backupFileName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(inputPath))
        {
            return Response<WhenItFailsBackupRestoreResult>.Invalid(
                code: "BackupRestorePathIsEmpty",
                message: "Project root or Jsons/WhenItFails path cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(backupFileName))
        {
            return Response<WhenItFailsBackupRestoreResult>.Invalid(
                code: "BackupFileNameIsEmpty",
                message: "Backup file name cannot be empty.");
        }

        string requestedFileName = backupFileName.Trim();
        if (!string.Equals(requestedFileName, Path.GetFileName(requestedFileName), StringComparison.Ordinal)
            || requestedFileName.IndexOfAny([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]) >= 0)
        {
            return Response<WhenItFailsBackupRestoreResult>.Invalid(
                code: "BackupFileNameMustNotContainPath",
                message: "Specify an exact backup file name, not a path.");
        }

        Response<IReadOnlyList<WhenItFailsBackupInfo>> listResponse =
            new WhenItFailsBackupLister().List(inputPath);
        if (!listResponse.IsSuccess || listResponse.Data is null)
        {
            return Response<WhenItFailsBackupRestoreResult>.Fail(
                code: listResponse.Issues.Count > 0
                    ? listResponse.Issues[0].Code
                    : "BackupListFailed",
                message: string.IsNullOrWhiteSpace(listResponse.Message)
                    ? "Workspace backups could not be listed."
                    : listResponse.Message);
        }

        WhenItFailsBackupInfo? selected = listResponse.Data.FirstOrDefault(backup =>
            string.Equals(backup.BackupFileName, requestedFileName, StringComparison.OrdinalIgnoreCase));
        if (selected is null)
        {
            return Response<WhenItFailsBackupRestoreResult>.NotFound(
                code: "BackupNotFound",
                message: $"Backup '{requestedFileName}' was not found in the workspace.");
        }

        if (!SupportedCatalogFiles.Contains(selected.CatalogFileName))
        {
            return Response<WhenItFailsBackupRestoreResult>.Invalid(
                code: "UnsupportedBackupCatalog",
                message: $"Backup '{selected.BackupFileName}' does not target a supported catalog.");
        }

        string targetPath = Path.Combine(
            Path.GetDirectoryName(selected.FullPath)!,
            selected.CatalogFileName);
        if (!File.Exists(targetPath))
        {
            return Response<WhenItFailsBackupRestoreResult>.NotFound(
                code: "ActiveCatalogNotFound",
                message: $"Active catalog was not found: {targetPath}");
        }

        byte[] activeContent = await File.ReadAllBytesAsync(targetPath, cancellationToken);
        string safetyBackupPath = CreateUniqueSafetyBackupPath(targetPath);
        await File.WriteAllBytesAsync(safetyBackupPath, activeContent, cancellationToken);

        try
        {
            byte[] restoredContent = await File.ReadAllBytesAsync(selected.FullPath, cancellationToken);
            await File.WriteAllBytesAsync(targetPath, restoredContent, cancellationToken);

            WhenItFailsWorkspaceValidationOutcome validation =
                await new WhenItFailsWorkspaceValidator().ValidateAsync(inputPath, cancellationToken);
            if (!validation.ValidationResult.IsValid)
            {
                await File.WriteAllBytesAsync(targetPath, activeContent, cancellationToken);
                return Response<WhenItFailsBackupRestoreResult>.Invalid(
                    code: "RestoredWorkspaceIsInvalid",
                    message: $"Backup '{selected.BackupFileName}' produced an invalid workspace. The active catalog was rolled back.");
            }

            WhenItFailsBackupRestoreResult result = new(
                CatalogFileName: selected.CatalogFileName,
                RestoredBackupFileName: selected.BackupFileName,
                ActiveCatalogPath: targetPath,
                SafetyBackupFileName: Path.GetFileName(safetyBackupPath));

            return Response<WhenItFailsBackupRestoreResult>.Ok(
                result,
                $"Backup '{selected.BackupFileName}' was restored successfully.");
        }
        catch
        {
            await File.WriteAllBytesAsync(targetPath, activeContent, CancellationToken.None);
            throw;
        }
    }

    private static string CreateUniqueSafetyBackupPath(string targetPath)
    {
        string directory = Path.GetDirectoryName(targetPath)!;
        string stem = Path.GetFileNameWithoutExtension(targetPath);
        string timestamp = DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmssfff'Z'");
        string candidate = Path.Combine(directory, $"{stem}.{timestamp}.bak.json");
        int suffix = 1;

        while (File.Exists(candidate))
        {
            candidate = Path.Combine(directory, $"{stem}.{timestamp}.{suffix}.bak.json");
            suffix++;
        }

        return candidate;
    }
}

/// <summary>
/// Describes a successful catalog backup restoration.
/// </summary>
internal sealed record WhenItFailsBackupRestoreResult(
    string CatalogFileName,
    string RestoredBackupFileName,
    string ActiveCatalogPath,
    string SafetyBackupFileName);
