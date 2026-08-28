using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class CatalogProviderPipelineNullIssuesCollectionContractTests
{
    [Fact]
    public async Task LoadNormalizeValidateAsync_ShouldUseFallbackCode_WhenLoaderIssuesCollectionIsNull()
    {
        Response<TestDocument> loaderResponse = new()
        {
            Status = ResultStatus.Failed,
            Message = string.Empty,
            Data = null,
            Issues = null!
        };

        Response<TestPayload> response =
            await CatalogProviderPipeline.LoadNormalizeValidateAsync<TestDocument, TestPayload>(
                filePath: "catalog.json",
                cancellationToken: default,
                loadAsync: (_, _) => Task.FromResult(loaderResponse),
                normalize: document => document,
                validate: _ => new ErrorCatalogValidationResult(),
                createPayload: (document, validation) =>
                    new TestPayload(document, validation),
                loadFailedCode: "LoadFailed",
                loadFailedMessage: "Load failed.",
                loadedDocumentIsNullCode: "DocumentNull",
                loadedDocumentIsNullMessage: "Document is null.",
                validationFailedCode: "ValidationFailed",
                validationFailedMessage: "Validation failed.");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal("Load failed.", response.Message);

        var issue = Assert.Single(response.Issues);
        Assert.Equal("LoadFailed", issue.Code);
        Assert.Equal("Load failed.", issue.Message);
    }

    private sealed record TestDocument(string Value);

    private sealed record TestPayload(
        TestDocument Document,
        ErrorCatalogValidationResult ValidationResult);
}
