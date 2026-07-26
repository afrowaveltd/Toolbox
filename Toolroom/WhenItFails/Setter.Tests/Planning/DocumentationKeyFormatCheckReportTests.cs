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
}
