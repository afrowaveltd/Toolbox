using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class DocumentationKeyCommandReportTests
{
    [Fact]
    public void Constructor_WithDifferentTotals_ThrowsArgumentException()
    {
        DocumentationKeyCheckReport keys = new(
            TotalErrors: 2,
            MissingKeys: [],
            DuplicateKeys: []);
        DocumentationKeyFormatCheckReport format = new(
            TotalErrors: 3,
            InvalidKeys: []);

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => new DocumentationKeyCommandReport(keys, format));

        Assert.Equal("format", exception.ParamName);
        Assert.Contains("same number of errors", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Constructor_WithMatchingTotals_CreatesConsistentReport()
    {
        DocumentationKeyCheckReport keys = new(
            TotalErrors: 2,
            MissingKeys: [],
            DuplicateKeys: []);
        DocumentationKeyFormatCheckReport format = new(
            TotalErrors: 2,
            InvalidKeys: []);

        DocumentationKeyCommandReport report = new(keys, format);

        Assert.Same(keys, report.Keys);
        Assert.Same(format, report.Format);
        Assert.Equal(2, report.TotalErrors);
        Assert.True(report.IsValid);
    }
}
