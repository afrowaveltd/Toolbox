using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogMultiValueIndexContractTests
{
    [Fact]
    public void FindByTag_ShouldPreserveSourceDefinitionOrder()
    {
        ErrorDefinition first = new()
        {
            Id = "AFW-TST-0001",
            Name = "FirstTestError",
            Tags = ["shared"]
        };

        ErrorDefinition second = new()
        {
            Id = "AFW-TST-0002",
            Name = "SecondTestError",
            Tags = ["shared"]
        };

        ErrorCatalog catalog = new([first, second]);

        IReadOnlyList<ErrorDefinition> results = catalog.FindByTag("shared");

        Assert.Equal(2, results.Count);
        Assert.Same(first, results[0]);
        Assert.Same(second, results[1]);
    }

    [Fact]
    public void FindByTag_ShouldNotDuplicateDefinition_ForRepeatedNormalizedValues()
    {
        ErrorDefinition definition = new()
        {
            Id = "AFW-TST-0001",
            Name = "RepeatedTagTestError",
            Tags = ["user-visible", "USER VISIBLE", " user-visible "]
        };

        ErrorCatalog catalog = new([definition]);

        ErrorDefinition result = Assert.Single(catalog.FindByTag("user visible"));

        Assert.Same(definition, result);
    }
}
