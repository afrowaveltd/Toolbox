using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogContextProviderExceptionShapeTests
{
    [Fact]
    public async Task LoadFromJsonsAsync_ShouldPropagateDifferentExceptionTypeUnchanged()
    {
        FormatException expectedException = new("Provider returned malformed catalog data.");

        ErrorCatalogContextProvider provider = new(
            new ThrowingFormatExceptionErrorCatalogProvider(expectedException),
            new UnexpectedCategoryCatalogProvider(),
            new UnexpectedCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        FormatException actualException = await Assert.ThrowsAsync<FormatException>(
            () => provider.LoadFromJsonsAsync(CreateOptions()));

        Assert.Same(expectedException, actualException);
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldPropagateCanceledProviderTaskWithOriginalToken()
    {
        using CancellationTokenSource providerCancellation = new();
        providerCancellation.Cancel();

        ErrorCatalogContextProvider provider = new(
            new CanceledErrorCatalogProvider(providerCancellation.Token),
            new UnexpectedCategoryCatalogProvider(),
            new UnexpectedCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        OperationCanceledException exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => provider.LoadFromJsonsAsync(CreateOptions(), CancellationToken.None));

        Assert.Equal(providerCancellation.Token, exception.CancellationToken);
    }

    private static JsonsOptions CreateOptions()
    {
        return new JsonsOptions
        {
            RootDirectory = "Jsons",
            PackageDirectoryName = "WhenItFails"
        };
    }

    private sealed class ThrowingFormatExceptionErrorCatalogProvider : IErrorCatalogProvider
    {
        private readonly FormatException _exception;

        public ThrowingFormatExceptionErrorCatalogProvider(FormatException exception)
        {
            _exception = exception;
        }

        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw _exception;
        }
    }

    private sealed class CanceledErrorCatalogProvider : IErrorCatalogProvider
    {
        private readonly CancellationToken _cancellationToken;

        public CanceledErrorCatalogProvider(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            return Task.FromCanceled<Response<ErrorCatalogProviderPayload>>(_cancellationToken);
        }
    }

    private sealed class UnexpectedCategoryCatalogProvider : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The category provider must not run after the error provider stops the operation.");
        }
    }

    private sealed class UnexpectedCodeGroupCatalogProvider : IErrorCodeGroupCatalogProvider
    {
        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The code-group provider must not run after the error provider stops the operation.");
        }
    }

    private sealed class UnexpectedOwnerCatalogProvider : IErrorOwnerCatalogProvider
    {
        public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The owner provider must not run after the error provider stops the operation.");
        }
    }

    private sealed class UnexpectedProfileCatalogProvider : IErrorProfileCatalogProvider
    {
        public Task<Response<ErrorProfileCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The profile provider must not run after the error provider stops the operation.");
        }
    }
}
