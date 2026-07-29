using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogContextProviderInputOrderingTests
{
    [Fact]
    public async Task LoadFromJsonsAsync_ShouldObservePreCancelledTokenBeforeNullOptionsValidation()
    {
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        ErrorCatalogContextProvider provider = CreateProvider();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => provider.LoadFromJsonsAsync(
                null!,
                cancellationTokenSource.Token));
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldRejectNullOptionsBeforeCallingProviders()
    {
        ErrorCatalogContextProvider provider = CreateProvider();

        ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => provider.LoadFromJsonsAsync(null!));

        Assert.Equal("options", exception.ParamName);
    }

    private static ErrorCatalogContextProvider CreateProvider()
    {
        return new ErrorCatalogContextProvider(
            new UnexpectedErrorCatalogProvider(),
            new UnexpectedCategoryCatalogProvider(),
            new UnexpectedCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());
    }

    private sealed class UnexpectedErrorCatalogProvider : IErrorCatalogProvider
    {
        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The error provider must not be called during input validation.");
        }
    }

    private sealed class UnexpectedCategoryCatalogProvider : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The category provider must not be called during input validation.");
        }
    }

    private sealed class UnexpectedCodeGroupCatalogProvider : IErrorCodeGroupCatalogProvider
    {
        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The code-group provider must not be called during input validation.");
        }
    }

    private sealed class UnexpectedOwnerCatalogProvider : IErrorOwnerCatalogProvider
    {
        public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The owner provider must not be called during input validation.");
        }
    }

    private sealed class UnexpectedProfileCatalogProvider : IErrorProfileCatalogProvider
    {
        public Task<Response<ErrorProfileCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The profile provider must not be called during input validation.");
        }
    }
}
