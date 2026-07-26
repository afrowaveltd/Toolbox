using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class DocumentationKeyCheckReportTests
{
    [Fact]
    public void Constructor_WithNullMissingKeys_ThrowsArgumentNullException()
    {
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => new DocumentationKeyCheckReport(
                TotalErrors: 0,
                MissingKeys: null!,
                DuplicateKeys: []));

        Assert.Equal("MissingKeys", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullDuplicateKeys_ThrowsArgumentNullException()
    {
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => new DocumentationKeyCheckReport(
                TotalErrors: 0,
                MissingKeys: [],
                DuplicateKeys: null!));

        Assert.Equal("DuplicateKeys", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNegativeTotalErrors_ThrowsArgumentOutOfRangeException()
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new DocumentationKeyCheckReport(
                TotalErrors: -1,
                MissingKeys: [],
                DuplicateKeys: []));

        Assert.Equal("TotalErrors", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithMoreMissingKeysThanErrors_ThrowsArgumentException()
    {
        DocumentationKeyIssue missingKey = new(
            ErrorId: "error-1",
            ErrorCode: 1,
            ErrorName: "Error one",
            DocumentationKey: null);

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => new DocumentationKeyCheckReport(
                TotalErrors: 0,
                MissingKeys: [missingKey],
                DuplicateKeys: []));

        Assert.Equal("MissingKeys", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullMissingKey_ThrowsArgumentException()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => new DocumentationKeyCheckReport(
                TotalErrors: 1,
                MissingKeys: [null!],
                DuplicateKeys: []));

        Assert.Equal("MissingKeys", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithMoreDuplicateKeysThanErrors_ThrowsArgumentException()
    {
        const string documentationKey = "general/shared-error";
        DuplicateDocumentationKey duplicateKey = new(
            DocumentationKey: documentationKey,
            Errors:
            [
                new DocumentationKeyIssue(
                    ErrorId: "error-1",
                    ErrorCode: 1,
                    ErrorName: "Error one",
                    DocumentationKey: documentationKey),
                new DocumentationKeyIssue(
                    ErrorId: "error-2",
                    ErrorCode: 2,
                    ErrorName: "Error two",
                    DocumentationKey: documentationKey)
            ]);

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => new DocumentationKeyCheckReport(
                TotalErrors: 0,
                MissingKeys: [],
                DuplicateKeys: [duplicateKey]));

        Assert.Equal("DuplicateKeys", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullDuplicateKey_ThrowsArgumentException()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => new DocumentationKeyCheckReport(
                TotalErrors: 1,
                MissingKeys: [],
                DuplicateKeys: [null!]));

        Assert.Equal("DuplicateKeys", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithMissingKeyCountEqualToTotalErrors_CreatesReport()
    {
        DocumentationKeyIssue missingKey = new(
            ErrorId: "error-1",
            ErrorCode: 1,
            ErrorName: "Error one",
            DocumentationKey: null);
        DocumentationKeyIssue[] missingKeys = [missingKey];
        DuplicateDocumentationKey[] duplicateKeys = [];

        DocumentationKeyCheckReport report = new(
            TotalErrors: 1,
            MissingKeys: missingKeys,
            DuplicateKeys: duplicateKeys);

        Assert.Equal(1, report.TotalErrors);
        Assert.Same(missingKeys, report.MissingKeys);
        Assert.Same(duplicateKeys, report.DuplicateKeys);
        Assert.False(report.IsValid);
    }
}
