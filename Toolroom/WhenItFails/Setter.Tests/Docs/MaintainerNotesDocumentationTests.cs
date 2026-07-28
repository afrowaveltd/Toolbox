namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Docs;

public sealed class MaintainerNotesDocumentationTests
{
    [Fact]
    public void Documentation_DescribesCurrentMaintainerContinuationWorkflow()
    {
        string repositoryRoot = FindRepositoryRoot();
        string documentationPath = Path.Combine(
            repositoryRoot,
            "Toolroom",
            "WhenItFails",
            "Setter",
            "Docs",
            "Maintainer Notes",
            "en.md");
        string documentation = File.ReadAllText(documentationPath);

        string[] requiredContent =
        [
            "# Maintainer Notes",
            "GitHub `master` is the source of truth.",
            "`IMPLEMENTATION_STATUS.md`",
            "dotnet test Toolroom/WhenItFails/Setter.Tests",
            "One small green step at a time",
            "`--json`",
            "`list-backups`",
            "`restore-backup`",
            "single-file operations",
            "not multi-file transactions",
            "git diff --check",
            "Do not continue while the focused Setter suite is red."
        ];

        foreach (string expected in requiredContent)
        {
            Assert.Contains(expected, documentation, StringComparison.Ordinal);
        }

        string[] staleContent =
        [
            "A future JSON output mode would be better for durable automation.",
            "when no restore command exists.",
            "Current Setter profile filtering is intentionally simpler than possible runtime profile behavior."
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
