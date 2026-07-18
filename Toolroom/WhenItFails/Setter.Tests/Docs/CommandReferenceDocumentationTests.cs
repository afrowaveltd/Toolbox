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
