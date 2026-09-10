using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class CatalogProviderPipelineLoaderExceptionContractTests
{
    [Fact]
    public async Task LoadNormalizeValidateAsync_WhenLoaderThrows_ReturnsStableFailure()
    {
        Response<TestPayload> response =
            await CatalogProviderPipeline.LoadNormalizeValidateAsync<TestDocument, TestPayload>(
                filePath: "catalog.json",
                cancellationToken: default,
                loadAsync: (_, _) => Task.FromException<Response<TestDocument>>(
                    new InvalidOperationException(
                        "Sensitive catalog provider pipeline loader detail must not escape.")),
                normalize: _ => throw new InvalidOperationException(
                    "The normalizer must not run after the pipeline loader throws."),
                validate: _ => throw new InvalidOperationException(
                    "The validator must not run after the pipeline loader throws."),
                createPayload: (_, _) => throw new InvalidOperationException(
                    "The payload factory must not run after the pipeline loader throws."),
                loadFailedCode: "ConfiguredLoadFailure",
                loadFailedMessage: "Configured load failure.",
                loadedDocumentIsNullCode: "DocumentNull",
                loadedDocumentIsNullMessage: "Document is null.",
                validationFailedCode: "ValidationFailed",
                validationFailedMessage: "Validation failed.");

        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(
            "The catalog provider pipeline loader failed.",
            response.Message);
        Assert.DoesNotContain(
            "Sensitive catalog provider pipeline loader detail must not escape.",
            response.Message,
            StringComparison.Ordinal);

        Assert.Collection(
            response.Issues,
            issue =>
            {
                Assert.Equal(
                    "WIF_CATALOG_PIPELINE_LOADER_FAILED",
                    issue.Code);
                Assert.Equal(
                    "The catalog provider pipeline loader failed.",
                    issue.Message);
            });
    }

    private sealed record TestDocument(string Value);

    private sealed record TestPayload(
        TestDocument Document,
        ErrorCatalogValidationResult ValidationResult);
}
