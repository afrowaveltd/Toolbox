using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class ReferenceCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithBundledReferenceCatalog_ReturnsSuccess()
    {
        int exitCode = await ReferenceCommand.ExecuteAsync();

        Assert.Equal(0, exitCode);
    }

    [Fact]
    public async Task SummarizeAsync_WithBundledReferenceCatalog_ReturnsExpectedCounts()
    {
        WhenItFailsReferenceCatalogSummarizer summarizer = new();

        WhenItFailsReferenceCatalogSummary summary =
            await summarizer.SummarizeAsync(Environment.CurrentDirectory);

        Assert.Equal(1, summary.OwnerCount);
        Assert.Equal(16, summary.CategoryCount);
        Assert.Equal(9, summary.CodeGroupCount);
        Assert.Equal(5, summary.ProfileCount);
        Assert.Equal(37, summary.ErrorCount);

        Assert.Contains("MINIMAL", summary.ProfileNames);
        Assert.Contains("CONSOLE_TOOL", summary.ProfileNames);
        Assert.Contains("WEB_API", summary.ProfileNames);
        Assert.Contains("DESKTOP_APP", summary.ProfileNames);
        Assert.Contains("FULL", summary.ProfileNames);
    }
}
