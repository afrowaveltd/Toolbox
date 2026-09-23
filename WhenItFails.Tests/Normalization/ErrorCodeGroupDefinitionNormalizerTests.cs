using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.WhenItFails.Tests.Normalization;

public sealed class ErrorCodeGroupDefinitionNormalizerTests
{
    [Fact]
    public void Normalize_ShouldThrowArgumentNullException_WhenDefinitionIsNull()
    {
        ErrorCodeGroupDefinitionNormalizer normalizer = new();

        Assert.Throws<ArgumentNullException>(
            () => normalizer.Normalize(null!));
    }
}
