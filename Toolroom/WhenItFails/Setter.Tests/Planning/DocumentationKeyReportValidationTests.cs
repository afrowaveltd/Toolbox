using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class DocumentationKeyReportValidationTests
{
    [Fact]
    public void CheckReport_WithNegativeTotalErrors_ThrowsArgumentOutOfRangeException()
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new DocumentationKeyCheckReport(
                TotalErrors: -1,
                MissingKeys: [],
                DuplicateKeys: []));

        Assert.Equal("TotalErrors", exception.ParamName);
    }

    [Fact]
    public void CheckReport_WithMoreMissingKeysThanTotalErrors_ThrowsArgumentException()
    {
        DocumentationKeyIssue missingKey = new(
            ErrorId: "AFW-NET-0001",
            ErrorCode: 1001,
            ErrorName: "Unavailable",
            DocumentationKey: null);

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => new DocumentationKeyCheckReport(
                TotalErrors: 0,
                MissingKeys: [missingKey],
                DuplicateKeys: []));

        Assert.Equal("MissingKeys", exception.ParamName);
        Assert.Contains("cannot exceed", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CheckReport_WithNullMissingKeys_ThrowsArgumentNullException()
    {
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => new DocumentationKeyCheckReport(
                TotalErrors: 0,
                MissingKeys: null!,
                DuplicateKeys: []));

        Assert.Equal("MissingKeys", exception.ParamName);
    }

    [Fact]
    public void CheckReport_WithNullDuplicateKeys_ThrowsArgumentNullException()
    {
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => new DocumentationKeyCheckReport(
                TotalErrors: 0,
                MissingKeys: [],
                DuplicateKeys: null!));

        Assert.Equal("DuplicateKeys", exception.ParamName);
    }

    [Fact]
    public void FormatReport_WithNegativeTotalErrors_ThrowsArgumentOutOfRangeException()
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new DocumentationKeyFormatCheckReport(
                totalErrors: -1,
                invalidKeys: []));

        Assert.Equal("totalErrors", exception.ParamName);
    }

    [Fact]
    public void FormatReport_WithMoreInvalidKeysThanTotalErrors_ThrowsArgumentException()
    {
        InvalidDocumentationKeyFormat invalidKey = new(
            ErrorId: "AFW-NET-0001",
            ErrorCode: 1001,
            ErrorName: "Unavailable",
            DocumentationKey: "Network.Unavailable");

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => new DocumentationKeyFormatCheckReport(
                totalErrors: 0,
                invalidKeys: [invalidKey]));

        Assert.Equal("invalidKeys", exception.ParamName);
        Assert.Contains("cannot exceed", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void FormatReport_WithNullInvalidKeys_ThrowsArgumentNullException()
    {
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => new DocumentationKeyFormatCheckReport(
                totalErrors: 0,
                invalidKeys: null!));

        Assert.Equal("invalidKeys", exception.ParamName);
    }
}
