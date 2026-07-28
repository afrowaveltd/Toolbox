using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogContextProviderShortCircuitTests
{
    [Fact]
    public async Task LoadFromJsonsAsync_ShouldNotCallLaterProviders_WhenErrorCatalogProviderFails()
    {
        ErrorCatalogContextProvider provider = new(
            new FailingErrorCatalogProvider(),
            new UnexpectedCategoryCatalogProvider(),
            new UnexpectedCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        Response<ErrorCatalogContext> response = await provider.LoadFromJsonsAsync(CreateOptions());

        Assert.False(response.IsSuccess);
        Assert.Equal("ErrorCatalogUnavailable", Assert.Single(response.Issues).Code);
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldNotCallLaterProviders_WhenCategoryCatalogProviderFails()
    {
        ErrorCatalogContextProvider provider = new(
            new SuccessfulErrorCatalogProvider(),
            new FailingCategoryCatalogProvider(),
            new UnexpectedCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        Response<ErrorCatalogContext> response = await provider.LoadFromJsonsAsync(CreateOptions());

        Assert.False(response.IsSuccess);
        Assert.Equal("CategoryCatalogUnavailable", Assert.Single(response.Issues).Code);
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldNotCallLaterProviders_WhenCodeGroupCatalogProviderFails()
    {
        ErrorCatalogContextProvider provider = new(
            new SuccessfulErrorCatalogProvider(),
            new SuccessfulCategoryCatalogProvider(),
            new FailingCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        Response<ErrorCatalogContext> response = await provider.LoadFromJsonsAsync(CreateOptions());

        Assert.False(response.IsSuccess);
        Assert.Equal("CodeGroupCatalogUnavailable", Assert.Single(response.Issues).Code);
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldNotCallProfileProvider_WhenOwnerCatalogProviderFails()
    {
        ErrorCatalogContextProvider provider = new(
            new SuccessfulErrorCatalogProvider(),
            new SuccessfulCategoryCatalogProvider(),
            new SuccessfulCodeGroupCatalogProvider(),
            new FailingOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        Response<ErrorCatalogContext> response = await provider.LoadFromJsonsAsync(CreateOptions());

        Assert.False(response.IsSuccess);
        Assert.Equal("OwnerCatalogUnavailable", Assert.Single(response.Issues).Code);
    }

    private static JsonsOptions CreateOptions()
    {
        return new JsonsOptions
        {
            RootDirectory = "Jsons",
            PackageDirectoryName = "WhenItFails"
        };
    }

    private sealed class FailingErrorCatalogProvider : IErrorCatalogProvider
    {
        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Response<ErrorCatalogProviderPayload>.NotFound(
                code: "ErrorCatalogUnavailable",
                message: "Error catalog is unavailable."));
        }
    }

    private sealed class SuccessfulErrorCatalogProvider : IErrorCatalogProvider
    {
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

            return Task.FromResult(Response<ErrorCatalogProviderPayload>.Ok(
                new ErrorCatalogProviderPayload
                {
                    Catalog = new ErrorCatalog(document.Errors),
                    Document = document,
                    ValidationResult = new ErrorCatalogValidationResult()
                }));
        }
    }

    private sealed class FailingCategoryCatalogProvider : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Response<ErrorCategoryCatalogProviderPayload>.Invalid(
                code: "CategoryCatalogUnavailable",
                message: "Category catalog is unavailable."));
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
                new ErrorCategoryCatalogProviderPayload
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
                }));
        }
    }

    private sealed class FailingCodeGroupCatalogProvider : IErrorCodeGroupCatalogProvider
    {
        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Response<ErrorCodeGroupCatalogProviderPayload>.Invalid(
                code: "CodeGroupCatalogUnavailable",
                message: "Code-group catalog is unavailable."));
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
                new ErrorCodeGroupCatalogProviderPayload
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
                }));
        }
    }

    private sealed class FailingOwnerCatalogProvider : IErrorOwnerCatalogProvider
    {
        public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Response<ErrorOwnerCatalogProviderPayload>.Invalid(
                code: "OwnerCatalogUnavailable",
                message: "Owner catalog is unavailable."));
        }
    }

    private sealed class UnexpectedCategoryCatalogProvider : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Category provider must not be called after the error catalog provider fails.");
        }
    }

    private sealed class UnexpectedCodeGroupCatalogProvider : IErrorCodeGroupCatalogProvider
    {
        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Code-group provider must not be called after an earlier provider fails.");
        }
    }

    private sealed class UnexpectedOwnerCatalogProvider : IErrorOwnerCatalogProvider
    {
        public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Owner provider must not be called after an earlier provider fails.");
        }
    }

    private sealed class UnexpectedProfileCatalogProvider : IErrorProfileCatalogProvider
    {
        public Task<Response<ErrorProfileCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Profile provider must not be called after an earlier provider fails.");
        }
    }
}
