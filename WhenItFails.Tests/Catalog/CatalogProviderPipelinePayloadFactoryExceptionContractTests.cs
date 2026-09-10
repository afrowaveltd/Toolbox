using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class CatalogProviderPipelinePayloadFactoryExceptionContractTests
{
    [Fact]
    public async Task LoadNormalizeValidateAsync_WhenPayloadFactoryThrows_ReturnsStableFailure()
    {
        Response<TestPayload> response =
            await CatalogProviderPipeline.LoadNormalizeValidateAsync<TestDocument, TestPayload>(
                filePath: "catalog.json",
                cancellationToken: default,
                loadAsync: (_, _) => Task.FromResult(
                    Response<TestDocument>.Ok(new TestDocument("loaded"))),
                normalize: document => document,
                validate: _ => new ErrorCatalogValidationResult(),
                createPayload: (_, _) => throw new InvalidOperationException(
                    "Sensitive catalog provider pipeline payload factory detail must not escape."),
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
            "The catalog provider pipeline payload factory failed.",
            response.Message);
        Assert.DoesNotContain(
            "Sensitive catalog provider pipeline payload factory detail must not escape.",
            response.Message,
            StringComparison.Ordinal);

        Assert.Collection(
            response.Issues,
            issue =>
            {
                Assert.Equal(
                    "WIF_CATALOG_PIPELINE_PAYLOAD_FACTORY_FAILED",
                    issue.Code);
                Assert.Equal(
                    "The catalog provider pipeline payload factory failed.",
                    issue.Message);
            });
    }

    private sealed record TestDocument(string Value);

    private sealed record TestPayload(
        TestDocument Document,
        ErrorCatalogValidationResult ValidationResult);
}
