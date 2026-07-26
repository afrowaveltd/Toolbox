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
}
