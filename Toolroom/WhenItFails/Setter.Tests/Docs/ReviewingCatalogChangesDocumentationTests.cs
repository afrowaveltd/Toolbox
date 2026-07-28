namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Docs;

public sealed class ReviewingCatalogChangesDocumentationTests
{
    [Fact]
    public void Documentation_DescribesCurrentCatalogReviewWorkflow()
    {
        string repositoryRoot = FindRepositoryRoot();
        string documentationPath = Path.Combine(
            repositoryRoot,
            "Toolroom",
            "WhenItFails",
            "Setter",
            "Docs",
            "Reviewing Catalog Changes",
            "en.md");
        string documentation = File.ReadAllText(documentationPath);

        string[] requiredContent =
        [
            "# Reviewing Catalog Changes",
            "Review the contract, not only the JSON diff.",
            "dotnet test Toolroom/WhenItFails/Setter.Tests",
            "`error-references`",
            "`explain-profile`",
            "`check-doc-keys`",
            "`check-doc-links`",
            "`--json`",
            "git diff --check",
            "git status --short",
            ".bak.json",
            "One logical change per commit",
            "Do not approve a red change."
        ];

        foreach (string expected in requiredContent)
        {
            Assert.Contains(expected, documentation, StringComparison.Ordinal);
        }

        string[] staleContent =
        [
            "Setter browsing currently uses simplified profile filtering.",
            "It does not fully enforce all runtime-style profile semantics.",
            "dotnet build\n\ndotnet test"
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
