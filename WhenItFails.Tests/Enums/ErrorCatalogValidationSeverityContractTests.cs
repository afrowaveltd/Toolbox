using Afrowave.Toolbox.WhenItFails.Enums;

namespace Afrowave.Toolbox.WhenItFails.Tests.Enums;

public sealed class ErrorCatalogValidationSeverityContractTests
{
    [Theory]
    [InlineData(ErrorCatalogValidationSeverity.Information, 0)]
    [InlineData(ErrorCatalogValidationSeverity.Warning, 1)]
    [InlineData(ErrorCatalogValidationSeverity.Error, 2)]
    public void Severity_ShouldHaveStableNumericValue(
        ErrorCatalogValidationSeverity severity,
        int expectedValue)
    {
        Assert.Equal(expectedValue, (int)severity);
    }
}
