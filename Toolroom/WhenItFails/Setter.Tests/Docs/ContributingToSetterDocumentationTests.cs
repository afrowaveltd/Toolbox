namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Docs;

public sealed class ContributingToSetterDocumentationTests
{
    [Fact]
    public void Documentation_DescribesCurrentContributionWorkflow()
    {
        string repositoryRoot = FindRepositoryRoot();
        string documentationPath = Path.Combine(
            repositoryRoot,
            "Toolroom",
            "WhenItFails",
            "Setter",
            "Docs",
            "Contributing to Setter",
            "en.md");
        string documentation = File.ReadAllText(documentationPath);

        string[] requiredContent =
        [
            "# Contributing to Setter",
            "GitHub `master` is the source of truth.",
            "One logical change per commit",
            "Add or update the corresponding test immediately",
            "dotnet test Toolroom/WhenItFails/Setter.Tests",
            "`README.md`",
            "`Docs/<topic>/en.md`",
            "`IMPLEMENTATION_STATUS.md`",
            "`check-doc-keys`",
            "`check-doc-links`",
            "git diff --check",
            "Do not continue while the focused Setter suite is red."
        ];

        foreach (string expected in requiredContent)
        {
            Assert.Contains(expected, documentation, StringComparison.Ordinal);
        }

        string[] staleContent =
        [
            "This is especially important for:\n\n- restore commands,\n- JSON output,\n- profile editing commands",
            "Setter currently does not provide a dedicated restore command.",
            "documentation-only changes, this may not apply directly"
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
