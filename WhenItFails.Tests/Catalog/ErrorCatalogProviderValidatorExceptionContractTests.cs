using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogProviderValidatorExceptionContractTests
{
    [Fact]
    public async Task LoadFromFileAsync_WhenValidatorThrows_ReturnsStableFailure()
    {
        ErrorCatalogProvider provider = new(
            new ValidLoader(),
            new PassthroughNormalizer(),
            new ThrowingValidator(),
            new UnexpectedFactory());

        Response<ErrorCatalogProviderPayload> response =
            await provider.LoadFromFileAsync("catalog.json");

        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(
            "The error catalog validator failed.",
            response.Message);
        Assert.DoesNotContain(
            "Sensitive error catalog validator detail must not escape.",
            response.Message,
            StringComparison.Ordinal);

        Assert.Collection(
            response.Issues,
            issue =>
            {
                Assert.Equal(
                    "WIF_ERROR_CATALOG_VALIDATOR_FAILED",
                    issue.Code);
                Assert.Equal(
                    "The error catalog validator failed.",
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

    private sealed class PassthroughNormalizer : IErrorCatalogDocumentNormalizer
    {
        public ErrorCatalogDocument Normalize(ErrorCatalogDocument document)
        {
            return document;
        }
    }

    private sealed class ThrowingValidator : IErrorCatalogValidator
    {
        public ErrorCatalogValidationResult Validate(ErrorCatalogDocument? document)
        {
            throw new InvalidOperationException(
                "Sensitive error catalog validator detail must not escape.");
        }
    }

    private sealed class UnexpectedFactory : IErrorCatalogFactory
    {
        public IErrorCatalog Create(ErrorCatalogDocument document)
        {
            throw new InvalidOperationException(
                "The factory must not run after the validator throws.");
        }
    }
}
