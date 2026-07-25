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
    public void CheckReport_WithMoreDuplicateKeysThanTotalErrors_ThrowsArgumentException()
    {
        const string documentationKey = "when-it-fails/errors/network/unavailable";
        DocumentationKeyIssue firstError = new(
            ErrorId: "AFW-NET-0001",
            ErrorCode: 1001,
            ErrorName: "Unavailable",
            DocumentationKey: documentationKey);
        DocumentationKeyIssue secondError = new(
            ErrorId: "AFW-NET-0002",
            ErrorCode: 1002,
            ErrorName: "Offline",
            DocumentationKey: documentationKey);
        DuplicateDocumentationKey duplicateKey = new(
            DocumentationKey: documentationKey,
            Errors: [firstError, secondError]);

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => new DocumentationKeyCheckReport(
                TotalErrors: 0,
                MissingKeys: [],
                DuplicateKeys: [duplicateKey]));

        Assert.Equal("DuplicateKeys", exception.ParamName);
        Assert.Contains("cannot exceed", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void DuplicateKey_WithFewerThanTwoErrors_ThrowsArgumentException(int errorCount)
    {
        const string documentationKey = "when-it-fails/errors/network/unavailable";
        DocumentationKeyIssue error = new(
            ErrorId: "AFW-NET-0001",
            ErrorCode: 1001,
            ErrorName: "Unavailable",
            DocumentationKey: documentationKey);
        IReadOnlyList<DocumentationKeyIssue> errors = errorCount == 0 ? [] : [error];

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => new DuplicateDocumentationKey(
                DocumentationKey: documentationKey,
                Errors: errors));

        Assert.Equal("Errors", exception.ParamName);
        Assert.Contains("at least two", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void DuplicateKey_WithEmptyDocumentationKey_ThrowsArgumentException(
        string documentationKey)
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => new DuplicateDocumentationKey(
                DocumentationKey: documentationKey,
                Errors: []));

        Assert.Equal("DocumentationKey", exception.ParamName);
    }

    [Fact]
    public void DuplicateKey_WithNullErrors_ThrowsArgumentNullException()
    {
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => new DuplicateDocumentationKey(
                DocumentationKey: "when-it-fails/errors/network/unavailable",
                Errors: null!));

        Assert.Equal("Errors", exception.ParamName);
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
