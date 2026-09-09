using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class ErrorCatalogProviderLoaderExceptionContractTests
{
    [Fact]
    public async Task LoadFromFileAsync_WhenLoaderThrows_ReturnsStableFailure()
    {
        ErrorCatalogProvider provider = new(
            new ThrowingLoader(),
            new UnexpectedNormalizer(),
            new UnexpectedValidator(),
            new UnexpectedFactory());

        Response<ErrorCatalogProviderPayload> response =
            await provider.LoadFromFileAsync("catalog.json");

        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(
            "The error catalog loader failed.",
            response.Message);
        Assert.DoesNotContain(
            "Sensitive error catalog loader detail must not escape.",
            response.Message,
            StringComparison.Ordinal);

        Assert.Collection(
            response.Issues,
            issue =>
            {
                Assert.Equal(
                    "WIF_ERROR_CATALOG_LOADER_FAILED",
                    issue.Code);
                Assert.Equal(
                    "The error catalog loader failed.",
                    issue.Message);
            });
    }

    private sealed class ThrowingLoader : IErrorCatalogLoader
    {
        public Task<Response<ErrorCatalogDocument>> LoadFromFileAsync(
            string filePath,
            CancellationToken cancellationToken = default)
        {
            return Task.FromException<Response<ErrorCatalogDocument>>(
                new InvalidOperationException(
                    "Sensitive error catalog loader detail must not escape."));
        }
    }

    private sealed class UnexpectedNormalizer : IErrorCatalogDocumentNormalizer
    {
        public ErrorCatalogDocument Normalize(ErrorCatalogDocument document)
        {
            throw new InvalidOperationException(
                "The normalizer must not run after the loader throws.");
        }
    }

    private sealed class UnexpectedValidator : IErrorCatalogValidator
    {
        public ErrorCatalogValidationResult Validate(ErrorCatalogDocument? document)
        {
            throw new InvalidOperationException(
                "The validator must not run after the loader throws.");
        }
    }

    private sealed class UnexpectedFactory : IErrorCatalogFactory
    {
        public IErrorCatalog Create(ErrorCatalogDocument document)
        {
            throw new InvalidOperationException(
                "The factory must not run after the loader throws.");
        }
    }
}
