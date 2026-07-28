namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Docs;

public sealed class CatalogAuthorChecklistDocumentationTests
{
    [Fact]
    public void Documentation_DescribesCurrentCatalogAuthoringWorkflow()
    {
        string repositoryRoot = FindRepositoryRoot();
        string documentationPath = Path.Combine(
            repositoryRoot,
            "Toolroom",
            "WhenItFails",
            "Setter",
            "Docs",
            "Catalog Author Checklist",
            "en.md");
        string documentation = File.ReadAllText(documentationPath);

        string[] requiredContent =
        [
            "# Catalog author checklist",
            "One logical catalog change at a time.",
            "`reference`",
            "`next-code`",
            "`suggest-doc-key`",
            "`add-error`",
            "`details`",
            "`error-references`",
            "`explain-profile`",
            "`check-doc-keys`",
            "`check-doc-links`",
            "dotnet test Toolroom/WhenItFails/Setter.Tests",
            "git diff --check",
            "git status --short",
            ".bak.json",
            "`IMPLEMENTATION_STATUS.md`",
            "Do not start the next catalog change while this one is red."
        ];

        foreach (string expected in requiredContent)
        {
            Assert.Contains(expected, documentation, StringComparison.Ordinal);
        }

        string[] staleContent =
        [
            "Manual JSON editing is currently needed for:\n\n- adding a new error",
            "adding or changing profiles",
            "changing fields not yet supported by Setter edit commands"
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
