using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Tests.DefinitionContracts;

public sealed class ErrorCategoryCatalogDocumentContractTests
{
    [Fact]
    public void NewDocument_ShouldUseSafeDefaults()
    {
        ErrorCategoryCatalogDocument document = new();

        Assert.Equal("1.0", document.SchemaVersion);
        Assert.Equal(string.Empty, document.CatalogId);
        Assert.Equal(string.Empty, document.CatalogName);
        Assert.Null(document.Description);
        Assert.Equal("en", document.Language);
        Assert.Null(document.SourceCatalogId);
        Assert.Null(document.SourceCatalogVersion);
        Assert.False(document.IsShadowCopy);
        Assert.Empty(document.Tags);
        Assert.NotNull(document.Metadata);
        Assert.Empty(document.Categories);
    }

    [Fact]
    public void NewDocuments_ShouldNotShareMutableContainers()
    {
        ErrorCategoryCatalogDocument first = new();
        ErrorCategoryCatalogDocument second = new();

        Assert.NotSame(first.Tags, second.Tags);
        Assert.NotSame(first.Metadata, second.Metadata);
        Assert.NotSame(first.Categories, second.Categories);
    }
}
