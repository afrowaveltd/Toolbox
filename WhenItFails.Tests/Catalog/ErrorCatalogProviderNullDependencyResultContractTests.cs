using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogProviderNullDependencyResultContractTests
{
    [Fact]
    public async Task LoadFromFileAsync_ShouldReturnInvalidResponse_WhenLoaderReturnsNull()
    {
        ErrorCatalogProvider provider = new(
            new NullResponseLoader(),
            new PassthroughNormalizer(),
            new ValidValidator(),
            new ValidFactory());

        Response<ErrorCatalogProviderPayload> response =
            await provider.LoadFromFileAsync("catalog.json");

        AssertInvalidResponse(
            response,
            "WIF_ERROR_CATALOG_LOADER_RESPONSE_NULL",
            "The error catalog loader returned a null response.");
    }

    [Fact]
    public async Task LoadFromFileAsync_ShouldReturnInvalidResponse_WhenNormalizerReturnsNull()
    {
        ErrorCatalogProvider provider = new(
            new ValidLoader(),
            new NullResultNormalizer(),
            new ValidValidator(),
            new ValidFactory());

        Response<ErrorCatalogProviderPayload> response =
            await provider.LoadFromFileAsync("catalog.json");

        AssertInvalidResponse(
            response,
            "WIF_ERROR_CATALOG_NORMALIZER_RESULT_NULL",
            "The error catalog document normalizer returned a null result.");
    }

    [Fact]
    public async Task LoadFromFileAsync_ShouldReturnInvalidResponse_WhenValidatorReturnsNull()
    {
        ErrorCatalogProvider provider = new(
            new ValidLoader(),
            new PassthroughNormalizer(),
            new NullResultValidator(),
            new ValidFactory());

        Response<ErrorCatalogProviderPayload> response =
            await provider.LoadFromFileAsync("catalog.json");

        AssertInvalidResponse(
            response,
            "WIF_ERROR_CATALOG_VALIDATOR_RESULT_NULL",
            "The error catalog validator returned a null result.");
    }

    [Fact]
    public async Task LoadFromFileAsync_ShouldReturnInvalidResponse_WhenFactoryReturnsNull()
    {
        ErrorCatalogProvider provider = new(
            new ValidLoader(),
            new PassthroughNormalizer(),
            new ValidValidator(),
            new NullResultFactory());

        Response<ErrorCatalogProviderPayload> response =
            await provider.LoadFromFileAsync("catalog.json");

        AssertInvalidResponse(
            response,
            "WIF_ERROR_CATALOG_FACTORY_RESULT_NULL",
            "The error catalog factory returned a null result.");
    }

    private static void AssertInvalidResponse(
        Response<ErrorCatalogProviderPayload> response,
        string code,
        string message)
    {
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);

        var issue = Assert.Single(response.Issues);
        Assert.Equal(code, issue.Code);
        Assert.Equal(message, response.Message);
    }

    private static ErrorCatalogDocument CreateDocument()
    {
        return new ErrorCatalogDocument
        {
            SchemaVersion = "1.0",
            CatalogId = "test.catalog",
            CatalogName = "Test Catalog",
            Language = "en",
            Errors =
            [
                new ErrorDefinition
                {
                    Id = "AFW_GEN_0001",
                    Code = 100001,
                    Name = "UnknownError",
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
    }

    private sealed class NullResponseLoader : IErrorCatalogLoader
    {
        public Task<Response<ErrorCatalogDocument>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Response<ErrorCatalogDocument>>(null!);
        }
    }

    private sealed class ValidLoader : IErrorCatalogLoader
    {
        public Task<Response<ErrorCatalogDocument>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Response<ErrorCatalogDocument>.Ok(CreateDocument()));
        }
    }

    private sealed class PassthroughNormalizer : IErrorCatalogDocumentNormalizer
    {
        public ErrorCatalogDocument Normalize(ErrorCatalogDocument document)
        {
            return document;
        }
    }

    private sealed class NullResultNormalizer : IErrorCatalogDocumentNormalizer
    {
        public ErrorCatalogDocument Normalize(ErrorCatalogDocument document)
        {
            return null!;
        }
    }

    private sealed class ValidValidator : IErrorCatalogValidator
    {
        public ErrorCatalogValidationResult Validate(
            ErrorCatalogDocument? document)
        {
            return new ErrorCatalogValidationResult();
        }
    }

    private sealed class NullResultValidator : IErrorCatalogValidator
    {
        public ErrorCatalogValidationResult Validate(
            ErrorCatalogDocument? document)
        {
            return null!;
        }
    }

    private sealed class ValidFactory : IErrorCatalogFactory
    {
        public IErrorCatalog Create(ErrorCatalogDocument document)
        {
            return new ErrorCatalog(document.Errors);
        }
    }

    private sealed class NullResultFactory : IErrorCatalogFactory
    {
        public IErrorCatalog Create(ErrorCatalogDocument document)
        {
            return null!;
        }
    }
}
