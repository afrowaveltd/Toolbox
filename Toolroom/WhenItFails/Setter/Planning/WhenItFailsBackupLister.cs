using Afrowave.Toolbox.Essentials.Results;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;

/// <summary>
/// Lists WhenItFails catalog backups without modifying the workspace.
/// </summary>
internal sealed class WhenItFailsBackupLister
{
    /// <summary>
    /// Lists all catalog backup files in a project or Jsons/WhenItFails directory.
    /// </summary>
    public Response<IReadOnlyList<WhenItFailsBackupInfo>> List(string inputPath)
    {
        if (string.IsNullOrWhiteSpace(inputPath))
        {
            return Response<IReadOnlyList<WhenItFailsBackupInfo>>.Invalid(
                code: "BackupPathIsEmpty",
                message: "Project root or Jsons/WhenItFails path cannot be empty.");
        }

        string jsonsPath = ResolveJsonsPath(inputPath);
        if (!Directory.Exists(jsonsPath))
        {
            return Response<IReadOnlyList<WhenItFailsBackupInfo>>.NotFound(
                code: "WhenItFailsDirectoryNotFound",
                message: $"WhenItFails JSON directory was not found: {jsonsPath}");
        }

        List<WhenItFailsBackupInfo> backups = Directory
            .EnumerateFiles(jsonsPath, "*.bak.json", SearchOption.TopDirectoryOnly)
            .Select(CreateBackupInfo)
            .OrderByDescending(backup => backup.LastWriteTimeUtc)
            .ThenBy(backup => backup.CatalogFileName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(backup => backup.BackupFileName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Response<IReadOnlyList<WhenItFailsBackupInfo>>.Ok(
            backups,
            backups.Count == 0
                ? "No WhenItFails catalog backups were found."
                : $"Found {backups.Count} WhenItFails catalog backup(s).");
    }

    private static string ResolveJsonsPath(string inputPath)
    {
        string fullPath = Path.GetFullPath(inputPath.Trim());
        string directoryName = Path.GetFileName(
            fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        string? parentName = Path.GetFileName(Path.GetDirectoryName(fullPath));

        if (string.Equals(directoryName, "WhenItFails", StringComparison.OrdinalIgnoreCase)
            && string.Equals(parentName, "Jsons", StringComparison.OrdinalIgnoreCase))
        {
            return fullPath;
        }

        return Path.Combine(fullPath, "Jsons", "WhenItFails");
    }

    private static WhenItFailsBackupInfo CreateBackupInfo(string filePath)
    {
        FileInfo file = new(filePath);
        string backupFileName = file.Name;
        string catalogFileName = InferCatalogFileName(backupFileName);

        return new WhenItFailsBackupInfo(
            CatalogFileName: catalogFileName,
            BackupFileName: backupFileName,
            FullPath: file.FullName,
            SizeBytes: file.Length,
            LastWriteTimeUtc: file.LastWriteTimeUtc);
    }

    private static string InferCatalogFileName(string backupFileName)
    {
        const string suffix = ".bak.json";
        string withoutSuffix = backupFileName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
            ? backupFileName[..^suffix.Length]
            : Path.GetFileNameWithoutExtension(backupFileName);

        int timestampSeparator = withoutSuffix.LastIndexOf('.');
        string catalogStem = timestampSeparator > 0
            ? withoutSuffix[..timestampSeparator]
            : withoutSuffix;

        return catalogStem + ".json";
    }
}

/// <summary>
/// Describes one WhenItFails catalog backup.
/// </summary>
internal sealed record WhenItFailsBackupInfo(
    string CatalogFileName,
    string BackupFileName,
    string FullPath,
    long SizeBytes,
    DateTime LastWriteTimeUtc);