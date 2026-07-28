using System.Runtime.CompilerServices;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Docs;

public sealed class GettingStartedDocumentationTests
{
    [Fact]
    public void Documentation_DescribesCurrentSafeAuthoringWorkflow()
    {
        string documentation = File.ReadAllText(GetDocumentationPath());

        string[] expectedCommands =
        [
            "init .",
            "validate .",
            "reference .",
            "next-code . NETWORK",
            "suggest-doc-key . NETWORK",
            "add-error .",
            "error-references . AFW_NET_0002",
            "explain-profile . WEB AFW_NET_0002",
            "check-doc-keys .",
            "check-doc-links .",
            "list-backups .",
            "restore-backup . <backup-file>",
            "dotnet test Toolroom/WhenItFails/Setter.Tests",
            "git diff --check"
        ];

        foreach (string command in expectedCommands)
        {
            Assert.Contains(command, documentation, StringComparison.Ordinal);
        }

        Assert.Contains(
            "Write commands validate their inputs, create a timestamped backup",
            documentation,
            StringComparison.Ordinal);
        Assert.Contains("Use `--json`", documentation, StringComparison.Ordinal);
        Assert.Contains("Do not parse rich terminal output", documentation, StringComparison.Ordinal);
        Assert.Contains(
            "> Validate first, make one explicit change, inspect it, test it, and commit it.",
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
            "Getting-Started",
            "en.md"));
    }
}
