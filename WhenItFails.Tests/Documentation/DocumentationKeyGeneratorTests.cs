using Afrowave.Toolbox.WhenItFails.Documentation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Documentation;

public sealed class DocumentationKeyGeneratorTests
{
    [Fact]
    public void Generate_WithSimpleCategoryAndTitle_ReturnsCanonicalKey()
    {
        DocumentationKeyGenerator generator = new();

        string result = generator.Generate(
            "NETWORK",
            "Connection timeout",
            []);

        Assert.Equal(
            "when-it-fails/errors/network/connection-timeout",
            result);
    }

    [Fact]
    public void Generate_WithDiacritics_RemovesDiacritics()
    {
        DocumentationKeyGenerator generator = new();

        string result = generator.Generate(
            "PŘIPOJENÍ",
            "Časový limit připojení",
            []);

        Assert.Equal(
            "when-it-fails/errors/pripojeni/casovy-limit-pripojeni",
            result);
    }

    [Fact]
    public void Generate_WithExistingBaseAndSecondSuffix_ReturnsThirdSuffix()
    {
        DocumentationKeyGenerator generator = new();

        string result = generator.Generate(
            "NETWORK",
            "Connection timeout",
            [
                "when-it-fails/errors/network/connection-timeout",
                "when-it-fails/errors/network/connection-timeout-2"
            ]);

        Assert.Equal(
            "when-it-fails/errors/network/connection-timeout-3",
            result);
    }

    [Fact]
    public void Generate_WithGapInNumericSuffixes_ReturnsFirstFreeSuffix()
    {
        DocumentationKeyGenerator generator = new();

        string result = generator.Generate(
            "NETWORK",
            "Connection timeout",
            [
                "when-it-fails/errors/network/connection-timeout",
                "when-it-fails/errors/network/connection-timeout-2",
                "when-it-fails/errors/network/connection-timeout-4"
            ]);

        Assert.Equal(
            "when-it-fails/errors/network/connection-timeout-3",
            result);
    }

    [Fact]
    public void Generate_WithDifferentExistingKeyCasing_TreatsItAsCollision()
    {
        DocumentationKeyGenerator generator = new();

        string result = generator.Generate(
            "NETWORK",
            "Connection timeout",
            ["WHEN-IT-FAILS/ERRORS/NETWORK/CONNECTION-TIMEOUT"]);

        Assert.Equal(
            "when-it-fails/errors/network/connection-timeout-2",
            result);
    }

    [Fact]
    public void Generate_WithWhitespaceAroundExistingKey_TreatsItAsCollision()
    {
        DocumentationKeyGenerator generator = new();

        string result = generator.Generate(
            "NETWORK",
            "Connection timeout",
            ["  when-it-fails/errors/network/connection-timeout\t"]);

        Assert.Equal(
            "when-it-fails/errors/network/connection-timeout-2",
            result);
    }

    [Fact]
    public void Generate_WithNullOrBlankExistingKeys_IgnoresThem()
    {
        DocumentationKeyGenerator generator = new();

        string result = generator.Generate(
            "NETWORK",
            "Connection timeout",
            [null, string.Empty, "   ", "\t"]);

        Assert.Equal(
            "when-it-fails/errors/network/connection-timeout",
            result);
    }

    [Fact]
    public void Generate_WithOnlyLongerPrefixedKey_ReturnsUnsuffixedBaseKey()
    {
        DocumentationKeyGenerator generator = new();

        string result = generator.Generate(
            "NETWORK",
            "Connection timeout",
            ["when-it-fails/errors/network/connection-timeout-extra"]);

        Assert.Equal(
            "when-it-fails/errors/network/connection-timeout",
            result);
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

    [Theory]
    [InlineData("  FILE_SYSTEM  ", "file-system")]
    [InlineData("Příliš žluťoučký — soubor!", "prilis-zlutoucky-soubor")]
    [InlineData("Already--Separated", "already-separated")]
    public void ToSegment_WithHumanReadableValue_ReturnsCanonicalSegment(
        string value,
        string expected)
    {
        string result = DocumentationKeyGenerator.ToSegment(value);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToSegment_WithNullValue_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            DocumentationKeyGenerator.ToSegment(null!));
    }
}
