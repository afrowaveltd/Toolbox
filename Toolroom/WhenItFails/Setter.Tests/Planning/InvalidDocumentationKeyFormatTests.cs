using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class InvalidDocumentationKeyFormatTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithMissingErrorId_ThrowsArgumentException(string? errorId)
    {
        ArgumentException exception = Assert.ThrowsAny<ArgumentException>(
            () => new InvalidDocumentationKeyFormat(
                ErrorId: errorId!,
                ErrorCode: 1,
                ErrorName: "Error one",
                DocumentationKey: "General/Invalid_Key"));

        Assert.Equal("ErrorId", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithMissingErrorName_ThrowsArgumentException(string? errorName)
    {
        ArgumentException exception = Assert.ThrowsAny<ArgumentException>(
            () => new InvalidDocumentationKeyFormat(
                ErrorId: "error-1",
                ErrorCode: 1,
                ErrorName: errorName!,
                DocumentationKey: "General/Invalid_Key"));

        Assert.Equal("ErrorName", exception.ParamName);
    }
}
