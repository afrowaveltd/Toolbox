using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogEmptyLookupContractTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void MultiValueLookups_ShouldReturnEmptyLists_ForUnusableKeys(
        string key)
    {
        ErrorCatalog catalog = new(Array.Empty<ErrorDefinition>());

        Assert.Empty(catalog.FindByOwner(key));
        Assert.Empty(catalog.FindByCodePrefix(key));
        Assert.Empty(catalog.FindByCodeGroup(key));
        Assert.Empty(catalog.FindByCategory(key));
        Assert.Empty(catalog.FindBySubcategory(key));
        Assert.Empty(catalog.FindByTag(key));
    }
}
