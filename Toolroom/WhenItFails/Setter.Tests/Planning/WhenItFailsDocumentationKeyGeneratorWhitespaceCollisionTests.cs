using Afrowave.Toolbox.WhenItFails.Documentation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsDocumentationKeyGeneratorWhitespaceCollisionTests
{
    [Fact]
    public void Generate_WhenExistingKeyHasSurroundingWhitespace_TreatsItAsCollision()
    {
        DocumentationKeyGenerator generator = new();
        const string baseKey = "when-it-fails/errors/network/connection-timeout";

        string result = generator.Generate(
            "NETWORK",
            "Connection timeout",
            [$"  {baseKey}\t"]);

        Assert.Equal($"{baseKey}-2", result);
    }
}
