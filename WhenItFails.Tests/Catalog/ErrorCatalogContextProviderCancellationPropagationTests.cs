using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogContextProviderCancellationPropagationTests
{
    [Fact]
    public async Task LoadFromJsonsAsync_ShouldPropagateCancellationBetweenProviderCalls()
    {
        using CancellationTokenSource cancellationTokenSource = new();

        ErrorCatalogContextProvider provider = new(
            new CancellingErrorCatalogProvider(cancellationTokenSource),
            new CancellationObservingCategoryCatalogProvider(),
            new UnexpectedCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => provider.LoadFromJsonsAsync(
                new JsonsOptions
                {
                    RootDirectory = "Jsons",
                    PackageDirectoryName = "WhenItFails"
                },
                cancellationTokenSource.Token));
    }

    private sealed class CancellingErrorCatalogProvider : IErrorCatalogProvider
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        public CancellingErrorCatalogProvider(CancellationTokenSource cancellationTokenSource)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }

        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ErrorCatalogDocument document = new()
            {
                Errors =
                [
                    new ErrorDefinition
                    {
                        Id = "AFW_GEN_0001",
                        Code = 100001,
                        Name = "UNKNOWNERROR",
                        Owner = "AFW",
                        CodePrefix = "GEN",
                        CodeGroup = "GENERAL",
                        PrimaryCategory = "GENERAL",
                        Categories = ["GENERAL"],
                        Title = "Unknown error",
                        Message = "An unknown error occurred.",
                        DefaultSeverity = "Error"
                    }
                ]
            };

            _cancellationTokenSource.Cancel();

            return Task.FromResult(Response<ErrorCatalogProviderPayload>.Ok(
                new ErrorCatalogProviderPayload
                {
                    Catalog = new ErrorCatalog(document.Errors),
                    Document = document,
                    ValidationResult = new ErrorCatalogValidationResult()
                }));
        }
    }

    private sealed class CancellationObservingCategoryCatalogProvider : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            throw new InvalidOperationException(
                "The category provider must observe cancellation before producing a response.");
        }
    }

    private sealed class UnexpectedCodeGroupCatalogProvider : IErrorCodeGroupCatalogProvider
    {
        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The code-group provider must not be called after cancellation is observed.");
        }
    }

    private sealed class UnexpectedOwnerCatalogProvider : IErrorOwnerCatalogProvider
    {
        public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The owner provider must not be called after cancellation is observed.");
        }
    }

    private sealed class UnexpectedProfileCatalogProvider : IErrorProfileCatalogProvider
    {
        public Task<Response<ErrorProfileCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The profile provider must not be called after cancellation is observed.");
        }
    }
}
