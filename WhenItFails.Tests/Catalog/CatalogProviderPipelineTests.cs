using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class CatalogProviderPipelineTests
{
    [Fact]
    public async Task LoadNormalizeValidateAsync_ShouldExecuteSuccessfulFlowInOrder()
    {
        List<string> calls = new();
        TestDocument loadedDocument = new("loaded");
        TestDocument normalizedDocument = new("normalized");
        ErrorCatalogValidationResult validationResult = new();
        TestPayload expectedPayload = new(normalizedDocument, validationResult);

        Response<TestPayload> response =
            await CatalogProviderPipeline.LoadNormalizeValidateAsync<TestDocument, TestPayload>(
                filePath: "catalog.json",
                cancellationToken: CancellationToken.None,
                loadAsync: (filePath, cancellationToken) =>
                {
                    calls.Add($"load:{filePath}");
                    Assert.False(cancellationToken.IsCancellationRequested);
                    return Task.FromResult(Response<TestDocument>.Ok(loadedDocument));
                },
                normalize: document =>
                {
                    calls.Add("normalize");
                    Assert.Same(loadedDocument, document);
                    return normalizedDocument;
                },
                validate: document =>
                {
                    calls.Add("validate");
                    Assert.Same(normalizedDocument, document);
                    return validationResult;
                },
                createPayload: (document, validation) =>
                {
                    calls.Add("create-payload");
                    Assert.Same(normalizedDocument, document);
                    Assert.Same(validationResult, validation);
                    return expectedPayload;
                },
                loadFailedCode: "LoadFailed",
                loadFailedMessage: "Load failed.",
                loadedDocumentIsNullCode: "DocumentNull",
                loadedDocumentIsNullMessage: "Document is null.",
                validationFailedCode: "ValidationFailed",
                validationFailedMessage: "Validation failed.");

        Assert.True(response.IsSuccess);
        Assert.Same(expectedPayload, response.Data);
        Assert.Equal(
            [
                "load:catalog.json",
                "normalize",
                "validate",
                "create-payload"
            ],
            calls);
    }

    private sealed record TestDocument(string Value);

    private sealed record TestPayload(
        TestDocument Document,
        ErrorCatalogValidationResult ValidationResult);
}
