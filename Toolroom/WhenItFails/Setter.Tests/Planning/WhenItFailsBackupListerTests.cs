using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsBackupListerTests
{
    [Fact]
    public async Task List_ShouldReturnBackupsNewestFirstWithCatalogInformation()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        string olderPath = Path.Combine(
            workspace.WhenItFailsJsonsPath,
            "errors.en.20260715T080000000Z.bak.json");
        string newerPath = Path.Combine(
            workspace.WhenItFailsJsonsPath,
            "profiles.20260715T090000000Z.bak.json");

        await File.WriteAllTextAsync(olderPath, "older");
        await File.WriteAllTextAsync(newerPath, "newer-backup");
        File.SetLastWriteTimeUtc(olderPath, new DateTime(2026, 7, 15, 8, 0, 0, DateTimeKind.Utc));
        File.SetLastWriteTimeUtc(newerPath, new DateTime(2026, 7, 15, 9, 0, 0, DateTimeKind.Utc));

        Response<IReadOnlyList<WhenItFailsBackupInfo>> response =
            new WhenItFailsBackupLister().List(workspace.ProjectRootPath);

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal(2, response.Data.Count);
        Assert.Equal("profiles.json", response.Data[0].CatalogFileName);
        Assert.Equal(Path.GetFileName(newerPath), response.Data[0].BackupFileName);
        Assert.Equal(new FileInfo(newerPath).Length, response.Data[0].SizeBytes);
        Assert.Equal("errors.en.json", response.Data[1].CatalogFileName);
        Assert.Equal(Path.GetFullPath(olderPath), response.Data[1].FullPath);
    }

    [Fact]
    public async Task List_ShouldAcceptDirectWhenItFailsDirectoryPath()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        string backupPath = Path.Combine(
            workspace.WhenItFailsJsonsPath,
            "owners.en.20260715T100000000Z.bak.json");
        await File.WriteAllTextAsync(backupPath, "backup");

        Response<IReadOnlyList<WhenItFailsBackupInfo>> response =
            new WhenItFailsBackupLister().List(workspace.WhenItFailsJsonsPath);

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        WhenItFailsBackupInfo backup = Assert.Single(response.Data);
        Assert.Equal("owners.en.json", backup.CatalogFileName);
    }

    [Fact]
    public async Task List_ShouldReturnSuccessfulEmptyResultWhenNoBackupsExist()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        Response<IReadOnlyList<WhenItFailsBackupInfo>> response =
            new WhenItFailsBackupLister().List(workspace.ProjectRootPath);

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Empty(response.Data);
        Assert.Contains("No WhenItFails catalog backups", response.Message);
    }

    [Fact]
    public void List_ShouldReturnNotFoundForMissingWorkspaceDirectory()
    {
        string missingPath = Path.Combine(
            Path.GetTempPath(),
            "Afrowave.Toolbox.Tests",
            Guid.NewGuid().ToString("N"));

        Response<IReadOnlyList<WhenItFailsBackupInfo>> response =
            new WhenItFailsBackupLister().List(missingPath);

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "WhenItFailsDirectoryNotFound");
    }

    [Fact]
    public void List_ShouldRejectEmptyPath()
    {
        Response<IReadOnlyList<WhenItFailsBackupInfo>> response =
            new WhenItFailsBackupLister().List(" ");

        Assert.False(response.IsSuccess);
        Assert.Contains(response.Issues, issue => issue.Code == "BackupPathIsEmpty");
    }
}