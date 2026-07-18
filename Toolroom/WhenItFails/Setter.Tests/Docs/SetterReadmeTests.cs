using System.Runtime.CompilerServices;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Docs;

public sealed class SetterReadmeTests
{
    [Fact]
    public void Readme_IntroducesSuggestDocumentationKeyCommand()
    {
        string readme = File.ReadAllText(GetReadmePath());

        Assert.Contains(
            "suggest-doc-key <path> <category-name|alias> <title> [--plain|--json]",
            readme,
            StringComparison.Ordinal);
        Assert.Contains("The command is read-only.", readme, StringComparison.Ordinal);
        Assert.Contains(
            "[Adding errors](Docs/Adding%20Errors/en.md)",
            readme,
            StringComparison.Ordinal);
    }

    private static string GetReadmePath(
        [CallerFilePath] string sourceFilePath = "")
    {
        string sourceDirectory = Path.GetDirectoryName(sourceFilePath)
            ?? throw new InvalidOperationException("The test source directory could not be resolved.");

        return Path.GetFullPath(Path.Combine(
            sourceDirectory,
            "..",
            "..",
            "Setter",
            "README.md"));
    }
}
