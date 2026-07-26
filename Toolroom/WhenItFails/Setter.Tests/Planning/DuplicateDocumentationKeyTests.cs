using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class DuplicateDocumentationKeyTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithMissingDocumentationKey_ThrowsArgumentException(string? documentationKey)
    {
        DocumentationKeyIssue[] errors =
        [
            new("error-1", 1, "Error one", "general/shared-key"),
            new("error-2", 2, "Error two", "general/shared-key")
        ];

        ArgumentException exception = Assert.ThrowsAny<ArgumentException>(
            () => new DuplicateDocumentationKey(
                DocumentationKey: documentationKey!,
                Errors: errors));

        Assert.Equal("DocumentationKey", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullErrors_ThrowsArgumentNullException()
    {
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
            () => new DuplicateDocumentationKey(
                DocumentationKey: "general/shared-key",
                Errors: null!));

        Assert.Equal("Errors", exception.ParamName);
    }

    [Theory]
    [MemberData(nameof(TooFewErrors))]
    public void Constructor_WithFewerThanTwoErrors_ThrowsArgumentException(
        IReadOnlyList<DocumentationKeyIssue> errors)
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => new DuplicateDocumentationKey(
                DocumentationKey: "general/shared-key",
                Errors: errors));

        Assert.Equal("Errors", exception.ParamName);
    }

    public static TheoryData<IReadOnlyList<DocumentationKeyIssue>> TooFewErrors =>
        new()
        {
            Array.Empty<DocumentationKeyIssue>(),
            new DocumentationKeyIssue[]
            {
                new("error-1", 1, "Error one", "general/shared-key")
            }
        };
}
