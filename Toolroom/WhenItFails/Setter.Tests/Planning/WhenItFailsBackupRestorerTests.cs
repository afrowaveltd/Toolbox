using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsBackupRestorerTests
{
    [Fact]
    public async Task RestoreAsync_ShouldRestoreSelectedBackupAndCreateSafetyBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        string activePath = Path.Combine(workspace.WhenItFailsJsonsPath, "errors.en.json");
        string originalContent = await File.ReadAllTextAsync(activePath);
        string backupPath = Path.Combine(
            workspace.WhenItFailsJsonsPath,
            "errors.en.20260718T010000000Z.bak.json");
        await File.WriteAllTextAsync(backupPath, originalContent);

        string changedContent = originalContent.Replace(
            "Network unavailable",
            "Network temporarily unavailable",
            StringComparison.Ordinal);
        Assert.NotEqual(originalContent, changedContent);
        await File.WriteAllTextAsync(activePath, changedContent);

        Response<WhenItFailsBackupRestoreResult> response =
            await new WhenItFailsBackupRestorer().RestoreAsync(
                workspace.ProjectRootPath,
                Path.GetFileName(backupPath));

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal(originalContent, await File.ReadAllTextAsync(activePath));
        Assert.Equal("errors.en.json", response.Data.CatalogFileName);
        Assert.Equal(Path.GetFileName(backupPath), response.Data.RestoredBackupFileName);

        string safetyBackupPath = Path.Combine(
            workspace.WhenItFailsJsonsPath,
            response.Data.SafetyBackupFileName);
        Assert.True(File.Exists(safetyBackupPath));
        Assert.Equal(changedContent, await File.ReadAllTextAsync(safetyBackupPath));
    }

    [Fact]
    public async Task RestoreAsync_ShouldRollbackWhenBackupMakesWorkspaceInvalid()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        string activePath = Path.Combine(workspace.WhenItFailsJsonsPath, "errors.en.json");
        byte[] activeContent = await File.ReadAllBytesAsync(activePath);
        string backupPath = Path.Combine(
            workspace.WhenItFailsJsonsPath,
            "errors.en.20260718T020000000Z.bak.json");
        await File.WriteAllTextAsync(backupPath, "{}");

        Response<WhenItFailsBackupRestoreResult> response =
            await new WhenItFailsBackupRestorer().RestoreAsync(
                workspace.ProjectRootPath,
                Path.GetFileName(backupPath));

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "RestoredWorkspaceIsInvalid");
        Assert.Equal(activeContent, await File.ReadAllBytesAsync(activePath));
        Assert.Contains(
            Directory.EnumerateFiles(workspace.WhenItFailsJsonsPath, "errors.en.*.bak.json"),
            path => !string.Equals(path, backupPath, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task RestoreAsync_ShouldRejectPathInsteadOfBackupFileName()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        Response<WhenItFailsBackupRestoreResult> response =
            await new WhenItFailsBackupRestorer().RestoreAsync(
                workspace.ProjectRootPath,
                Path.Combine("nested", "errors.en.backup.bak.json"));

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "BackupFileNameMustNotContainPath");
    }

    [Fact]
    public async Task RestoreAsync_ShouldReturnNotFoundForUnknownBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        Response<WhenItFailsBackupRestoreResult> response =
            await new WhenItFailsBackupRestorer().RestoreAsync(
                workspace.ProjectRootPath,
                "errors.en.unknown.bak.json");

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "BackupNotFound");
    }

    [Fact]
    public async Task RestoreAsync_ShouldRejectUnsupportedCatalogBackup()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        string backupPath = Path.Combine(
            workspace.WhenItFailsJsonsPath,
            "custom.20260718T030000000Z.bak.json");
        await File.WriteAllTextAsync(backupPath, "{}");

        Response<WhenItFailsBackupRestoreResult> response =
            await new WhenItFailsBackupRestorer().RestoreAsync(
                workspace.ProjectRootPath,
                Path.GetFileName(backupPath));

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "UnsupportedBackupCatalog");
    }
}
