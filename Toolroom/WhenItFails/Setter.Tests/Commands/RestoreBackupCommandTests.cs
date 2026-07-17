using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class RestoreBackupCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidBackup_RestoresCatalog()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        string activePath = Path.Combine(workspace.WhenItFailsJsonsPath, "errors.en.json");
        string originalContent = await File.ReadAllTextAsync(activePath);
        string backupPath = Path.Combine(
            workspace.WhenItFailsJsonsPath,
            "errors.en.20260718T040000000Z.bak.json");
        await File.WriteAllTextAsync(backupPath, originalContent);
        await File.WriteAllTextAsync(
            activePath,
            originalContent.Replace(
                "Network unavailable",
                "Network briefly unavailable",
                StringComparison.Ordinal));

        int exitCode = await RestoreBackupCommand.ExecuteAsync(
        [
            "restore-backup",
            workspace.ProjectRootPath,
            Path.GetFileName(backupPath)
        ]);

        Assert.Equal(0, exitCode);
        Assert.Equal(originalContent, await File.ReadAllTextAsync(activePath));
    }

    [Fact]
    public async Task ExecuteAsync_WithValidBackupAndJsonOutput_ReturnsSuccess()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        string activePath = Path.Combine(workspace.WhenItFailsJsonsPath, "profiles.json");
        string backupPath = Path.Combine(
            workspace.WhenItFailsJsonsPath,
            "profiles.20260718T050000000Z.bak.json");
        File.Copy(activePath, backupPath);

        int exitCode = await RestoreBackupCommand.ExecuteAsync(
        [
            "restore-backup",
            workspace.ProjectRootPath,
            Path.GetFileName(backupPath),
            "--json"
        ]);

        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownBackup_ReturnsEditFailure()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        int exitCode = await RestoreBackupCommand.ExecuteAsync(
        [
            "restore-backup",
            workspace.ProjectRootPath,
            "errors.en.missing.bak.json"
        ]);

        Assert.Equal(2, exitCode);
    }

    public static TheoryData<string[]> InvalidArgumentCases =>
        new()
        {
            new[] { "restore-backup" },
            new[] { "restore-backup", "." },
            new[] { "restore-backup", ".", "backup.bak.json", "--unknown" },
            new[] { "restore-backup", ".", "backup.bak.json", "--json", "--json" }
        };

    [Theory]
    [MemberData(nameof(InvalidArgumentCases))]
    public async Task ExecuteAsync_WithInvalidArguments_ReturnsCommandInputError(string[] args)
    {
        Assert.Equal(1, await RestoreBackupCommand.ExecuteAsync(args));
    }
}
