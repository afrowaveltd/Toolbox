using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogProviderNormalizerExceptionContractTests
{
    [Fact]
    public async Task LoadFromFileAsync_WhenNormalizerThrows_ReturnsStableFailure()
    {
        ErrorCatalogProvider provider = new(
            new ValidLoader(),
            new ThrowingNormalizer(),
            new UnexpectedValidator(),
            new UnexpectedFactory());

        Response<ErrorCatalogProviderPayload> response =
            await provider.LoadFromFileAsync("catalog.json");

        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(
            "The error catalog document normalizer failed.",
            response.Message);
        Assert.DoesNotContain(
            "Sensitive error catalog normalizer detail must not escape.",
            response.Message,
            StringComparison.Ordinal);

        Assert.Collection(
            response.Issues,
            issue =>
            {
                Assert.Equal(
                    "WIF_ERROR_CATALOG_NORMALIZER_FAILED",
                    issue.Code);
                Assert.Equal(
                    "The error catalog document normalizer failed.",
                    issue.Message);
            });
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

    private sealed class ThrowingNormalizer : IErrorCatalogDocumentNormalizer
    {
        public ErrorCatalogDocument Normalize(ErrorCatalogDocument document)
        {
            throw new InvalidOperationException(
                "Sensitive error catalog normalizer detail must not escape.");
        }
    }

    private sealed class UnexpectedValidator : IErrorCatalogValidator
    {
        public ErrorCatalogValidationResult Validate(ErrorCatalogDocument? document)
        {
            throw new InvalidOperationException(
                "The validator must not run after the normalizer throws.");
        }
    }

    private sealed class UnexpectedFactory : IErrorCatalogFactory
    {
        public IErrorCatalog Create(ErrorCatalogDocument document)
        {
            throw new InvalidOperationException(
                "The factory must not run after the normalizer throws.");
        }
    }
}
