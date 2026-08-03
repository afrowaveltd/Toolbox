using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.WhenItFails.Tests.DefinitionContracts;

public sealed class ErrorOwnerCatalogDocumentContractTests
{
    [Fact]
    public void NewDocument_ShouldUseSafeDefaults()
    {
        ErrorOwnerCatalogDocument document = new();

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
        Assert.Empty(document.Owners);
    }

    [Fact]
    public void NewDocuments_ShouldNotShareMutableContainers()
    {
        ErrorOwnerCatalogDocument first = new();
        ErrorOwnerCatalogDocument second = new();

        Assert.NotSame(first.Tags, second.Tags);
        Assert.NotSame(first.Metadata, second.Metadata);
        Assert.NotSame(first.Owners, second.Owners);
    }
}
