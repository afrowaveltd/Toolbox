using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class CatalogProviderPipelineNormalizerExceptionContractTests
{
    [Fact]
    public async Task LoadNormalizeValidateAsync_WhenNormalizerThrows_ReturnsStableFailure()
    {
        Response<TestPayload> response =
            await CatalogProviderPipeline.LoadNormalizeValidateAsync<TestDocument, TestPayload>(
                filePath: "catalog.json",
                cancellationToken: default,
                loadAsync: (_, _) => Task.FromResult(
                    Response<TestDocument>.Ok(new TestDocument("loaded"))),
                normalize: _ => throw new InvalidOperationException(
                    "Sensitive catalog provider pipeline normalizer detail must not escape."),
                validate: _ => throw new InvalidOperationException(
                    "The validator must not run after the pipeline normalizer throws."),
                createPayload: (_, _) => throw new InvalidOperationException(
                    "The payload factory must not run after the pipeline normalizer throws."),
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
            "The catalog provider pipeline normalizer failed.",
            response.Message);
        Assert.DoesNotContain(
            "Sensitive catalog provider pipeline normalizer detail must not escape.",
            response.Message,
            StringComparison.Ordinal);

        Assert.Collection(
            response.Issues,
            issue =>
            {
                Assert.Equal(
                    "WIF_CATALOG_PIPELINE_NORMALIZER_FAILED",
                    issue.Code);
                Assert.Equal(
                    "The catalog provider pipeline normalizer failed.",
                    issue.Message);
            });
    }

    [Fact]
    public async Task LoadNormalizeValidateAsync_WhenNormalizerCancels_RethrowsSameOperationCanceledException()
    {
        OperationCanceledException cancellation = new(
            "Catalog provider pipeline normalizer cancellation must propagate unchanged.");

        OperationCanceledException thrown =
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => CatalogProviderPipeline.LoadNormalizeValidateAsync<TestDocument, TestPayload>(
                    filePath: "catalog.json",
                    cancellationToken: default,
                    loadAsync: (_, _) => Task.FromResult(
                        Response<TestDocument>.Ok(new TestDocument("loaded"))),
                    normalize: _ => throw cancellation,
                    validate: _ => throw new InvalidOperationException(
                        "The validator must not run after the pipeline normalizer cancels."),
                    createPayload: (_, _) => throw new InvalidOperationException(
                        "The payload factory must not run after the pipeline normalizer cancels."),
                    loadFailedCode: "ConfiguredLoadFailure",
                    loadFailedMessage: "Configured load failure.",
                    loadedDocumentIsNullCode: "DocumentNull",
                    loadedDocumentIsNullMessage: "Document is null.",
                    validationFailedCode: "ValidationFailed",
                    validationFailedMessage: "Validation failed."));

        Assert.Same(cancellation, thrown);
    }

    private sealed record TestDocument(string Value);

    private sealed record TestPayload(
        TestDocument Document,
        ErrorCatalogValidationResult ValidationResult);
}
