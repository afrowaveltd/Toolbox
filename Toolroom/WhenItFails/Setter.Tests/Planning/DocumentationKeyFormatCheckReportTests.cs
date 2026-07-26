using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class DocumentationKeyFormatCheckReportTests
{
    [Fact]
    public void Constructor_WithNullInvalidKeys_ThrowsArgumentNullException()
    {
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => new DocumentationKeyFormatCheckReport(
                totalErrors: 0,
                invalidKeys: null!));

        Assert.Equal("invalidKeys", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNegativeTotalErrors_ThrowsArgumentOutOfRangeException()
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new DocumentationKeyFormatCheckReport(
                totalErrors: -1,
                invalidKeys: []));

        Assert.Equal("totalErrors", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithMoreInvalidKeysThanErrors_ThrowsArgumentException()
    {
        InvalidDocumentationKeyFormat invalidKey = new(
            ErrorId: "error-1",
            ErrorCode: 1,
            ErrorName: "Error one",
            DocumentationKey: "General/Invalid_Key");

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => new DocumentationKeyFormatCheckReport(
                totalErrors: 0,
                invalidKeys: [invalidKey]));

        Assert.Equal("invalidKeys", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithInvalidKeyCountEqualToTotalErrors_CreatesReport()
    {
        InvalidDocumentationKeyFormat invalidKey = new(
            ErrorId: "error-1",
            ErrorCode: 1,
            ErrorName: "Error one",
            DocumentationKey: "General/Invalid_Key");
        InvalidDocumentationKeyFormat[] invalidKeys = [invalidKey];

        DocumentationKeyFormatCheckReport report = new(
            totalErrors: 1,
            invalidKeys: invalidKeys);

        Assert.Equal(1, report.TotalErrors);
        Assert.Same(invalidKeys, report.InvalidKeys);
        Assert.False(report.IsValid);
    }
}
