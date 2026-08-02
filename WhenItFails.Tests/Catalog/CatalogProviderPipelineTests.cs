using Afrowave.Toolbox.Essentials.Enums;
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

        Response<TestPayload> response = await InvokePipelineAsync(
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
            });

        Assert.True(response.IsSuccess);
        Assert.Same(expectedPayload, response.Data);
        Assert.Equal(
            ["load:catalog.json", "normalize", "validate", "create-payload"],
            calls);
    }

    [Fact]
    public async Task LoadNormalizeValidateAsync_ShouldPreserveLoaderFailureAndShortCircuit()
    {
        List<string> calls = new();
        Response<TestDocument> loadResponse = Response<TestDocument>.Invalid(
            code: "SpecificLoadFailure",
            message: "The catalog source could not be read.");

        Response<TestPayload> response = await InvokePipelineAsync(
            loadAsync: (filePath, _) =>
            {
                calls.Add($"load:{filePath}");
                return Task.FromResult(loadResponse);
            },
            normalize: document =>
            {
                calls.Add("normalize");
                return document;
            },
            validate: _ =>
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
            loadFailedMessage: "Fallback load failure.");

        Assert.False(response.IsSuccess);
        Assert.Equal(loadResponse.Status, response.Status);
        Assert.Null(response.Data);
        Assert.NotEmpty(response.Issues);
        Assert.Equal("SpecificLoadFailure", response.Issues[0].Code);
        Assert.Equal("The catalog source could not be read.", response.Message);
        Assert.Equal(["load:catalog.json"], calls);
    }

    [Fact]
    public async Task LoadNormalizeValidateAsync_ShouldUseConfiguredFallbacks_WhenLoaderFailureHasNoDetails()
    {
        List<string> calls = new();
        Response<TestDocument> loadResponse = new()
        {
            Status = ResultStatus.NotFound,
            Message = "   "
        };

        Response<TestPayload> response = await InvokePipelineAsync(
            loadAsync: (filePath, _) =>
            {
                calls.Add($"load:{filePath}");
                return Task.FromResult(loadResponse);
            },
            normalize: document =>
            {
                calls.Add("normalize");
                return document;
            },
            validate: _ =>
            {
                calls.Add("validate");
                return new ErrorCatalogValidationResult();
            },
            createPayload: (document, validation) =>
            {
                calls.Add("create-payload");
                return new TestPayload(document, validation);
            },
            loadFailedCode: "ConfiguredLoadFailure",
            loadFailedMessage: "The configured catalog could not be loaded.");

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.NotFound, response.Status);
        Assert.Null(response.Data);
        Assert.Single(response.Issues);
        Assert.Equal("ConfiguredLoadFailure", response.Issues[0].Code);
        Assert.Equal("The configured catalog could not be loaded.", response.Message);
        Assert.Equal(["load:catalog.json"], calls);
    }

    [Fact]
    public async Task LoadNormalizeValidateAsync_ShouldRejectSuccessfulLoadWithNullDocumentAndShortCircuit()
    {
        List<string> calls = new();

        Response<TestPayload> response = await InvokePipelineAsync(
            loadAsync: (filePath, _) =>
            {
                calls.Add($"load:{filePath}");
                return Task.FromResult(Response<TestDocument>.Ok(null!));
            },
            normalize: document =>
            {
                calls.Add("normalize");
                return document;
            },
            validate: _ =>
            {
                calls.Add("validate");
                return new ErrorCatalogValidationResult();
            },
            createPayload: (document, validation) =>
            {
                calls.Add("create-payload");
                return new TestPayload(document, validation);
            },
            loadedDocumentIsNullCode: "SpecificDocumentNull",
            loadedDocumentIsNullMessage: "The loaded catalog document is null.");

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

        Response<TestPayload> response = await InvokePipelineAsync(
            loadAsync: (filePath, _) =>
            {
                calls.Add($"load:{filePath}");
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
            validationFailedCode: "SpecificValidationFailure",
            validationFailedMessage: "The catalog document failed validation.");

        Assert.False(response.IsSuccess);
        Assert.Null(response.Data);
        Assert.NotEmpty(response.Issues);
        Assert.Equal("SpecificValidationFailure", response.Issues[0].Code);
        Assert.Equal("The catalog document failed validation.", response.Message);
        Assert.Equal(["load:catalog.json", "normalize", "validate"], calls);
    }

    [Fact]
    public async Task LoadNormalizeValidateAsync_ShouldThrowBeforeLoader_WhenCancellationIsAlreadyRequested()
    {
        List<string> calls = new();
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => InvokePipelineAsync(
                loadAsync: (filePath, _) =>
                {
                    calls.Add($"load:{filePath}");
                    return Task.FromResult(Response<TestDocument>.Ok(new TestDocument("loaded")));
                },
                normalize: document => document,
                validate: _ => new ErrorCatalogValidationResult(),
                createPayload: (document, validation) => new TestPayload(document, validation),
                cancellationToken: cancellationTokenSource.Token));

        Assert.Empty(calls);
    }

    [Fact]
    public async Task LoadNormalizeValidateAsync_ShouldThrowArgumentNullException_WhenLoaderIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => InvokePipelineAsync(
                loadAsync: null!,
                normalize: ValidNormalize,
                validate: ValidValidate,
                createPayload: ValidCreatePayload));
    }

    [Fact]
    public async Task LoadNormalizeValidateAsync_ShouldThrowArgumentNullException_WhenNormalizerIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => InvokePipelineAsync(
                loadAsync: ValidLoadAsync,
                normalize: null!,
                validate: ValidValidate,
                createPayload: ValidCreatePayload));
    }

    [Fact]
    public async Task LoadNormalizeValidateAsync_ShouldThrowArgumentNullException_WhenValidatorIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => InvokePipelineAsync(
                loadAsync: ValidLoadAsync,
                normalize: ValidNormalize,
                validate: null!,
                createPayload: ValidCreatePayload));
    }

    [Fact]
    public async Task LoadNormalizeValidateAsync_ShouldThrowArgumentNullException_WhenPayloadFactoryIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => InvokePipelineAsync(
                loadAsync: ValidLoadAsync,
                normalize: ValidNormalize,
                validate: ValidValidate,
                createPayload: null!));
    }

    private static Task<Response<TestPayload>> InvokePipelineAsync(
        Func<string, CancellationToken, Task<Response<TestDocument>>> loadAsync,
        Func<TestDocument, TestDocument> normalize,
        Func<TestDocument, ErrorCatalogValidationResult> validate,
        Func<TestDocument, ErrorCatalogValidationResult, TestPayload> createPayload,
        CancellationToken cancellationToken = default,
        string loadFailedCode = "LoadFailed",
        string loadFailedMessage = "Load failed.",
        string loadedDocumentIsNullCode = "DocumentNull",
        string loadedDocumentIsNullMessage = "Document is null.",
        string validationFailedCode = "ValidationFailed",
        string validationFailedMessage = "Validation failed.")
    {
        return CatalogProviderPipeline.LoadNormalizeValidateAsync<TestDocument, TestPayload>(
            filePath: "catalog.json",
            cancellationToken: cancellationToken,
            loadAsync: loadAsync,
            normalize: normalize,
            validate: validate,
            createPayload: createPayload,
            loadFailedCode: loadFailedCode,
            loadFailedMessage: loadFailedMessage,
            loadedDocumentIsNullCode: loadedDocumentIsNullCode,
            loadedDocumentIsNullMessage: loadedDocumentIsNullMessage,
            validationFailedCode: validationFailedCode,
            validationFailedMessage: validationFailedMessage);
    }

    private static Task<Response<TestDocument>> ValidLoadAsync(
        string _,
        CancellationToken __)
    {
        return Task.FromResult(
            Response<TestDocument>.Ok(new TestDocument("loaded")));
    }

    private static TestDocument ValidNormalize(TestDocument document) => document;

    private static ErrorCatalogValidationResult ValidValidate(TestDocument _) => new();

    private static TestPayload ValidCreatePayload(
        TestDocument document,
        ErrorCatalogValidationResult validation)
    {
        return new TestPayload(document, validation);
    }

    private sealed record TestDocument(string Value);

    private sealed record TestPayload(
        TestDocument Document,
        ErrorCatalogValidationResult ValidationResult);
}
