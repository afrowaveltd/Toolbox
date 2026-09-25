using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class NormalizedCatalogReferenceOwnershipTests
{
    [Fact]
    public void Normalization_DetachesDefinitionIdentityWhileFactoryIndexesNormalizedInstance()
    {
        ErrorDefinition sourceDefinition = CreateDefinition();
        ErrorCatalogDocument sourceDocument = new()
        {
            Errors = [sourceDefinition]
        };

        ErrorCatalogDocument normalized =
            new ErrorCatalogDocumentNormalizer().Normalize(sourceDocument);
        IErrorCatalog catalog = new ErrorCatalogFactory().Create(normalized);
        ErrorDefinition indexed = Assert.Single(catalog.GetAll());

        Assert.NotSame(sourceDocument, normalized);
        Assert.NotSame(sourceDefinition, indexed);
        Assert.Same(Assert.Single(normalized.Errors), indexed);
        Assert.Equal("AFW_CFG_0001", indexed.Id);

        sourceDefinition.Id = "AFW-SOURCE-CHANGED";
        sourceDefinition.Title = "Changed in the original definition";

        Assert.Equal("AFW_CFG_0001", indexed.Id);
        Assert.Equal("Original title", indexed.Title);
        Assert.Same(indexed, catalog.FindById("AFW_CFG_0001"));
        Assert.Null(catalog.FindById(sourceDefinition.Id));
    }

    [Fact]
    public void NormalizedDocumentAndIndex_ShareDefinitionsAndKeepOriginalNameIndexAfterMutation()
    {
        ErrorCatalogDocument normalized =
            new ErrorCatalogDocumentNormalizer().Normalize(new ErrorCatalogDocument
            {
                Errors = [CreateDefinition()]
            });
        IErrorCatalog catalog = new ErrorCatalogFactory().Create(normalized);
        ErrorDefinition fromDocument = Assert.Single(normalized.Errors);
        ErrorDefinition fromLookup = Assert.IsType<ErrorDefinition>(
            catalog.FindByName("KNOWN_CONFIGURATION_ERROR"));

        Assert.Same(fromDocument, fromLookup);
        fromDocument.Name = "RENAMED_AFTER_INDEXING";
        fromLookup.Message = "Changed through the lookup result";

        Assert.Equal("RENAMED_AFTER_INDEXING", fromLookup.Name);
        Assert.Equal("Changed through the lookup result", fromDocument.Message);
        Assert.Same(fromDocument, catalog.FindByName("KNOWN_CONFIGURATION_ERROR"));
        Assert.Null(catalog.FindByName("RENAMED_AFTER_INDEXING"));
    }

    [Fact]
    public void Normalization_CopiesTagListsButRetainsMetadataReferencesThroughCatalog()
    {
        ErrorDefinition sourceDefinition = CreateDefinition();
        sourceDefinition.Tags.Add("before");
        sourceDefinition.Metadata.Set("definitionKey", "before");
        ErrorCatalogDocument sourceDocument = new()
        {
            Errors = [sourceDefinition]
        };
        sourceDocument.Tags.Add("document-before");
        sourceDocument.Metadata.Set("documentKey", "before");

        ErrorCatalogDocument normalized =
            new ErrorCatalogDocumentNormalizer().Normalize(sourceDocument);
        IErrorCatalog catalog = new ErrorCatalogFactory().Create(normalized);
        ErrorDefinition indexed = Assert.Single(catalog.GetAll());

        Assert.NotSame(sourceDocument.Tags, normalized.Tags);
        Assert.NotSame(sourceDefinition.Tags, indexed.Tags);
        Assert.Same(sourceDocument.Metadata, normalized.Metadata);
        Assert.Same(sourceDefinition.Metadata, indexed.Metadata);

        sourceDocument.Tags.Add("document-after");
        sourceDefinition.Tags.Add("definition-after");
        sourceDocument.Metadata.Set("documentKey", "after");
        sourceDefinition.Metadata.Set("definitionKey", "after");

        Assert.Single(normalized.Tags);
        Assert.Single(indexed.Tags);
        Assert.Equal("after", normalized.Metadata["documentKey"]);
        Assert.Equal("after", indexed.Metadata["definitionKey"]);
    }

    private static ErrorDefinition CreateDefinition() => new()
    {
        Id = "afw cfg 0001",
        Code = 200001,
        Name = "known configuration error",
        Owner = "AFW",
        CodePrefix = "CFG",
        CodeGroup = "Configuration",
        PrimaryCategory = "Configuration",
        Title = "Original title",
        Message = "Original message"
    };
}
