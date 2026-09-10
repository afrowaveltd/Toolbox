using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class CatalogProviderPipelineValidatorExceptionContractTests
{
    [Fact]
    public async Task LoadNormalizeValidateAsync_WhenValidatorThrows_ReturnsStableFailure()
    {
        Response<TestPayload> response =
            await CatalogProviderPipeline.LoadNormalizeValidateAsync<TestDocument, TestPayload>(
                filePath: "catalog.json",
                cancellationToken: default,
                loadAsync: (_, _) => Task.FromResult(
                    Response<TestDocument>.Ok(new TestDocument("loaded"))),
                normalize: document => document,
                validate: _ => throw new InvalidOperationException(
                    "Sensitive catalog provider pipeline validator detail must not escape."),
                createPayload: (_, _) => throw new InvalidOperationException(
                    "The payload factory must not run after the pipeline validator throws."),
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
            "The catalog provider pipeline validator failed.",
            response.Message);
        Assert.DoesNotContain(
            "Sensitive catalog provider pipeline validator detail must not escape.",
            response.Message,
            StringComparison.Ordinal);

        Assert.Collection(
            response.Issues,
            issue =>
            {
                Assert.Equal(
                    "WIF_CATALOG_PIPELINE_VALIDATOR_FAILED",
                    issue.Code);
                Assert.Equal(
                    "The catalog provider pipeline validator failed.",
                    issue.Message);
            });
    }

    private sealed record TestDocument(string Value);

    private sealed record TestPayload(
        TestDocument Document,
        ErrorCatalogValidationResult ValidationResult);
}
