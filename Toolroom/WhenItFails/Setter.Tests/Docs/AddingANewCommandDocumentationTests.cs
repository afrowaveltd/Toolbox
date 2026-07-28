namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Docs;

public sealed class AddingANewCommandDocumentationTests
{
    [Fact]
    public void Documentation_DescribesCurrentCommandAuthoringWorkflow()
    {
        string repositoryRoot = FindRepositoryRoot();
        string documentationPath = Path.Combine(
            repositoryRoot,
            "Toolroom",
            "WhenItFails",
            "Setter",
            "Docs",
            "Adding a New Command",
            "en.md");
        string documentation = File.ReadAllText(documentationPath);

        string[] requiredContent =
        [
            "# Adding a New Command",
            "Treat every command as a public contract.",
            "command dispatch",
            "service layer",
            "`CommandInputError`",
            "`--plain`",
            "`--json`",
            "exit code",
            "issue code",
            "dotnet test Toolroom/WhenItFails/Setter.Tests",
            "`README.md`",
            "`Docs/Commands/en.md`",
            "`Docs/Command Quick Reference/en.md`",
            "`IMPLEMENTATION_STATUS.md`",
            "Do not start the next command while the focused Setter suite is red."
        ];

        foreach (string expected in requiredContent)
        {
            Assert.Contains(expected, documentation, StringComparison.Ordinal);
        }

        string[] staleContent =
        [
            "If adding a future JSON output mode",
            "Plain output is not currently JSON.",
            "Imagine a future command:"
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
