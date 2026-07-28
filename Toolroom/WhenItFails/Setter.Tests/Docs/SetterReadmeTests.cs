using System.Runtime.CompilerServices;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Docs;

public sealed class SetterReadmeTests
{
    [Fact]
    public void Readme_IntroducesSuggestDocumentationKeyCommand()
    {
        string readme = File.ReadAllText(GetReadmePath());

        Assert.Contains("suggest-doc-key", readme, StringComparison.Ordinal);
        Assert.Contains("<category-name\\|alias>", readme, StringComparison.Ordinal);
        Assert.Contains("--plain\\|--json", readme, StringComparison.Ordinal);
        Assert.Contains("The command is read-only.", readme, StringComparison.Ordinal);
        Assert.Contains(
            "[Adding errors](Docs/Adding%20Errors/en.md)",
            readme,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Readme_DescribesCurrentAuthoringScope()
    {
        string readme = File.ReadAllText(GetReadmePath());

        Assert.Contains("add-error", readme, StringComparison.Ordinal);
        Assert.Contains("remove-error", readme, StringComparison.Ordinal);
        Assert.Contains("restore-backup", readme, StringComparison.Ordinal);
        Assert.Contains("profile-set-default-mapping", readme, StringComparison.Ordinal);
        Assert.Contains("check-doc-links", readme, StringComparison.Ordinal);
        Assert.Contains("check-doc-keys", readme, StringComparison.Ordinal);
        Assert.Contains(
            "complete day-to-day authoring workflow",
            readme,
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "Setter currently edits selected presentation and diagnostic fields",
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
