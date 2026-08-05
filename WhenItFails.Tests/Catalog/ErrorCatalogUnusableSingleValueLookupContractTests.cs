using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogUnusableSingleValueLookupContractTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FindById_ShouldReturnNull_ForUnusableKeys(string? key)
    {
        ErrorCatalog catalog = CreateCatalog();

        ErrorDefinition? result = catalog.FindById(key!);

        Assert.Null(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FindByName_ShouldReturnNull_ForUnusableKeys(string? key)
    {
        ErrorCatalog catalog = CreateCatalog();

        ErrorDefinition? result = catalog.FindByName(key!);

        Assert.Null(result);
    }

    private static ErrorCatalog CreateCatalog()
    {
        return new ErrorCatalog(
        [
            new ErrorDefinition
            {
                Id = "AFW-TST-0001",
                Name = "TestError"
            }
        ]);
    }
}
