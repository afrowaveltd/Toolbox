using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogContextProviderNullResponseTaskResultTests
{
    [Fact]
    public async Task LoadFromJsonsAsync_ShouldReturnInvalid_WhenErrorProviderTaskReturnsNullResponse()
    {
        ErrorCatalogContextProvider provider = new(
            new NullResponseErrorCatalogProvider(),
            new UnexpectedCategoryCatalogProvider(),
            new UnexpectedCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        Response<ErrorCatalogContext> response =
            await provider.LoadFromJsonsAsync(CreateOptions());

        AssertInvalidResponse(
            response,
            "WIF_ERROR_CATALOG_PROVIDER_RESPONSE_NULL",
            "The error catalog provider returned a null response.");
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldReturnInvalid_WhenCategoryProviderTaskReturnsNullResponse()
    {
        ErrorCatalogContextProvider provider = new(
            new SuccessfulErrorCatalogProvider(),
            new NullResponseCategoryCatalogProvider(),
            new UnexpectedCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        Response<ErrorCatalogContext> response =
            await provider.LoadFromJsonsAsync(CreateOptions());

        AssertInvalidResponse(
            response,
            "WIF_CATEGORY_CATALOG_PROVIDER_RESPONSE_NULL",
            "The error category catalog provider returned a null response.");
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldReturnInvalid_WhenCodeGroupProviderTaskReturnsNullResponse()
    {
        ErrorCatalogContextProvider provider = new(
            new SuccessfulErrorCatalogProvider(),
            new SuccessfulCategoryCatalogProvider(),
            new NullResponseCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        Response<ErrorCatalogContext> response =
            await provider.LoadFromJsonsAsync(CreateOptions());

        AssertInvalidResponse(
            response,
            "WIF_CODE_GROUP_CATALOG_PROVIDER_RESPONSE_NULL",
            "The error code group catalog provider returned a null response.");
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldReturnInvalid_WhenOwnerProviderTaskReturnsNullResponse()
    {
        ErrorCatalogContextProvider provider = new(
            new SuccessfulErrorCatalogProvider(),
            new SuccessfulCategoryCatalogProvider(),
            new SuccessfulCodeGroupCatalogProvider(),
            new NullResponseOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        Response<ErrorCatalogContext> response =
            await provider.LoadFromJsonsAsync(CreateOptions());

        AssertInvalidResponse(
            response,
            "WIF_OWNER_CATALOG_PROVIDER_RESPONSE_NULL",
            "The error owner catalog provider returned a null response.");
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldReturnInvalid_WhenProfileProviderTaskReturnsNullResponse()
    {
        ErrorCatalogContextProvider provider = new(
            new SuccessfulErrorCatalogProvider(),
            new SuccessfulCategoryCatalogProvider(),
            new SuccessfulCodeGroupCatalogProvider(),
            new SuccessfulOwnerCatalogProvider(),
            new NullResponseProfileCatalogProvider());

        Response<ErrorCatalogContext> response =
            await provider.LoadFromJsonsAsync(CreateOptions());

        AssertInvalidResponse(
            response,
            "WIF_PROFILE_CATALOG_PROVIDER_RESPONSE_NULL",
            "The error profile catalog provider returned a null response.");
    }

    private static void AssertInvalidResponse(
        Response<ErrorCatalogContext> response,
        string expectedCode,
        string expectedMessage)
    {
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);

        var issue = Assert.Single(response.Issues);
        Assert.Equal(expectedCode, issue.Code);
        Assert.Equal(expectedMessage, response.Message);
    }

    private static JsonsOptions CreateOptions()
    {
        return new JsonsOptions
        {
            RootDirectory = "Jsons",
            PackageDirectoryName = "WhenItFails"
        };
    }

    private sealed class NullResponseErrorCatalogProvider : IErrorCatalogProvider
    {
        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<Response<ErrorCatalogProviderPayload>>(null!);
        }
    }

    private sealed class SuccessfulErrorCatalogProvider : IErrorCatalogProvider
    {
        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

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

    private sealed class NullResponseCategoryCatalogProvider : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<Response<ErrorCategoryCatalogProviderPayload>>(null!);
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
                    Document = new ErrorCategoryCatalogDocument(),
                    ValidationResult = new ErrorCatalogValidationResult()
                }));
        }
    }

    private sealed class NullResponseCodeGroupCatalogProvider : IErrorCodeGroupCatalogProvider
    {
        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<Response<ErrorCodeGroupCatalogProviderPayload>>(null!);
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
                    Document = new ErrorCodeGroupCatalogDocument(),
                    ValidationResult = new ErrorCatalogValidationResult()
                }));
        }
    }

    private sealed class NullResponseOwnerCatalogProvider : IErrorOwnerCatalogProvider
    {
        public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<Response<ErrorOwnerCatalogProviderPayload>>(null!);
        }
    }

    private sealed class SuccessfulOwnerCatalogProvider : IErrorOwnerCatalogProvider
    {
        public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Response<ErrorOwnerCatalogProviderPayload>.Ok(
                new ErrorOwnerCatalogProviderPayload
                {
                    Document = new ErrorOwnerCatalogDocument(),
                    ValidationResult = new ErrorCatalogValidationResult()
                }));
        }
    }

    private sealed class NullResponseProfileCatalogProvider : IErrorProfileCatalogProvider
    {
        public Task<Response<ErrorProfileCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<Response<ErrorProfileCatalogProviderPayload>>(null!);
        }
    }

    private sealed class UnexpectedCategoryCatalogProvider : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The category provider must not run after the error provider returns a null response.");
        }
    }

    private sealed class UnexpectedCodeGroupCatalogProvider : IErrorCodeGroupCatalogProvider
    {
        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The code-group provider must not run after an earlier provider returns a null response.");
        }
    }

    private sealed class UnexpectedOwnerCatalogProvider : IErrorOwnerCatalogProvider
    {
        public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The owner provider must not run after an earlier provider returns a null response.");
        }
    }

    private sealed class UnexpectedProfileCatalogProvider : IErrorProfileCatalogProvider
    {
        public Task<Response<ErrorProfileCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "The profile provider must not run after an earlier provider returns a null response.");
        }
    }
}
