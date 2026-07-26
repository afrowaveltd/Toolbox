using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class DocumentationKeyIssueTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithMissingErrorId_ThrowsArgumentException(string? errorId)
    {
        ArgumentException exception = Assert.ThrowsAny<ArgumentException>(
            () => new DocumentationKeyIssue(
                ErrorId: errorId!,
                ErrorCode: 1,
                ErrorName: "Error one",
                DocumentationKey: null));

        Assert.Equal("ErrorId", exception.ParamName);
    }
}
