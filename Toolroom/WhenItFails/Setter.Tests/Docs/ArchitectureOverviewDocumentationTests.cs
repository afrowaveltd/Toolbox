namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Docs;

public sealed class ArchitectureOverviewDocumentationTests
{
    [Fact]
    public void Documentation_DescribesCurrentSetterArchitecture()
    {
        string repositoryRoot = FindRepositoryRoot();
        string documentationPath = Path.Combine(
            repositoryRoot,
            "Toolroom",
            "WhenItFails",
            "Setter",
            "Docs",
            "Architecture Overview",
            "en.md");
        string documentation = File.ReadAllText(documentationPath);

        string[] requiredContent =
        [
            "# Architecture Overview",
            "Commands orchestrate; services implement reusable behavior; views render.",
            "## Entry point and dispatch",
            "## Command layer",
            "## Service layer",
            "## Workspace and catalog models",
            "## Validation and structured failures",
            "## Persistence and recovery",
            "## Output boundaries",
            "`--json`",
            "single-file safe write",
            "not a multi-file transaction",
            "dotnet test Toolroom/WhenItFails/Setter.Tests",
            "Models do not render themselves.",
            "Views do not decide command semantics."
        ];

        foreach (string expected in requiredContent)
        {
            Assert.Contains(expected, documentation, StringComparison.Ordinal);
        }

        string[] staleContent =
        [
            "If adding a future output mode such as JSON",
            "JSON output would be a machine-readable contract.",
            "Neither should be confused with a stable JSON API unless a JSON mode is explicitly added later.",
            "making profile browsing look runtime-complete when it is not"
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
