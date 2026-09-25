using System.Reflection;
using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Runtime;
using Afrowave.Toolbox.WhenItFails.Services;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class ContextPublicationGenerationContractTests
{
    [Fact]
    public void BeforeFirstPublication_BothReadContractsRemainUninitialized()
    {
        ErrorCatalogContextStore store = new();
        IErrorCatalogContextPublicationReader reader = store;

        Assert.False(store.IsInitialized);
        Assert.Null(store.Current);

        Response<ErrorCatalogContext> legacy = store.GetCurrent();
        Response<ErrorCatalogContextPublication> publication =
            reader.GetCurrentPublication();

        Assert.Equal(ResultStatus.Invalid, legacy.Status);
        Assert.Null(legacy.Data);
        Assert.Equal(ResultStatus.Invalid, publication.Status);
        Assert.Null(publication.Data);
        Assert.Contains(publication.Issues,
            issue => issue.Code == "ErrorCatalogContextNotInitialized");
    }

    [Fact]
    public void FirstAndSubsequentSets_IncrementGenerationAndRetainOldPublication()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext first = new();
        ErrorCatalogContext second = new();

        store.Set(first);
        ErrorCatalogContextPublication old =
            Assert.IsType<ErrorCatalogContextPublication>(
                store.GetCurrentPublication().Data);

        store.Set(second);
        ErrorCatalogContextPublication current =
            Assert.IsType<ErrorCatalogContextPublication>(
                store.GetCurrentPublication().Data);

        Assert.NotEqual(Guid.Empty, current.StoreId);
        Assert.Equal(old.StoreId, current.StoreId);
        Assert.Equal(1L, old.Generation);
        Assert.Equal(2L, current.Generation);
        Assert.Same(first, old.Context);
        Assert.Same(second, current.Context);
        Assert.Same(second, store.Current);
        Assert.Same(second, store.GetCurrent().Data);
        Assert.NotSame(old, current);
        Assert.True(store.IsInitialized);
    }

    [Fact]
    public void PublishingSameReferenceAgain_CountsAsNewPublication()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext shared = new();
        store.Set(shared);
        ErrorCatalogContextPublication first =
            Assert.IsType<ErrorCatalogContextPublication>(
                store.GetCurrentPublication().Data);

        // A successful Set is a publication event even for identical object identity.
        store.Set(shared);
        ErrorCatalogContextPublication second =
            Assert.IsType<ErrorCatalogContextPublication>(
                store.GetCurrentPublication().Data);

        Assert.Equal(first.StoreId, second.StoreId);
        Assert.Equal(1L, first.Generation);
        Assert.Equal(2L, second.Generation);
        Assert.Same(shared, first.Context);
        Assert.Same(shared, second.Context);
        Assert.NotSame(first, second);
    }

    [Fact]
    public void FailedSet_DoesNotReplacePublicationOrAdvanceGeneration()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext context = new();
        store.Set(context);
        ErrorCatalogContextPublication before =
            Assert.IsType<ErrorCatalogContextPublication>(
                store.GetCurrentPublication().Data);

        Assert.Throws<ArgumentNullException>(() => store.Set(null!));

        ErrorCatalogContextPublication after =
            Assert.IsType<ErrorCatalogContextPublication>(
                store.GetCurrentPublication().Data);

        Assert.Same(before, after);
        Assert.Equal(1L, after.Generation);
        Assert.Same(context, store.GetCurrent().Data);
    }

    [Fact]
    public void ConcurrentSets_AdvanceGenerationInSuccessfulPublicationOrder()
    {
        const int writeCount = 64;
        ErrorCatalogContextStore store = new();

        Parallel.For(0, writeCount, _ =>
            store.Set(new ErrorCatalogContext()));

        ErrorCatalogContextPublication published =
            Assert.IsType<ErrorCatalogContextPublication>(
                store.GetCurrentPublication().Data);

        Assert.Equal((long)writeCount, published.Generation);
        Assert.Same(published.Context, store.GetCurrent().Data);
    }

    [Fact]
    public void IndependentStores_HaveIndependentGenerationScopes()
    {
        ErrorCatalogContextStore first = new();
        ErrorCatalogContextStore second = new();
        first.Set(new ErrorCatalogContext());
        second.Set(new ErrorCatalogContext());

        ErrorCatalogContextPublication firstPublication =
            Assert.IsType<ErrorCatalogContextPublication>(
                first.GetCurrentPublication().Data);
        ErrorCatalogContextPublication secondPublication =
            Assert.IsType<ErrorCatalogContextPublication>(
                second.GetCurrentPublication().Data);

        Assert.Equal(1L, firstPublication.Generation);
        Assert.Equal(1L, secondPublication.Generation);
        Assert.NotEqual(firstPublication.StoreId, secondPublication.StoreId);
    }

    [Fact]
    public void Publication_HasGetterOnlyPropertiesAndIsAnOptionalStoreContract()
    {
        Type type = typeof(ErrorCatalogContextPublication);
        Assert.True(type.IsSealed);
        Assert.True(type.IsPublic);
        Assert.Empty(type.GetConstructors(
            BindingFlags.Public | BindingFlags.Instance));
        Assert.Equal(3, type.GetProperties(
            BindingFlags.Public | BindingFlags.Instance |
            BindingFlags.DeclaredOnly).Length);

        Assert.All(type.GetProperties(
                BindingFlags.Public | BindingFlags.Instance |
                BindingFlags.DeclaredOnly),
            property => Assert.Null(property.SetMethod));

        Assert.Contains(typeof(IErrorCatalogContextPublicationReader),
            typeof(ErrorCatalogContextStore).GetInterfaces());
        Assert.DoesNotContain(typeof(IErrorCatalogContextStore).GetMethods(),
            method => method.Name == "GetCurrentPublication");
    }
}
