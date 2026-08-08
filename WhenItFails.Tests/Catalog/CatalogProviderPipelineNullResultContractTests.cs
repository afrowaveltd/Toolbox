using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Catalog;

public sealed class CatalogProviderPipelineNullResultContractTests
{
    [Fact]
    public async Task LoadNormalizeValidateAsync_ShouldReturnInvalidResponse_WhenLoaderReturnsNull()
    {
        Response<TestPayload> response = await InvokePipelineAsync(
            loadAsync: (_, _) =>
                Task.FromResult<Response<TestDocument>>(null!),
            normalize: document => document,
            validate: _ => new ErrorCatalogValidationResult(),
            createPayload: (document, validation) =>
                new TestPayload(document, validation));

        AssertInvalidResponse(
            response,
            "WIF_CATALOG_PIPELINE_LOADER_RESPONSE_NULL",
            "The catalog provider pipeline loader returned a null response.");
    }

    [Fact]
    public async Task LoadNormalizeValidateAsync_ShouldReturnInvalidResponse_WhenNormalizerReturnsNull()
    {
        Response<TestPayload> response = await InvokePipelineAsync(
            loadAsync: ValidLoadAsync,
            normalize: _ => null!,
            validate: _ => new ErrorCatalogValidationResult(),
            createPayload: (document, validation) =>
                new TestPayload(document, validation));

        AssertInvalidResponse(
            response,
            "WIF_CATALOG_PIPELINE_NORMALIZER_RESULT_NULL",
            "The catalog provider pipeline normalizer returned a null result.");
    }

    [Fact]
    public async Task LoadNormalizeValidateAsync_ShouldReturnInvalidResponse_WhenValidatorReturnsNull()
    {
        Response<TestPayload> response = await InvokePipelineAsync(
            loadAsync: ValidLoadAsync,
            normalize: document => document,
            validate: _ => null!,
            createPayload: (document, validation) =>
                new TestPayload(document, validation));

        AssertInvalidResponse(
            response,
            "WIF_CATALOG_PIPELINE_VALIDATOR_RESULT_NULL",
            "The catalog provider pipeline validator returned a null result.");
    }

    [Fact]
    public async Task LoadNormalizeValidateAsync_ShouldReturnInvalidResponse_WhenPayloadFactoryReturnsNull()
    {
        Response<TestPayload> response = await InvokePipelineAsync(
            loadAsync: ValidLoadAsync,
            normalize: document => document,
            validate: _ => new ErrorCatalogValidationResult(),
            createPayload: (_, _) => null!);

        AssertInvalidResponse(
            response,
            "WIF_CATALOG_PIPELINE_PAYLOAD_NULL",
            "The catalog provider pipeline payload factory returned a null result.");
    }

    private static void AssertInvalidResponse(
        Response<TestPayload> response,
        string code,
        string message)
    {
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);

        var issue = Assert.Single(response.Issues);
        Assert.Equal(code, issue.Code);
        Assert.Equal(message, response.Message);
    }

    private static Task<Response<TestPayload>> InvokePipelineAsync(
        Func<string, CancellationToken, Task<Response<TestDocument>>> loadAsync,
        Func<TestDocument, TestDocument> normalize,
        Func<TestDocument, ErrorCatalogValidationResult> validate,
        Func<TestDocument, ErrorCatalogValidationResult, TestPayload> createPayload)
    {
        return CatalogProviderPipeline.LoadNormalizeValidateAsync<TestDocument, TestPayload>(
            filePath: "catalog.json",
            cancellationToken: default,
            loadAsync: loadAsync,
            normalize: normalize,
            validate: validate,
            createPayload: createPayload,
            loadFailedCode: "LoadFailed",
            loadFailedMessage: "Load failed.",
            loadedDocumentIsNullCode: "DocumentNull",
            loadedDocumentIsNullMessage: "Document is null.",
            validationFailedCode: "ValidationFailed",
            validationFailedMessage: "Validation failed.");
    }

    private static Task<Response<TestDocument>> ValidLoadAsync(
        string _,
        CancellationToken __)
    {
        return Task.FromResult(
            Response<TestDocument>.Ok(new TestDocument("loaded")));
    }

    private sealed record TestDocument(string Value);

    private sealed record TestPayload(
        TestDocument Document,
        ErrorCatalogValidationResult ValidationResult);
}
