using System.Runtime.CompilerServices;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Docs;

public sealed class KnownLimitationsDocumentationTests
{
    [Fact]
    public void Documentation_DescribesOnlyCurrentBoundaries()
    {
        string documentation = File.ReadAllText(GetDocumentationPath());

        Assert.Contains("No automatic schema migration", documentation, StringComparison.Ordinal);
        Assert.Contains("No multi-file transaction", documentation, StringComparison.Ordinal);
        Assert.Contains("No full localization workflow", documentation, StringComparison.Ordinal);
        Assert.Contains("No remote catalog synchronization", documentation, StringComparison.Ordinal);
        Assert.Contains("No package publishing automation", documentation, StringComparison.Ordinal);
        Assert.Contains("No command plug-in system", documentation, StringComparison.Ordinal);
        Assert.Contains(
            "Setter should document real boundaries without describing implemented features as missing.",
            documentation,
            StringComparison.Ordinal);

        Assert.DoesNotContain("No add-error command yet", documentation, StringComparison.Ordinal);
        Assert.DoesNotContain("No remove-error command yet", documentation, StringComparison.Ordinal);
        Assert.DoesNotContain("No JSON command output yet", documentation, StringComparison.Ordinal);
        Assert.DoesNotContain("No restore-backup command yet", documentation, StringComparison.Ordinal);
        Assert.DoesNotContain("No full documentation linting", documentation, StringComparison.Ordinal);
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
            "Known Limitations",
            "en.md"));
    }
}
