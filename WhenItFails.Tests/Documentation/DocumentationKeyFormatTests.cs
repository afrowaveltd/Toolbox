using Afrowave.Toolbox.WhenItFails.Documentation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Documentation;

public sealed class DocumentationKeyFormatTests
{
    [Theory]
    [InlineData("when-it-fails/errors/network/connection-timeout")]
    [InlineData("category/item")]
    [InlineData("a1/b2-c3")]
    public void IsCanonical_WithCanonicalKey_ReturnsTrue(string documentationKey)
    {
        Assert.True(DocumentationKeyFormat.IsCanonical(documentationKey));
    }

    [Theory]
    [InlineData("")]
    [InlineData("single-segment")]
    [InlineData(" leading/segment")]
    [InlineData("trailing/segment ")]
    [InlineData("UPPER/case")]
    [InlineData("under_score/key")]
    [InlineData("dot.segment/key")]
    [InlineData("double--hyphen/key")]
    [InlineData("leading-/segment")]
    [InlineData("-leading/segment")]
    [InlineData("empty//segment")]
    [InlineData("unicode/žluťoučký")]
    public void IsCanonical_WithNonCanonicalKey_ReturnsFalse(string documentationKey)
    {
        Assert.False(DocumentationKeyFormat.IsCanonical(documentationKey));
    }

    [Fact]
    public void IsCanonical_WithNullKey_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            DocumentationKeyFormat.IsCanonical(null!));
    }
}
