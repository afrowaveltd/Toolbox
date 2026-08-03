using Afrowave.Toolbox.WhenItFails.Enums;

namespace Afrowave.Toolbox.WhenItFails.Tests.Enums;

public sealed class ErrorCatalogRuntimeStateContractTests
{
    [Theory]
    [InlineData(ErrorCatalogRuntimeState.Unknown, 0)]
    [InlineData(ErrorCatalogRuntimeState.ProjectCatalog, 1)]
    [InlineData(ErrorCatalogRuntimeState.PreviousContextRecovery, 2)]
    [InlineData(ErrorCatalogRuntimeState.BuiltInFallback, 3)]
    [InlineData(ErrorCatalogRuntimeState.BuiltInDefaults, 4)]
    public void Values_ShouldKeepStableNumericContracts(
        ErrorCatalogRuntimeState state,
        int expectedValue)
    {
        Assert.Equal(expectedValue, (int)state);
    }
}
