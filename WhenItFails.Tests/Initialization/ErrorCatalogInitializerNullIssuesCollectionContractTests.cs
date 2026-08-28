using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;

namespace Afrowave.Toolbox.WhenItFails.Tests.Initialization;

public sealed class ErrorCatalogInitializerNullIssuesCollectionContractTests
{
    [Fact]
    public async Task InitializeAsync_ShouldUseFallbackCode_WhenBootstrapperIssuesCollectionIsNull()
    {
        ErrorCatalogInitializer initializer = new(
            new NullIssuesBootstrapper(),
            new ThrowingContextProvider(),
            new ThrowingContextStore());

        Response<ErrorCatalogInitializationPayload> response =
            await initializer.InitializeAsync(new JsonsOptions());

        Assert.False(response.IsSuccess);
        Assert.Equal(ResultStatus.Failed, response.Status);
        Assert.Null(response.Data);
        Assert.Equal(
            "The WhenItFails JSON workspace could not be prepared.",
            response.Message);

        var issue = Assert.Single(response.Issues);
        Assert.Equal("ErrorCatalogBootstrapFailed", issue.Code);
        Assert.Equal(
            "The WhenItFails JSON workspace could not be prepared.",
            issue.Message);
    }

    private sealed class NullIssuesBootstrapper : IJsonsBootstrapper
    {
        public Task<Response<JsonsBootstrapPayload>> EnsureWorkspaceAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            Response<JsonsBootstrapPayload> response = new()
            {
                Status = ResultStatus.Failed,
                Message = string.Empty,
                Issues = null!
            };

            return Task.FromResult(response);
        }
    }

    private sealed class ThrowingContextProvider : IErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadFromJsonsAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "Context provider must not run after bootstrap failure.");
        }
    }

    private sealed class ThrowingContextStore : IErrorCatalogContextStore
    {
        public bool IsInitialized => false;

        public ErrorCatalogContext? Current => null;

        public Response<ErrorCatalogContext> GetCurrent()
        {
            throw new InvalidOperationException(
                "Context store must not be read during initialization.");
        }

        public void Set(ErrorCatalogContext context)
        {
            throw new InvalidOperationException(
                "Context store must not be written after bootstrap failure.");
        }
    }
}
