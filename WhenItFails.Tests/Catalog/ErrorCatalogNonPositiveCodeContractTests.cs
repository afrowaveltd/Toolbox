using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogNonPositiveCodeContractTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void FindByCode_ShouldNotIndexNonPositiveCodes(int code)
    {
        ErrorDefinition definition = new()
        {
            Id = "AFW-TST-0001",
            Code = code,
            Name = "NonPositiveCodeError"
        };

        ErrorCatalog catalog = new([definition]);

        ErrorDefinition stored = Assert.Single(catalog.GetAll());

        Assert.Same(definition, stored);
        Assert.Null(catalog.FindByCode(code));
    }
}
