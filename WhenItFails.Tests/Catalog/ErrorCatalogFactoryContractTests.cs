using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogFactoryContractTests
{
    [Fact]
    public void Create_ShouldReturnUsableEmptyCatalog_WhenDocumentHasNoErrors()
    {
        ErrorCatalogFactory factory = new();
        ErrorCatalogDocument document = new();

        IErrorCatalog catalog = factory.Create(document);

        Assert.Empty(catalog.GetAll());
        Assert.Null(catalog.FindById("AFW-TST-0001"));
        Assert.Empty(catalog.FindByTag("test"));
    }

    [Fact]
    public void Create_ShouldSnapshotDocumentErrorsAtCreationTime()
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

        ErrorCatalogDocument document = new()
        {
            Errors = [first]
        };

        ErrorCatalogFactory factory = new();
        IErrorCatalog catalog = factory.Create(document);

        document.Errors.Add(second);

        ErrorDefinition stored = Assert.Single(catalog.GetAll());
        Assert.Same(first, stored);
        Assert.Null(catalog.FindById(second.Id));
    }
}
