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

    [Fact]
    public async Task LoadNormalizeValidateAsync_ShouldPreserveLoaderFailureAndShortCircuit()
    {
        List<string> calls = new();
        Response<TestDocument> loadResponse =
            Response<TestDocument>.Invalid(
                code: "SpecificLoadFailure",
                message: "The catalog source could not be read.");

        Response<TestPayload> response =
            await CatalogProviderPipeline.LoadNormalizeValidateAsync<TestDocument, TestPayload>(
                filePath: "catalog.json",
                cancellationToken: CancellationToken.None,
                loadAsync: (filePath, cancellationToken) =>
                {
                    calls.Add($"load:{filePath}");
                    Assert.False(cancellationToken.IsCancellationRequested);
                    return Task.FromResult(loadResponse);
                },
                normalize: document =>
                {
                    calls.Add("normalize");
                    return document;
                },
                validate: document =>
                {
                    calls.Add("validate");
                    return new ErrorCatalogValidationResult();
                },
                createPayload: (document, validation) =>
                {
                    calls.Add("create-payload");
                    return new TestPayload(document, validation);
                },
                loadFailedCode: "FallbackLoadFailure",
                loadFailedMessage: "Fallback load failure.",
                loadedDocumentIsNullCode: "DocumentNull",
                loadedDocumentIsNullMessage: "Document is null.",
                validationFailedCode: "ValidationFailed",
                validationFailedMessage: "Validation failed.");

        Assert.False(response.IsSuccess);
        Assert.Equal(loadResponse.Status, response.Status);
        Assert.Null(response.Data);
        Assert.NotEmpty(response.Issues);
        Assert.Equal("SpecificLoadFailure", response.Issues[0].Code);
        Assert.Equal("The catalog source could not be read.", response.Message);
        Assert.Equal(["load:catalog.json"], calls);
    }

    [Fact]
    public async Task LoadNormalizeValidateAsync_ShouldRejectSuccessfulLoadWithNullDocumentAndShortCircuit()
    {
        List<string> calls = new();

        Response<TestPayload> response =
            await CatalogProviderPipeline.LoadNormalizeValidateAsync<TestDocument, TestPayload>(
                filePath: "catalog.json",
                cancellationToken: CancellationToken.None,
                loadAsync: (filePath, cancellationToken) =>
                {
                    calls.Add($"load:{filePath}");
                    Assert.False(cancellationToken.IsCancellationRequested);
                    return Task.FromResult(Response<TestDocument>.Ok(null!));
                },
                normalize: document =>
                {
                    calls.Add("normalize");
                    return document;
                },
                validate: document =>
                {
                    calls.Add("validate");
                    return new ErrorCatalogValidationResult();
                },
                createPayload: (document, validation) =>
                {
                    calls.Add("create-payload");
                    return new TestPayload(document, validation);
                },
                loadFailedCode: "LoadFailed",
                loadFailedMessage: "Load failed.",
                loadedDocumentIsNullCode: "SpecificDocumentNull",
                loadedDocumentIsNullMessage: "The loaded catalog document is null.",
                validationFailedCode: "ValidationFailed",
                validationFailedMessage: "Validation failed.");

        Assert.False(response.IsSuccess);
        Assert.Null(response.Data);
        Assert.NotEmpty(response.Issues);
        Assert.Equal("SpecificDocumentNull", response.Issues[0].Code);
        Assert.Equal("The loaded catalog document is null.", response.Message);
        Assert.Equal(["load:catalog.json"], calls);
    }

    [Fact]
    public async Task LoadNormalizeValidateAsync_ShouldRejectInvalidDocumentAndSkipPayloadCreation()
    {
        List<string> calls = new();
        TestDocument loadedDocument = new("loaded");
        TestDocument normalizedDocument = new("normalized");
        ErrorCatalogValidationResult validationResult = new();
        validationResult.AddError(
            code: "InvalidEntry",
            message: "The normalized document contains an invalid entry.");

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
                    return new TestPayload(document, validation);
                },
                loadFailedCode: "LoadFailed",
                loadFailedMessage: "Load failed.",
                loadedDocumentIsNullCode: "DocumentNull",
                loadedDocumentIsNullMessage: "Document is null.",
                validationFailedCode: "SpecificValidationFailure",
                validationFailedMessage: "The catalog document failed validation.");

        Assert.False(response.IsSuccess);
        Assert.Null(response.Data);
        Assert.NotEmpty(response.Issues);
        Assert.Equal("SpecificValidationFailure", response.Issues[0].Code);
        Assert.Equal("The catalog document failed validation.", response.Message);
        Assert.Equal(
            [
                "load:catalog.json",
                "normalize",
                "validate"
            ],
            calls);
    }

    [Fact]
    public async Task LoadNormalizeValidateAsync_ShouldThrowBeforeLoader_WhenCancellationIsAlreadyRequested()
    {
        List<string> calls = new();
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => CatalogProviderPipeline.LoadNormalizeValidateAsync<TestDocument, TestPayload>(
                filePath: "catalog.json",
                cancellationToken: cancellationTokenSource.Token,
                loadAsync: (filePath, cancellationToken) =>
                {
                    calls.Add($"load:{filePath}");
                    return Task.FromResult(Response<TestDocument>.Ok(new TestDocument("loaded")));
                },
                normalize: document =>
                {
                    calls.Add("normalize");
                    return document;
                },
                validate: document =>
                {
                    calls.Add("validate");
                    return new ErrorCatalogValidationResult();
                },
                createPayload: (document, validation) =>
                {
                    calls.Add("create-payload");
                    return new TestPayload(document, validation);
                },
                loadFailedCode: "LoadFailed",
                loadFailedMessage: "Load failed.",
                loadedDocumentIsNullCode: "DocumentNull",
                loadedDocumentIsNullMessage: "Document is null.",
                validationFailedCode: "ValidationFailed",
                validationFailedMessage: "Validation failed."));

        Assert.Empty(calls);
    }

    private sealed record TestDocument(string Value);

    private sealed record TestPayload(
        TestDocument Document,
        ErrorCatalogValidationResult ValidationResult);
}
