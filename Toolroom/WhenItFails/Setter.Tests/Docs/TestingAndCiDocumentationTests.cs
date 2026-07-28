namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Docs;

public sealed class TestingAndCiDocumentationTests
{
    [Fact]
    public void Documentation_DescribesCurrentVerificationWorkflow()
    {
        string repositoryRoot = FindRepositoryRoot();
        string documentationPath = Path.Combine(
            repositoryRoot,
            "Toolroom",
            "WhenItFails",
            "Setter",
            "Docs",
            "Testing and CI",
            "en.md");
        string documentation = File.ReadAllText(documentationPath);

        string[] requiredContent =
        [
            "dotnet test Toolroom/WhenItFails/Setter.Tests",
            "one implementation or documentation change",
            "one corresponding test change",
            "one temporary workspace per write test",
            "response and issue contract",
            "persisted catalog state",
            "backup side effects",
            "`--json`",
            "`--plain`",
            "rich output",
            "exit codes",
            "check-doc-keys",
            "check-doc-links",
            "git diff --check",
            "Do not continue while the focused Setter test run is red."
        ];

        foreach (string expected in requiredContent)
        {
            Assert.Contains(expected, documentation, StringComparison.Ordinal);
        }

        Assert.DoesNotContain(
            "Current Setter tests verify editing behavior such as:",
            documentation,
            StringComparison.Ordinal);
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
