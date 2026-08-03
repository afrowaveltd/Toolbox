using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogDuplicateSingleValueKeyContractTests
{
    [Fact]
    public void FindById_ShouldReturnFirstDefinition_WhenNormalizedIdsAreDuplicated()
    {
        ErrorDefinition first = new()
        {
            Id = "AFW-TST-0001",
            Name = "FirstDefinition",
            Code = 900001
        };

        ErrorDefinition second = new()
        {
            Id = "afw tst 0001",
            Name = "SecondDefinition",
            Code = 900002
        };

        ErrorCatalog catalog = new([first, second]);

        ErrorDefinition? result = catalog.FindById("AFW_TST_0001");

        Assert.Same(first, result);
    }

    [Fact]
    public void FindByName_ShouldReturnFirstDefinition_WhenNormalizedNamesAreDuplicated()
    {
        ErrorDefinition first = new()
        {
            Id = "AFW-TST-0001",
            Name = "Duplicate Test Name",
            Code = 900001
        };

        ErrorDefinition second = new()
        {
            Id = "AFW-TST-0002",
            Name = "duplicate-test-name",
            Code = 900002
        };

        ErrorCatalog catalog = new([first, second]);

        ErrorDefinition? result = catalog.FindByName("DUPLICATE_TEST_NAME");

        Assert.Same(first, result);
    }

    [Fact]
    public void FindByCode_ShouldReturnFirstDefinition_WhenPositiveCodesAreDuplicated()
    {
        ErrorDefinition first = new()
        {
            Id = "AFW-TST-0001",
            Name = "FirstDefinition",
            Code = 900001
        };

        ErrorDefinition second = new()
        {
            Id = "AFW-TST-0002",
            Name = "SecondDefinition",
            Code = 900001
        };

        ErrorCatalog catalog = new([first, second]);

        ErrorDefinition? result = catalog.FindByCode(900001);

        Assert.Same(first, result);
    }
}
