using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Services;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class ActiveContextSharedReferenceContractTests
{
    [Fact]
    public void GetCurrent_ReturnsLiveMutableContextSharedBySubsequentReaders()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext context = new();
        store.Set(context);

        Response<ErrorCatalogContext> response = store.GetCurrent();
        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);

        ErrorCategoryCatalogDocument replacement = new();
        response.Data.CategoryCatalog = replacement;

        Assert.Same(context, store.Current);
        Assert.Same(replacement, context.CategoryCatalog);
        Assert.Same(replacement, store.GetCurrent().Data!.CategoryCatalog);
    }

    [Fact]
    public void Set_RetainsPublishersMutableContextReference()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext published = new();
        store.Set(published);

        ErrorOwnerCatalogDocument replacement = new();
        published.OwnerCatalog = replacement;

        Assert.Same(published, store.Current);
        Assert.Same(replacement, store.GetCurrent().Data!.OwnerCatalog);
    }

    [Fact]
    public void Set_ReplacesActiveReferenceWithoutRetargetingPreviouslyReadContext()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext first = new();
        ErrorCatalogContext second = new();
        store.Set(first);

        ErrorCatalogContext previouslyRead = store.GetCurrent().Data!;
        store.Set(second);

        ErrorProfileCatalogDocument oldContextOnly = new();
        previouslyRead.ProfileCatalog = oldContextOnly;

        Assert.Same(first, previouslyRead);
        Assert.Same(oldContextOnly, first.ProfileCatalog);
        Assert.Same(second, store.Current);
        Assert.Same(second, store.GetCurrent().Data);
        Assert.NotSame(first.ProfileCatalog, second.ProfileCatalog);
    }
}
