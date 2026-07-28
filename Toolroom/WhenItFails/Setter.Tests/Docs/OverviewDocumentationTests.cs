namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Docs;

public sealed class OverviewDocumentationTests
{
    [Fact]
    public void Documentation_DescribesCurrentSetterProductSurface()
    {
        string repositoryRoot = FindRepositoryRoot();
        string documentationPath = Path.Combine(
            repositoryRoot,
            "Toolroom",
            "WhenItFails",
            "Setter",
            "Docs",
            "Overview",
            "en.md");
        string documentation = File.ReadAllText(documentationPath);

        string[] requiredContent =
        [
            "# WhenItFails Setter Overview",
            "`.NET 10`",
            "`Jsons/WhenItFails`",
            "`add-error`",
            "`remove-error`",
            "`error-references`",
            "`explain-profile`",
            "`list-backups`",
            "`restore-backup`",
            "`check-doc-keys`",
            "`check-doc-links`",
            "`--plain`",
            "`--json`",
            "Machine consumers should use JSON output and process exit codes.",
            "dotnet test Toolroom/WhenItFails/Setter.Tests",
            "`IMPLEMENTATION_STATUS.md`"
        ];

        foreach (string expected in requiredContent)
        {
            Assert.Contains(expected, documentation, StringComparison.Ordinal);
        }

        string[] staleContent =
        [
            "It is not currently a formal serialization format.",
            "If adding a future JSON output mode",
            "Setter currently supports safe updates of:",
            "Stable identity fields are not currently edited through simple setter commands.",
            "Spectre.Console 0.57.1"
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
