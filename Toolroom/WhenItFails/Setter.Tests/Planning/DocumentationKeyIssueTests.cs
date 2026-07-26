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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithMissingErrorName_ThrowsArgumentException(string? errorName)
    {
        ArgumentException exception = Assert.ThrowsAny<ArgumentException>(
            () => new DocumentationKeyIssue(
                ErrorId: "error-1",
                ErrorCode: 1,
                ErrorName: errorName!,
                DocumentationKey: null));

        Assert.Equal("ErrorName", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidValues_PreservesValues()
    {
        DocumentationKeyIssue issue = new(
            ErrorId: "error-1",
            ErrorCode: 42,
            ErrorName: "Error one",
            DocumentationKey: null);

        Assert.Equal("error-1", issue.ErrorId);
        Assert.Equal(42, issue.ErrorCode);
        Assert.Equal("Error one", issue.ErrorName);
        Assert.Null(issue.DocumentationKey);
    }
}
