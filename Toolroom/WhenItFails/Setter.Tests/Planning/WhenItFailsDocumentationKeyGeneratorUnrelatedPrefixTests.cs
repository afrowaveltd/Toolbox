namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsDocumentationKeyGeneratorUnrelatedPrefixTests
{
    [Fact]
    public void Generate_WhenOnlyLongerPrefixedKeyExists_ReturnsUnsuffixedBaseKey()
    {
        WhenItFailsDocumentationKeyGenerator generator = new();

        string result = generator.Generate(
            "NETWORK",
            "Connection timeout",
            ["when-it-fails/errors/network/connection-timeout-extra"]);

        Assert.Equal(
            "when-it-fails/errors/network/connection-timeout",
            result);
    }
}
