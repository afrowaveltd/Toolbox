namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Docs;

public sealed class SafeWritesDocumentationTests
{
    [Fact]
    public void Documentation_DescribesCurrentSafeWriteContract()
    {
        string repositoryRoot = FindRepositoryRoot();
        string documentationPath = Path.Combine(
            repositoryRoot,
            "Toolroom",
            "WhenItFails",
            "Setter",
            "Docs",
            "Safe Writes",
            "en.md");
        string documentation = File.ReadAllText(documentationPath);

        string[] requiredContent =
        [
            "# Safe Writes",
            "validate before replacement",
            "temporary file",
            "timestamped backup",
            "target file remains unchanged",
            "no backup is created",
            "`list-backups`",
            "`restore-backup`",
            "single-file operation",
            "not a multi-file transaction",
            "not a multi-process locking system",
            "dotnet test Toolroom/WhenItFails/Setter.Tests",
            "git status --short",
            ".bak.json",
            "Do not retry blindly after a failed write."
        ];

        foreach (string expected in requiredContent)
        {
            Assert.Contains(expected, documentation, StringComparison.Ordinal);
        }

        string[] staleContent =
        [
            "All currently supported Setter edits affect one error definition at a time.",
            "Each current write command modifies only:",
            "errors.en.json"
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
