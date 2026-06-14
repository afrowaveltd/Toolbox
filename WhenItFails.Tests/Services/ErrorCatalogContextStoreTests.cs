using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Services;

namespace Afrowave.Toolbox.WhenItFails.Tests.Services;

public sealed class ErrorCatalogContextStoreTests
{
    [Fact]
    public void NewStore_ShouldNotBeInitialized()
    {
        ErrorCatalogContextStore store = new();

        Assert.False(store.IsInitialized);
        Assert.Null(store.Current);
    }

    [Fact]
    public void GetCurrent_ShouldReturnInvalidResponse_WhenStoreIsNotInitialized()
    {
        ErrorCatalogContextStore store = new();

        Response<ErrorCatalogContext> response =
            store.GetCurrent();

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);
        Assert.NotEmpty(response.Issues);
        Assert.Equal(
            "ErrorCatalogContextNotInitialized",
            response.Issues[0].Code);
    }

    [Fact]
    public void Set_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        ErrorCatalogContextStore store = new();

        Assert.Throws<ArgumentNullException>(
            () => store.Set(null!));
    }

    [Fact]
    public void Set_ShouldStoreContext()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext context = new();

        store.Set(context);

        Assert.True(store.IsInitialized);
        Assert.Same(context, store.Current);
    }

    [Fact]
    public void GetCurrent_ShouldReturnStoredContext()
    {
        ErrorCatalogContextStore store = new();
        ErrorCatalogContext context = new();

        store.Set(context);

        Response<ErrorCatalogContext> response =
            store.GetCurrent();

        Assert.True(response.IsSuccess);
        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Same(context, response.Data);
    }

    [Fact]
    public void Set_ShouldReplaceExistingContext()
    {
        ErrorCatalogContextStore store = new();

        ErrorCatalogContext firstContext = new();
        ErrorCatalogContext secondContext = new();

        store.Set(firstContext);
        store.Set(secondContext);

        Assert.True(store.IsInitialized);
        Assert.Same(secondContext, store.Current);
        Assert.NotSame(firstContext, store.Current);
    }
}