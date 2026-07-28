namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Docs;

public sealed class FaqDocumentationTests
{
    [Fact]
    public void Documentation_AnswersCurrentSetterQuestions()
    {
        string repositoryRoot = FindRepositoryRoot();
        string documentationPath = Path.Combine(
            repositoryRoot,
            "Toolroom",
            "WhenItFails",
            "Setter",
            "Docs",
            "FAQ",
            "en.md");
        string documentation = File.ReadAllText(documentationPath);

        string[] requiredContent =
        [
            "## Can Setter add a new error?",
            "`add-error`",
            "`remove-error`",
            "`error-references`",
            "`next-code`",
            "`suggest-doc-key`",
            "`list-backups`",
            "`restore-backup`",
            "## Does Setter support JSON output?",
            "`--json`",
            "`check-doc-keys`",
            "`check-doc-links`",
            "`explain-profile`",
            "## Does Setter migrate old schemas automatically?",
            "## Does Setter provide full localization management?"
        ];

        foreach (string expected in requiredContent)
        {
            Assert.Contains(expected, documentation, StringComparison.Ordinal);
        }

        string[] staleClaims =
        [
            "There is no `restore-backup` command yet.",
            "There is no stable `--json` output mode yet.",
            "Can I add a new error through Setter?\n\nNot yet.",
            "Does Setter verify documentation links?\n\nNot fully."
        ];

        foreach (string staleClaim in staleClaims)
        {
            Assert.DoesNotContain(staleClaim, documentation, StringComparison.Ordinal);
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
