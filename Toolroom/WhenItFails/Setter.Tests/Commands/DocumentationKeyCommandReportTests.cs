using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Commands;

public sealed class DocumentationKeyCommandReportTests
{
    [Fact]
    public void Constructor_WithNullKeys_ThrowsArgumentNullException()
    {
        DocumentationKeyFormatCheckReport format = new(
            totalErrors: 0,
            invalidKeys: []);

        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => new DocumentationKeyCommandReport(null!, format));

        Assert.Equal("keys", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullFormat_ThrowsArgumentNullException()
    {
        DocumentationKeyCheckReport keys = new(
            TotalErrors: 0,
            MissingKeys: [],
            DuplicateKeys: []);

        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => new DocumentationKeyCommandReport(keys, null!));

        Assert.Equal("format", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithDifferentTotals_ThrowsArgumentException()
    {
        DocumentationKeyCheckReport keys = new(
            TotalErrors: 2,
            MissingKeys: [],
            DuplicateKeys: []);
        DocumentationKeyFormatCheckReport format = new(
            totalErrors: 3,
            invalidKeys: []);

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => new DocumentationKeyCommandReport(keys, format));

        Assert.Equal("format", exception.ParamName);
        Assert.Contains("same number of errors", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Constructor_WithAnyInvalidComponent_CreatesInvalidReport(
        bool invalidKeys,
        bool invalidFormat)
    {
        DocumentationKeyIssue[] missingKeys = invalidKeys
            ? [new DocumentationKeyIssue("error-1", 1, "Error one", null)]
            : [];
        InvalidDocumentationKeyFormat[] invalidFormats = invalidFormat
            ? [new InvalidDocumentationKeyFormat("error-1", 1, "Error one", "Invalid/Key")]
            : [];
        DocumentationKeyCheckReport keys = new(
            TotalErrors: 1,
            MissingKeys: missingKeys,
            DuplicateKeys: []);
        DocumentationKeyFormatCheckReport format = new(
            totalErrors: 1,
            invalidKeys: invalidFormats);

        DocumentationKeyCommandReport report = new(keys, format);

        Assert.False(report.IsValid);
    }

    [Fact]
    public void Constructor_WithMatchingTotals_CreatesConsistentReport()
    {
        DocumentationKeyCheckReport keys = new(
            TotalErrors: 2,
            MissingKeys: [],
            DuplicateKeys: []);
        DocumentationKeyFormatCheckReport format = new(
            totalErrors: 2,
            invalidKeys: []);

        DocumentationKeyCommandReport report = new(keys, format);

        Assert.Same(keys, report.Keys);
        Assert.Same(format, report.Format);
        Assert.Equal(2, report.TotalErrors);
        Assert.True(report.IsValid);
    }
}
