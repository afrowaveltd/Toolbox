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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithMissingDocumentationKey_ThrowsArgumentException(string? documentationKey)
    {
        ArgumentException exception = Assert.ThrowsAny<ArgumentException>(
            () => new InvalidDocumentationKeyFormat(
                ErrorId: "error-1",
                ErrorCode: 1,
                ErrorName: "Error one",
                DocumentationKey: documentationKey!));

        Assert.Equal("DocumentationKey", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidValues_PreservesValues()
    {
        InvalidDocumentationKeyFormat invalidKey = new(
            ErrorId: "error-1",
            ErrorCode: 42,
            ErrorName: "Error one",
            DocumentationKey: "General/Invalid_Key");

        Assert.Equal("error-1", invalidKey.ErrorId);
        Assert.Equal(42, invalidKey.ErrorCode);
        Assert.Equal("Error one", invalidKey.ErrorName);
        Assert.Equal("General/Invalid_Key", invalidKey.DocumentationKey);
    }
}
