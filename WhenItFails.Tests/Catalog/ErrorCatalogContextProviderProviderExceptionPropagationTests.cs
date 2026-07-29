using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogContextProviderProviderExceptionPropagationTests
{
    [Fact]
    public async Task LoadFromJsonsAsync_ShouldPropagateErrorProviderExceptionWithoutCallingLaterProviders()
    {
        InvalidOperationException expectedException = new("Provider failed unexpectedly.");

        ErrorCatalogContextProvider provider = new(
            new ThrowingErrorCatalogProvider(expectedException),
            new UnexpectedCategoryCatalogProvider(),
            new UnexpectedCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        InvalidOperationException actualException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => provider.LoadFromJsonsAsync(CreateOptions()));

        Assert.Same(expectedException, actualException);
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldPropagateCategoryProviderExceptionAfterErrorProviderSucceeds()
    {
        InvalidOperationException expectedException = new("Category provider failed unexpectedly.");

        ErrorCatalogContextProvider provider = new(
            new SuccessfulErrorCatalogProvider(),
            new ThrowingCategoryCatalogProvider(expectedException),
            new UnexpectedCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        InvalidOperationException actualException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => provider.LoadFromJsonsAsync(CreateOptions()));

        Assert.Same(expectedException, actualException);
    }

    private static JsonsOptions CreateOptions()
    {
        return new JsonsOptions
        {
            RootDirectory = "Jsons",
            PackageDirectoryName = "WhenItFails"
        };
    }

    private sealed class ThrowingErrorCatalogProvider : IErrorCatalogProvider
    {
        private readonly InvalidOperationException _exception;

        public ThrowingErrorCatalogProvider(InvalidOperationException exception)
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

    private sealed class SuccessfulErrorCatalogProvider : IErrorCatalogProvider
    {
        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            ErrorCatalogDocument document = new();

            return Task.FromResult(Response<ErrorCatalogProviderPayload>.Ok(
                new ErrorCatalogProviderPayload
                {
                    Catalog = new ErrorCatalog(document.Errors),
                    Document = document,
                    ValidationResult = new ErrorCatalogValidationResult()
                }));
        }
    }

    private sealed class ThrowingCategoryCatalogProvider : IErrorCategoryCatalogProvider
    {
        private readonly InvalidOperationException _exception;

        public ThrowingCategoryCatalogProvider(InvalidOperationException exception)
        {
            _exception = exception;
        }

        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw _exception;
        }
    }

    private sealed class UnexpectedCategoryCatalogProvider : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The category provider must not run after the error provider throws.");
        }
    }

    private sealed class UnexpectedCodeGroupCatalogProvider : IErrorCodeGroupCatalogProvider
    {
        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The code-group provider must not run after an earlier provider throws.");
        }
    }

    private sealed class UnexpectedOwnerCatalogProvider : IErrorOwnerCatalogProvider
    {
        public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The owner provider must not run after an earlier provider throws.");
        }
    }

    private sealed class UnexpectedProfileCatalogProvider : IErrorProfileCatalogProvider
    {
        public Task<Response<ErrorProfileCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The profile provider must not run after an earlier provider throws.");
        }
    }
}
