using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.WhenItFails.Documentation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsDocumentationKeyGeneratorTests
{
    [Fact]
    public void Generate_WithUnusedTitle_ReturnsCanonicalBaseKey()
    {
        string key = new DocumentationKeyGenerator().Generate(
            "NETWORK",
            "Connection interrupted",
            []);

        Assert.Equal(
            "when-it-fails/errors/network/connection-interrupted",
            key);
        Assert.True(WhenItFailsDocumentationKeyFormatChecker.IsCanonical(key));
    }

    [Fact]
    public void Generate_WithDiacriticsAndSeparators_NormalizesSegments()
    {
        string key = new DocumentationKeyGenerator().Generate(
            "FILE_SYSTEM",
            "Příliš žluťoučký — soubor!",
            []);

        Assert.Equal(
            "when-it-fails/errors/file-system/prilis-zlutoucky-soubor",
            key);
        Assert.True(WhenItFailsDocumentationKeyFormatChecker.IsCanonical(key));
    }

    [Fact]
    public void Generate_WithOccupiedBaseKey_ReturnsFirstFreeNumericSuffix()
    {
        string key = new DocumentationKeyGenerator().Generate(
            "NETWORK",
            "Connection interrupted",
            [
                "when-it-fails/errors/network/connection-interrupted",
                "when-it-fails/errors/network/connection-interrupted-2",
                "when-it-fails/errors/network/connection-interrupted-4"
            ]);

        Assert.Equal(
            "when-it-fails/errors/network/connection-interrupted-3",
            key);
    }

    [Fact]
    public void Generate_TreatsExistingKeysCaseInsensitively()
    {
        string key = new DocumentationKeyGenerator().Generate(
            "NETWORK",
            "Connection interrupted",
            ["WHEN-IT-FAILS/ERRORS/NETWORK/CONNECTION-INTERRUPTED"]);

        Assert.Equal(
            "when-it-fails/errors/network/connection-interrupted-2",
            key);
    }

    [Theory]
    [InlineData("---", "Title", "categoryName")]
    [InlineData("NETWORK", "!!!", "title")]
    public void Generate_WithoutAsciiContent_ThrowsArgumentException(
        string categoryName,
        string title,
        string expectedParameterName)
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            new DocumentationKeyGenerator().Generate(
                categoryName,
                title,
                []));

        Assert.Equal(expectedParameterName, exception.ParamName);
    }
}
