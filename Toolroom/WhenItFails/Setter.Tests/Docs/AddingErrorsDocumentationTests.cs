using System.Runtime.CompilerServices;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Docs;

public sealed class AddingErrorsDocumentationTests
{
    [Fact]
    public void Documentation_DescribesSuggestDocumentationKeyJsonContract()
    {
        string documentation = File.ReadAllText(GetDocumentationPath());

        Assert.Contains("suggest-doc-key", documentation, StringComparison.Ordinal);
        Assert.Contains("category", documentation, StringComparison.Ordinal);
        Assert.Contains("title", documentation, StringComparison.Ordinal);
        Assert.Contains("documentationKey", documentation, StringComparison.Ordinal);
        Assert.Contains("failureCode", documentation, StringComparison.Ordinal);
        Assert.Contains("failureMessage", documentation, StringComparison.Ordinal);
        Assert.Contains("On success", documentation, StringComparison.Ordinal);
        Assert.Contains("On failure", documentation, StringComparison.Ordinal);
        Assert.Contains("exit code `0`", documentation, StringComparison.Ordinal);
        Assert.Contains("`1` for invalid command arguments", documentation, StringComparison.Ordinal);
        Assert.Contains("`2` when the workspace lookup or suggestion fails", documentation, StringComparison.Ordinal);
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
            "Adding Errors",
            "en.md"));
    }
}
