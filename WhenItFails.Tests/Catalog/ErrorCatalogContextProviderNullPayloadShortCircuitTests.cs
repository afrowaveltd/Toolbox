using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogContextProviderNullPayloadShortCircuitTests
{
    [Fact]
    public async Task LoadFromJsonsAsync_ShouldNotCallLaterProviders_WhenErrorCatalogProviderSucceedsWithoutPayload()
    {
        ErrorCatalogContextProvider provider = new(
            new NullPayloadErrorCatalogProvider(),
            new UnexpectedCategoryCatalogProvider(),
            new UnexpectedCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        Response<ErrorCatalogContext> response = await provider.LoadFromJsonsAsync(CreateOptions());

        AssertNullPayloadFailure(response);
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldNotCallLaterProviders_WhenCategoryCatalogProviderSucceedsWithoutPayload()
    {
        ErrorCatalogContextProvider provider = new(
            new SuccessfulErrorCatalogProvider(),
            new NullPayloadCategoryCatalogProvider(),
            new UnexpectedCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        Response<ErrorCatalogContext> response = await provider.LoadFromJsonsAsync(CreateOptions());

        AssertNullPayloadFailure(response);
    }

    [Fact]
    public async Task LoadFromJsonsAsync_ShouldNotCallLaterProviders_WhenCodeGroupCatalogProviderSucceedsWithoutPayload()
    {
        ErrorCatalogContextProvider provider = new(
            new SuccessfulErrorCatalogProvider(),
            new SuccessfulCategoryCatalogProvider(),
            new NullPayloadCodeGroupCatalogProvider(),
            new UnexpectedOwnerCatalogProvider(),
            new UnexpectedProfileCatalogProvider());

        Response<ErrorCatalogContext> response = await provider.LoadFromJsonsAsync(CreateOptions());

        AssertNullPayloadFailure(response);
    }

    private static JsonsOptions CreateOptions()
    {
        return new JsonsOptions
        {
            RootDirectory = "Jsons",
            PackageDirectoryName = "WhenItFails"
        };
    }

    private static void AssertNullPayloadFailure(Response<ErrorCatalogContext> response)
    {
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);
        Assert.Equal("ErrorCatalogContextPayloadIsNull", Assert.Single(response.Issues).Code);
    }

    private sealed class NullPayloadErrorCatalogProvider : IErrorCatalogProvider
    {
        public Task<Response<ErrorCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Response<ErrorCatalogProviderPayload>.Ok(null));
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

    private sealed class NullPayloadCategoryCatalogProvider : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Response<ErrorCategoryCatalogProviderPayload>.Ok(null));
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

    private sealed class NullPayloadCodeGroupCatalogProvider : IErrorCodeGroupCatalogProvider
    {
        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Response<ErrorCodeGroupCatalogProviderPayload>.Ok(null));
        }
    }

    private sealed class UnexpectedCategoryCatalogProvider : IErrorCategoryCatalogProvider
    {
        public Task<Response<ErrorCategoryCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "Category provider must not be called after the error provider returns a null payload.");
        }
    }

    private sealed class UnexpectedCodeGroupCatalogProvider : IErrorCodeGroupCatalogProvider
    {
        public Task<Response<ErrorCodeGroupCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "Code-group provider must not be called after an earlier provider returns a null payload.");
        }
    }

    private sealed class UnexpectedOwnerCatalogProvider : IErrorOwnerCatalogProvider
    {
        public Task<Response<ErrorOwnerCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "Owner provider must not be called after an earlier provider returns a null payload.");
        }
    }

    private sealed class UnexpectedProfileCatalogProvider : IErrorProfileCatalogProvider
    {
        public Task<Response<ErrorProfileCatalogProviderPayload>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "Profile provider must not be called after an earlier provider returns a null payload.");
        }
    }
}
