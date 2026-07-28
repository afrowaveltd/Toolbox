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
            "Last updated: 2026-07-28",
            "## Current state",
            "## Verification status",
            "1,224 tests",
            "## Documentation synchronization completed",
            "Docs/Commands/en.md",
            "Docs/Known Limitations/en.md",
            "Docs/Roadmap and Future Work/en.md",
            "Docs/Getting-Started/en.md",
            "Docs/FAQ/en.md",
            "## Current intentional boundaries",
            "## Working rules",
            "GitHub `master` is the source of truth.",
            "## Recommended next step",
            "Docs/Testing and CI/en.md",
            "runtime/public-API audit",
            "## Last completed change",
            "1b770d7506b92c76caf3c3e0f7766133f9fb7a14"
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
