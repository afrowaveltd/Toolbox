namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Docs;

public sealed class ImplementationStatusDocumentationTests
{
    [Fact]
    public void Documentation_ProvidesCurrentContinuationPoint()
    {
        string repositoryRoot = FindRepositoryRoot();
        string documentationPath = Path.Combine(
            repositoryRoot,
            "Toolroom",
            "WhenItFails",
            "Setter",
            "IMPLEMENTATION_STATUS.md");
        string documentation = File.ReadAllText(documentationPath);

        string[] requiredContent =
        [
            "# Implementation status",
            "Last updated:",
            "## Current state",
            "## Verification status",
            "The latest user-verified Setter test run",
            "dotnet test Toolroom/WhenItFails/Setter.Tests",
            "## Documentation synchronization completed",
            "Docs/Commands/en.md",
            "Docs/Known Limitations/en.md",
            "Docs/Roadmap and Future Work/en.md",
            "Docs/Getting-Started/en.md",
            "Docs/FAQ/en.md",
            "Docs/Testing and CI/en.md",
            "Docs/Reviewing Catalog Changes/en.md",
            "## Current intentional boundaries",
            "## Working rules",
            "GitHub `master` is the source of truth.",
            "## Recommended next step",
            "Next documentation target:",
            "runtime/public-API audit",
            "## Last completed change"
        ];

        foreach (string expected in requiredContent)
        {
            Assert.Contains(expected, documentation, StringComparison.Ordinal);
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
