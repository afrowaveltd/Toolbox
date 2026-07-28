using System.Runtime.CompilerServices;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Docs;

public sealed class RoadmapDocumentationTests
{
    [Fact]
    public void Documentation_SeparatesCompletedFoundationFromFutureWork()
    {
        string documentation = File.ReadAllText(GetDocumentationPath());

        Assert.Contains("## Completed foundation", documentation, StringComparison.Ordinal);
        Assert.Contains("profile explanation", documentation, StringComparison.Ordinal);
        Assert.Contains("timestamped backup listing and validated restore", documentation, StringComparison.Ordinal);
        Assert.Contains("documentation-link and documentation-key checks", documentation, StringComparison.Ordinal);
        Assert.Contains("## Schema migration", documentation, StringComparison.Ordinal);
        Assert.Contains("## Localization workflow", documentation, StringComparison.Ordinal);
        Assert.Contains("## Generated schemas and editor integration", documentation, StringComparison.Ordinal);
        Assert.Contains("## Import and export", documentation, StringComparison.Ordinal);
        Assert.DoesNotContain("A future `add-error` command", documentation, StringComparison.Ordinal);
        Assert.DoesNotContain("A future restore command", documentation, StringComparison.Ordinal);
        Assert.DoesNotContain("A future JSON output mode", documentation, StringComparison.Ordinal);
        Assert.DoesNotContain("1. `list-profiles`", documentation, StringComparison.Ordinal);
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
            "Roadmap and Future Work",
            "en.md"));
    }
}
