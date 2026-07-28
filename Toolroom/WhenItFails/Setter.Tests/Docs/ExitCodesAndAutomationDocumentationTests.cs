namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Docs;

public sealed class ExitCodesAndAutomationDocumentationTests
{
    [Fact]
    public void Documentation_DescribesCurrentAutomationContract()
    {
        string repositoryRoot = FindRepositoryRoot();
        string documentationPath = Path.Combine(
            repositoryRoot,
            "Toolroom",
            "WhenItFails",
            "Setter",
            "Docs",
            "Exit Codes and Automation",
            "en.md");
        string documentation = File.ReadAllText(documentationPath);

        string[] requiredContent =
        [
            "# Exit Codes and Automation",
            "Exit code `0`",
            "Exit code `1`",
            "Exit code `2`",
            "Exit code `3`",
            "`--json`",
            "`--plain`",
            "Do not parse rich terminal output",
            "capture the exit code immediately",
            "set -euo pipefail",
            "$LASTEXITCODE",
            "dotnet test Toolroom/WhenItFails/Setter.Tests",
            "Machine consumers should use JSON and the process exit code together."
        ];

        foreach (string expected in requiredContent)
        {
            Assert.Contains(expected, documentation, StringComparison.Ordinal);
        }

        string[] staleContent =
        [
            "Current edit commands:",
            "set-title\nset-message\nset-developer-hint\nset-severity\nset-documentation-key",
            "The demo command is intended for manually showing sample validation output."
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
