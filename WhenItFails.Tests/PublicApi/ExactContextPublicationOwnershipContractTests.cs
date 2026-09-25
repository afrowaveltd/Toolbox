using System.Collections.Concurrent;
using System.Reflection;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Runtime;
using Afrowave.Toolbox.WhenItFails.Services;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class ExactContextPublicationOwnershipContractTests
{
    [Fact]
    public void Publish_ReturnsExactRecordCreatedByThisWrite()
    {
        ErrorCatalogContextStore store = new();
        IErrorCatalogContextPublisher publisher = store;
        ErrorCatalogContext context = new();

        ErrorCatalogContextPublication published = publisher.Publish(context);
        ErrorCatalogContextPublication current =
            Assert.IsType<ErrorCatalogContextPublication>(
                store.GetCurrentPublication().Data);

        Assert.Same(published, current);
        Assert.Same(context, published.Context);
        Assert.Equal(1L, published.Generation);
        Assert.NotEqual(Guid.Empty, published.StoreId);
        Assert.Same(context, store.GetCurrent().Data);
    }

    [Fact]
    public void LaterSameReferenceWrite_DoesNotRetargetPreviousOwnersRecord()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext shared = new();

        ErrorCatalogContextPublication first = store.Publish(shared);
        ErrorCatalogContextPublication external = store.Publish(shared);

        Assert.NotSame(first, external);
        Assert.Equal(first.StoreId, external.StoreId);
        Assert.Equal(1L, first.Generation);
        Assert.Equal(2L, external.Generation);
        Assert.Same(shared, first.Context);
        Assert.Same(shared, external.Context);
        Assert.Same(external, store.GetCurrentPublication().Data);

        // Context reference equality is insufficient to identify which
        // writer owns a publication; the successful record is distinct.
        Assert.Same(first.Context, external.Context);
        Assert.NotSame(first, external);
    }

    [Fact]
    public void LegacySetAndNewPublish_ShareOneGenerationSequence()
    {
        ErrorCatalogContextStore store = new();
        IErrorCatalogContextStore legacy = store;
        legacy.Set(new ErrorCatalogContext());

        ErrorCatalogContextPublication second = store.Publish(
            new ErrorCatalogContext());

        legacy.Set(new ErrorCatalogContext());
        ErrorCatalogContextPublication current =
            Assert.IsType<ErrorCatalogContextPublication>(
                store.GetCurrentPublication().Data);

        Assert.Equal(2L, second.Generation);
        Assert.Equal(3L, current.Generation);
        Assert.Equal(second.StoreId, current.StoreId);
        Assert.NotSame(second, current);
    }

    [Fact]
    public void ConcurrentPublishers_EachReceiveTheirOwnDistinctSuccessfulRecord()
    {
        const int count = 64;
        ErrorCatalogContextStore store = new();
        ConcurrentBag<ErrorCatalogContextPublication> records = new();

        Parallel.For(0, count, _ =>
        {
            ErrorCatalogContext context = new();
            ErrorCatalogContextPublication owned = store.Publish(context);
            Assert.Same(context, owned.Context);
            records.Add(owned);
        });

        Assert.Equal(count, records.Count);
        Assert.Equal(count,
            records.Select(record => record.Generation).Distinct().Count());
        Assert.Equal(
            Enumerable.Range(1, count).Select(value => (long)value),
            records.Select(record => record.Generation).OrderBy(value => value));
        Assert.Single(records.Select(record => record.StoreId).Distinct());
        Assert.Equal(count, store.GetCurrentPublication().Data!.Generation);
        Assert.Contains(
            records,
            record => ReferenceEquals(record, store.GetCurrentPublication().Data));
    }

    [Fact]
    public void NullPublish_DoesNotReplacePreviouslySuccessfulRecord()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContextPublication previous =
            store.Publish(new ErrorCatalogContext());

        Assert.Throws<ArgumentNullException>(() => store.Publish(null!));

        Assert.Same(previous, store.GetCurrentPublication().Data);
        Assert.Equal(1L, store.GetCurrentPublication().Data!.Generation);
    }

    [Fact]
    public void OptionalPublisher_DoesNotChangeLegacyStoreInterfaceOrExposePublicConstructor()
    {
        Assert.Contains(typeof(IErrorCatalogContextPublisher),
            typeof(ErrorCatalogContextStore).GetInterfaces());

        MethodInfo publish = Assert.Single(
            typeof(IErrorCatalogContextPublisher).GetMethods());
        Assert.Equal("Publish", publish.Name);
        Assert.Equal(typeof(ErrorCatalogContextPublication), publish.ReturnType);
        Assert.Equal(typeof(ErrorCatalogContext),
            Assert.Single(publish.GetParameters()).ParameterType);

        MethodInfo set = Assert.Single(
            typeof(IErrorCatalogContextStore).GetMethods(),
            method => method.Name == "Set");
        Assert.Equal(typeof(void), set.ReturnType);
        Assert.DoesNotContain(typeof(IErrorCatalogContextStore).GetMethods(),
            method => method.Name == "Publish");
        Assert.Empty(typeof(ErrorCatalogContextPublication).GetConstructors(
            BindingFlags.Public | BindingFlags.Instance));
    }
}
