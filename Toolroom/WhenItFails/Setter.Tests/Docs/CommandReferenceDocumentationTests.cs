using System.Runtime.CompilerServices;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Docs;

public sealed class CommandReferenceDocumentationTests
{
    [Fact]
    public void Documentation_DescribesSuggestDocumentationKeyCommand()
    {
        string documentation = File.ReadAllText(GetDocumentationPath());

        Assert.Contains(
            "suggest-doc-key <path> <category-name|alias> <title> [--plain|--json]",
            documentation,
            StringComparison.Ordinal);
        Assert.Contains("The command is read-only.", documentation, StringComparison.Ordinal);
        Assert.Contains("documentationKey", documentation, StringComparison.Ordinal);
        Assert.Contains("failureCode", documentation, StringComparison.Ordinal);
        Assert.Contains("failureMessage", documentation, StringComparison.Ordinal);
        Assert.Contains("0  suggestion produced", documentation, StringComparison.Ordinal);
        Assert.Contains("1  command arguments were invalid", documentation, StringComparison.Ordinal);
        Assert.Contains(
            "2  workspace loading, category lookup, or key generation failed",
            documentation,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Documentation_ListsCurrentCommandGroups()
    {
        string documentation = File.ReadAllText(GetDocumentationPath());

        string[] expectedCommands =
        [
            "reference <path>",
            "next-code <path> <owner> <code-group>",
            "add-error <path> ...",
            "remove-error <path> <id|code|name>",
            "error-references <path> <id|code|name>",
            "explain-profile <path> <profile-name> [--plain|--json]",
            "profile-set-default-mapping",
            "list-backups <path> [--plain|--json]",
            "restore-backup <path> <backup-file> [--plain|--json]",
            "check-doc-links <path> [--plain|--json]",
            "check-doc-keys <path> [--plain|--json]"
        ];

        foreach (string expectedCommand in expectedCommands)
        {
            Assert.Contains(
                expectedCommand,
                documentation,
                StringComparison.Ordinal);
        }

        Assert.Contains("## Error lifecycle", documentation, StringComparison.Ordinal);
        Assert.Contains("## Backups", documentation, StringComparison.Ordinal);
        Assert.Contains("## Documentation checks", documentation, StringComparison.Ordinal);
    }

    private static string GetDocumentationPath(
        [CallerFilePath] string sourceFilePath = "")
    {
        string sourceDirectory = Path.GetDirectoryName(sourceFilePath)
            ?? throw new InvalidOperationException("The test source directory could not be resolved.");

        return Path.GetFullPath(Path.Combine(
            sourceDirectory,
            "..",
            "..",
            "Setter",
            "Docs",
            "Commands",
            "en.md"));
    }
}
