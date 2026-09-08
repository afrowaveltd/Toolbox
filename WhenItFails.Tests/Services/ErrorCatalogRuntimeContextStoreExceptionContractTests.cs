using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Services;

namespace Afrowave.Toolbox.WhenItFails.Tests.Services;

public sealed class ErrorCatalogRuntimeContextStoreExceptionContractTests
{
    [Fact]
    public void GetCurrentContext_WhenStoreThrows_ReturnsStableFailure()
    {
        ErrorCatalogRuntime runtime = new(
            new UnusedInitializer(),
            new WhenItFailsOptions(),
            new ThrowingContextStore(),
            new UnusedBuiltInContextProvider(),
            new UnusedDescriptorService(),
            new UnusedProfileSelectionService());

        Response<ErrorCatalogContext> response =
            runtime.GetCurrentContext();

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

    private sealed class ThrowingContextStore : IErrorCatalogContextStore
    {
        public bool IsInitialized => true;

        public ErrorCatalogContext? Current =>
            throw new InvalidOperationException("Unexpected Current access.");

        public Response<ErrorCatalogContext> GetCurrent()
        {
            throw new InvalidOperationException(
                "Sensitive runtime context store detail must not escape.");
        }

        public void Set(ErrorCatalogContext context)
        {
            throw new InvalidOperationException("Unexpected Set call.");
        }
    }

    private sealed class UnusedInitializer : IErrorCatalogInitializer
    {
        public Task<Response<ErrorCatalogInitializationPayload>> InitializeAsync(
            JsonsOptions options,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Unexpected initializer call.");
        }
    }

    private sealed class UnusedBuiltInContextProvider
        : IBuiltInErrorCatalogContextProvider
    {
        public Task<Response<ErrorCatalogContext>> LoadAsync(
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "Unexpected built-in context provider call.");
        }
    }

    private sealed class UnusedDescriptorService : IErrorDescriptorService
    {
        public Response<ErrorDescriptor> FromId(
            ErrorCatalogContext? context,
            string errorId)
        {
            throw new InvalidOperationException("Unexpected FromId call.");
        }

        public Response<ErrorDescriptor> FromName(
            ErrorCatalogContext? context,
            string errorName)
        {
            throw new InvalidOperationException("Unexpected FromName call.");
        }

        public Response<ErrorDescriptor> FromCode(
            ErrorCatalogContext? context,
            int code)
        {
            throw new InvalidOperationException("Unexpected FromCode call.");
        }
    }

    private sealed class UnusedProfileSelectionService
        : IErrorProfileSelectionService
    {
        public Response<IReadOnlyList<ErrorDefinition>> ResolveByProfileName(
            ErrorCatalogContext? context,
            string profileName)
        {
            throw new InvalidOperationException(
                "Unexpected profile selection service call.");
        }
    }
}
