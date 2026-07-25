using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class DocumentationKeyReportValidationTests
{
    [Fact]
    public void CheckReport_WithNullMissingKeys_ThrowsArgumentNullException()
    {
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => new DocumentationKeyCheckReport(
                totalErrors: 0,
                missingKeys: null!,
                duplicateKeys: []));

        Assert.Equal("missingKeys", exception.ParamName);
    }

    [Fact]
    public void CheckReport_WithNullDuplicateKeys_ThrowsArgumentNullException()
    {
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => new DocumentationKeyCheckReport(
                totalErrors: 0,
                missingKeys: [],
                duplicateKeys: null!));

        Assert.Equal("duplicateKeys", exception.ParamName);
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
