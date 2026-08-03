using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogSnapshotContractTests
{
    [Fact]
    public void Constructor_ShouldSnapshotTheSuppliedSequence()
    {
        ErrorDefinition first = new()
        {
            Id = "AFW-TST-0001",
            Name = "FirstTestError"
        };

        ErrorDefinition second = new()
        {
            Id = "AFW-TST-0002",
            Name = "SecondTestError"
        };

        List<ErrorDefinition> source = [first];
        ErrorCatalog catalog = new(source);

        source.Add(second);

        ErrorDefinition stored = Assert.Single(catalog.GetAll());
        Assert.Same(first, stored);
    }

    [Fact]
    public void GetAll_ShouldPreserveOrderAndExactInstances()
    {
        ErrorDefinition first = new()
        {
            Id = "AFW-TST-0001",
            Name = "FirstTestError"
        };

        ErrorDefinition second = new()
        {
            Id = "AFW-TST-0002",
            Name = "SecondTestError"
        };

        ErrorCatalog catalog = new([first, second]);

        IReadOnlyList<ErrorDefinition> errors = catalog.GetAll();

        Assert.Equal(2, errors.Count);
        Assert.Same(first, errors[0]);
        Assert.Same(second, errors[1]);
    }
}
