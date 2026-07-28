using System.Runtime.CompilerServices;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Docs;

public sealed class CommandQuickReferenceDocumentationTests
{
    [Fact]
    public void Documentation_ListsCurrentCommandFamilies()
    {
        string documentation = File.ReadAllText(GetDocumentationPath());

        string[] expectedCommands =
        [
            "reference",
            "next-code",
            "add-error",
            "remove-error",
            "error-references",
            "explain-profile",
            "profile-set-default-mapping",
            "restore-backup",
            "check-doc-links",
            "check-doc-keys"
        ];

        foreach (string command in expectedCommands)
        {
            Assert.Contains(command, documentation, StringComparison.Ordinal);
        }

        Assert.Contains("## Error lifecycle", documentation, StringComparison.Ordinal);
        Assert.Contains("## Backups", documentation, StringComparison.Ordinal);
        Assert.Contains("## Documentation checks", documentation, StringComparison.Ordinal);
        Assert.Contains(
            "[Complete command reference](../Commands/en.md)",
            documentation,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "update selected fields in errors.en.json",
            documentation,
            StringComparison.Ordinal);
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
            "Command Quick Reference",
            "en.md"));
    }
}
