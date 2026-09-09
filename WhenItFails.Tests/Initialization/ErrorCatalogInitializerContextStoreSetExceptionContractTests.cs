using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Initialization;

public sealed class ErrorCatalogInitializerContextStoreSetExceptionContractTests
{
    [Fact]
    public async Task InitializeAsync_WhenContextStoreSetThrows_ReturnsStableFailure()
    {
        ErrorCatalogInitializer initializer = new(
            new SuccessfulBootstrapper(),
            new SuccessfulContextProvider(),
            new ThrowingSetContextStore());

        Response<ErrorCatalogInitializationPayload> response =
            await initializer.InitializeAsync(new JsonsOptions());

        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(
            "The error catalog context store failed.",
            response.Message);

        Assert.Collection(
            response.Issues,
            issue =>
            {
                Assert.Equal("WIF_CONTEXT_STORE_FAILED", issue.Code);
                Assert.Equal(
                    "The error catalog context store failed.",
                    issue.Message);
            });
    }

    private sealed class SuccessfulBootstrapper : IJsonsBootstrapper
    {
        public Task<Response<JsonsBootstrapPayload>> EnsureWorkspaceAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Response<JsonsBootstrapPayload>.Ok(
                    new JsonsBootstrapPayload()));
        }
    }

    private sealed class SuccessfulContextProvider
        : IErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadFromJsonsAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Response<ErrorCatalogContext>.Ok(
                    new ErrorCatalogContext()));
        }
    }

    private sealed class ThrowingSetContextStore : IErrorCatalogContextStore
    {
        public bool IsInitialized =>
            throw new InvalidOperationException("Unexpected IsInitialized access.");

        public ErrorCatalogContext? Current =>
            throw new InvalidOperationException("Unexpected Current access.");

        public Response<ErrorCatalogContext> GetCurrent()
        {
            throw new InvalidOperationException("Unexpected GetCurrent call.");
        }

        public void Set(ErrorCatalogContext context)
        {
            throw new InvalidOperationException(
                "Sensitive initializer context store Set detail must not escape.");
        }
    }
}
