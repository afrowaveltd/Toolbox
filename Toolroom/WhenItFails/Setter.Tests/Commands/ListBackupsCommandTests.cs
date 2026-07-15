using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class ListBackupsCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithBackups_ReturnsSuccessWithoutChangingFiles()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();
        string firstBackup = Path.Combine(
            workspace.WhenItFailsJsonsPath,
            "errors.en.20260715T080000000Z.bak.json");
        string secondBackup = Path.Combine(
            workspace.WhenItFailsJsonsPath,
            "profiles.20260715T090000000Z.bak.json");
        await File.WriteAllTextAsync(firstBackup, "first");
        await File.WriteAllTextAsync(secondBackup, "second");
        Dictionary<string, string> before = await ReadAllFilesAsync(workspace.WhenItFailsJsonsPath);

        int exitCode = await ListBackupsCommand.ExecuteAsync(
        [
            "list-backups",
            workspace.ProjectRootPath,
            "--plain"
        ]);

        Assert.Equal(0, exitCode);
        Dictionary<string, string> after = await ReadAllFilesAsync(workspace.WhenItFailsJsonsPath);
        Assert.Equal(before, after);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutBackups_ReturnsSuccess()
    {
        using TemporaryWhenItFailsWorkspace workspace =
            await TemporaryWhenItFailsWorkspace.CreateInitializedAsync();

        Assert.Equal(
            0,
            await ListBackupsCommand.ExecuteAsync(
            [
                "list-backups",
                workspace.WhenItFailsJsonsPath
            ]));
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingDirectory_ReturnsListingFailure()
    {
        string missingPath = Path.Combine(
            Path.GetTempPath(),
            "WhenItFailsSetterTests",
            Guid.NewGuid().ToString("N"));

        Assert.Equal(
            2,
            await ListBackupsCommand.ExecuteAsync(
            [
                "list-backups",
                missingPath
            ]));
    }

    public static TheoryData<string[]> InvalidArgumentCases =>
        new()
        {
            new[] { "list-backups" },
            new[] { "list-backups", ".", "--unknown" },
            new[] { "list-backups", ".", "--plain", "EXTRA" }
        };

    [Theory]
    [MemberData(nameof(InvalidArgumentCases))]
    public async Task ExecuteAsync_WithInvalidArguments_ReturnsCommandInputError(string[] args)
    {
        Assert.Equal(1, await ListBackupsCommand.ExecuteAsync(args));
    }

    private static async Task<Dictionary<string, string>> ReadAllFilesAsync(string directoryPath)
    {
        Dictionary<string, string> contents = new(StringComparer.OrdinalIgnoreCase);
        foreach (string filePath in Directory.EnumerateFiles(
                     directoryPath,
                     "*",
                     SearchOption.TopDirectoryOnly))
        {
            contents[Path.GetFileName(filePath)] = await File.ReadAllTextAsync(filePath);
        }

        return contents;
    }
}
