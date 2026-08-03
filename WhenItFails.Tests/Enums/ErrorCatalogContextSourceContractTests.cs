using Afrowave.Toolbox.WhenItFails.Enums;

namespace Afrowave.Toolbox.WhenItFails.Tests.Enums;

public sealed class ErrorCatalogContextSourceContractTests
{
    [Theory]
    [InlineData(ErrorCatalogContextSource.ProjectCatalog, 0)]
    [InlineData(ErrorCatalogContextSource.PreviousContext, 1)]
    [InlineData(ErrorCatalogContextSource.BuiltInDefaults, 2)]
    public void Values_ShouldRemainStable(
        ErrorCatalogContextSource source,
        int expectedValue)
    {
        Assert.Equal(expectedValue, (int)source);
    }
}
