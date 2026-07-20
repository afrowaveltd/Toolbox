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
}