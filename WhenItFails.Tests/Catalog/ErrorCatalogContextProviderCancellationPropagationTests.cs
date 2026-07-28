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
    public async Task LoadFromJsonsAsync_ShouldPropagateCancellationBetweenErrorAndCategoryProviderCalls()
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
                CreateOptions(),
                cancellationTokenSource.Token));
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldPropagateCancellationBetweenCategoryAndCodeGroupProviderCalls()
    {
        using CancellationTokenSource cancellationTokenSource = new();

        ErrorCatalogContextProvider provider = new(
            new SuccessfulErrorCatalogProvider(),
            new CancellingCategoryCatalogProvider(cancellationTokenSource),
            new CancellationObservingCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => provider.LoadFromJsonsAsync(
                CreateOptions(),
                cancellationTokenSource.Token));
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldPropagateCancellationBetweenCodeGroupAndOwnerProviderCalls()
    {
        using CancellationTokenSource cancellationTokenSource = new();

        ErrorCatalogContextProvider provider = new(
            new SuccessfulErrorCatalogProvider(),
            new SuccessfulCategoryCatalogProvider(),
            new CancellingCodeGroupCatalogProvider(cancellationTokenSource),
            new CancellationObservingOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => provider.LoadFromJsonsAsync(
                CreateOptions(),
                cancellationTokenSource.Token));
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldPropagateCancellationBetweenOwnerAndProfileProviderCalls()
    {
        using CancellationTokenSource cancellationTokenSource = new();

        ErrorCatalogContextProvider provider = new(
            new SuccessfulErrorCatalogProvider(),
            new SuccessfulCategoryCatalogProvider(),
            new SuccessfulCodeGroupCatalogProvider(),
            new CancellingOwnerCatalogProvider(cancellationTokenSource),
            new CancellationObservingProfileCatalogProvider());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => provider.LoadFromJsonsAsync(
                CreateOptions(),
                cancellationTokenSource.Token));
    }

    private static JsonsOptions CreateOptions()
    {
        return new JsonsOptions
        {
            RootDirectory = "Jsons",
            PackageDirectoryName = "WhenItFails"
        };
    }

    private static ErrorCatalogProviderPayload CreateErrorCatalogPayload()
    {
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

        return new ErrorCatalogProviderPayload
        {
            Catalog = new ErrorCatalog(document.Errors),
            Document = document,
            ValidationResult = new ErrorCatalogValidationResult()
        };
    }

    private static ErrorCategoryCatalogProviderPayload CreateCategoryCatalogPayload()
    {
        return new ErrorCategoryCatalogProviderPayload
        {
            Document = new ErrorCategoryCatalogDocument
            {
                Categories =
                [
                    new ErrorCategoryDefinition
                    {
                        Name = "GENERAL",
                        DisplayName = "General"
                    }
                ]
            },
            ValidationResult = new ErrorCatalogValidationResult()
        };
    }

    private static ErrorCodeGroupCatalogProviderPayload CreateCodeGroupCatalogPayload()
    {
        return new ErrorCodeGroupCatalogProviderPayload
        {
            Document = new ErrorCodeGroupCatalogDocument
            {
                CodeGroups =
                [
                    new ErrorCodeGroupDefinition
                    {
                        Name = "GENERAL",
                        DisplayName = "General",
                        CodePrefix = "GEN",
                        CodeFrom = 100000,
                        CodeTo = 199999
                    }
                ]
            },
            ValidationResult = new ErrorCatalogValidationResult()
        };
    }

    private static ErrorOwnerCatalogProviderPayload CreateOwnerCatalogPayload()
    {
        return new ErrorOwnerCatalogProviderPayload
        {
            Document = new ErrorOwnerCatalogDocument
            {
                Owners =
                [
                    new ErrorOwnerDefinition
                    {
                        Name = "AFW",
                        DisplayName = "Afrowave",
                        CodeFrom = 0,
                        CodeTo = 999999,
                        IsBuiltIn = true
                    }
                ]
            },
            ValidationResult = new ErrorCatalogValidationResult()
        };
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
            _cancellationTokenSource.Cancel();

            return Task.FromResult(Response<ErrorCatalogProviderPayload>.Ok(
                CreateErrorCatalogPayload()));
        }
    }

    private sealed class SuccessfulErrorCatalogProvider : IErrorCatalogProvider
    {
        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Response<ErrorCatalogProviderPayload>.Ok(
                CreateErrorCatalogPayload()));
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

    private sealed class CancellingCategoryCatalogProvider : IErrorCategoryCatalogProvider
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        public CancellingCategoryCatalogProvider(CancellationTokenSource cancellationTokenSource)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }

        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _cancellationTokenSource.Cancel();

            return Task.FromResult(Response<ErrorCategoryCatalogProviderPayload>.Ok(
                CreateCategoryCatalogPayload()));
        }
    }

    private sealed class SuccessfulCategoryCatalogProvider : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Response<ErrorCategoryCatalogProviderPayload>.Ok(
                CreateCategoryCatalogPayload()));
        }
    }

    private sealed class CancellationObservingCodeGroupCatalogProvider : IErrorCodeGroupCatalogProvider
    {
        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            throw new InvalidOperationException(
                "The code-group provider must observe cancellation before producing a response.");
        }
    }

    private sealed class CancellingCodeGroupCatalogProvider : IErrorCodeGroupCatalogProvider
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        public CancellingCodeGroupCatalogProvider(CancellationTokenSource cancellationTokenSource)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }

        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _cancellationTokenSource.Cancel();

            return Task.FromResult(Response<ErrorCodeGroupCatalogProviderPayload>.Ok(
                CreateCodeGroupCatalogPayload()));
        }
    }

    private sealed class SuccessfulCodeGroupCatalogProvider : IErrorCodeGroupCatalogProvider
    {
        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Response<ErrorCodeGroupCatalogProviderPayload>.Ok(
                CreateCodeGroupCatalogPayload()));
        }
    }

    private sealed class CancellationObservingOwnerCatalogProvider : IErrorOwnerCatalogProvider
    {
        public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            throw new InvalidOperationException(
                "The owner provider must observe cancellation before producing a response.");
        }
    }

    private sealed class CancellingOwnerCatalogProvider : IErrorOwnerCatalogProvider
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        public CancellingOwnerCatalogProvider(CancellationTokenSource cancellationTokenSource)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }

        public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _cancellationTokenSource.Cancel();

            return Task.FromResult(Response<ErrorOwnerCatalogProviderPayload>.Ok(
                CreateOwnerCatalogPayload()));
        }
    }

    private sealed class CancellationObservingProfileCatalogProvider : IErrorProfileCatalogProvider
    {
        public Task<Response<ErrorProfileCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            throw new InvalidOperationException(
                "The profile provider must observe cancellation before producing a response.");
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
