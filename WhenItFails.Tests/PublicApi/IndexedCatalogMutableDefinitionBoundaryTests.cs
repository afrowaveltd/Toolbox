using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class IndexedCatalogMutableDefinitionBoundaryTests
{
    [Fact]
    public void Factory_RetainsDefinitionReferenceWhileIdentityIndexKeepsOriginalKey()
    {
        ErrorDefinition definition = CreateDefinition();
        ErrorCatalogDocument document = new()
        {
            Errors = [definition]
        };

        IErrorCatalog catalog = new ErrorCatalogFactory().Create(document);
        const string originalId = "AFW-CFG-0001";
        const string changedId = "AFW-CFG-0002";

        definition.Id = changedId;
        definition.Title = "Changed after indexing";

        ErrorDefinition indexed = Assert.IsType<ErrorDefinition>(
            catalog.FindById(originalId));

        Assert.Same(definition, indexed);
        Assert.Equal(changedId, indexed.Id);
        Assert.Equal("Changed after indexing", indexed.Title);
        Assert.Null(catalog.FindById(changedId));
        Assert.Same(definition, catalog.FindByCode(200001));
    }

    [Fact]
    public void IndexedTagLookup_KeepsOriginalKeyButExposesLiveTagList()
    {
        ErrorDefinition definition = CreateDefinition();
        definition.Tags.Add("initial-tag");
        IErrorCatalog catalog = new ErrorCatalog([definition]);

        definition.Tags.Clear();
        definition.Tags.Add("later-tag");

        ErrorDefinition indexed = Assert.Single(
            catalog.FindByTag("initial-tag"));

        Assert.Same(definition, indexed);
        Assert.Equal("later-tag", Assert.Single(indexed.Tags));
        Assert.Empty(catalog.FindByTag("later-tag"));
    }

    [Fact]
    public void Factory_SnapshotsSourceListMembershipButNotContainedDefinitionObjects()
    {
        ErrorDefinition first = CreateDefinition();
        ErrorCatalogDocument document = new()
        {
            Errors = [first]
        };

        IErrorCatalog catalog = new ErrorCatalogFactory().Create(document);
        ErrorDefinition second = CreateDefinition();
        second.Id = "AFW-CFG-0002";
        second.Code = 200002;

        document.Errors.Add(second);
        first.Message = "Changed through source document";

        Assert.Equal(2, document.Errors.Count);
        ErrorDefinition onlyIndexed = Assert.Single(catalog.GetAll());
        Assert.Same(first, onlyIndexed);
        Assert.Equal("Changed through source document", onlyIndexed.Message);
        Assert.Null(catalog.FindById(second.Id));
    }

    private static ErrorDefinition CreateDefinition()
    {
        return new ErrorDefinition
        {
            Id = "AFW-CFG-0001",
            Code = 200001,
            Name = "KnownConfigurationError",
            Owner = "AFW",
            CodePrefix = "CFG",
            CodeGroup = "Configuration",
            PrimaryCategory = "Configuration",
            Title = "Original title",
            Message = "Original message"
        };
    }
}
