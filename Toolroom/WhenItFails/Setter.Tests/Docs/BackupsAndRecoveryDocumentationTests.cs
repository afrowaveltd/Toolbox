namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Docs;

public sealed class BackupsAndRecoveryDocumentationTests
{
    [Fact]
    public void Documentation_DescribesCurrentBackupRecoveryWorkflow()
    {
        string repositoryRoot = FindRepositoryRoot();
        string documentationPath = Path.Combine(
            repositoryRoot,
            "Toolroom",
            "WhenItFails",
            "Setter",
            "Docs",
            "Backups and Recovery",
            "en.md");
        string documentation = File.ReadAllText(documentationPath);

        string[] requiredContent =
        [
            "# Backups and Recovery",
            "`list-backups`",
            "`restore-backup`",
            "Do not restore by timestamp alone.",
            "single catalog file",
            "not a complete workspace snapshot",
            "git status --short",
            "git diff --check",
            "dotnet run --project Toolroom/WhenItFails/Setter -- validate .",
            "dotnet test Toolroom/WhenItFails/Setter.Tests",
            ".bak.json",
            "one active writer per workspace",
            "Do not continue editing after an unverified restore."
        ];

        foreach (string expected in requiredContent)
        {
            Assert.Contains(expected, documentation, StringComparison.Ordinal);
        }

        string[] staleContent =
        [
            "Setter currently does not provide a dedicated restore command.",
            "Manual restoration should therefore be deliberate and reviewable."
        ];

        foreach (string stale in staleContent)
        {
            Assert.DoesNotContain(stale, documentation, StringComparison.Ordinal);
        }
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Toolbox.sln")) &&
                Directory.Exists(Path.Combine(directory.FullName, "Toolroom", "WhenItFails", "Setter")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate the Toolbox repository root from the test output directory.");
    }
}
